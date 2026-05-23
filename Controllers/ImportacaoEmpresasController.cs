using AppTurismoIndustrial.Api.Application.DTOs.Import;
using AppTurismoIndustrial.Api.Application.Services;
using AppTurismoIndustrial.Api.Domain.Entities;
using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace AppTurismoIndustrial.Api.Controllers;

/// <summary>
/// Controlador para importação de dados de empresas de múltiplas fontes.
/// Suporta upload de arquivos (CSV, JSON) e sincronização com APIs externas.
/// </summary>
[ApiController]
[Route("api/import")]
public class ImportacaoEmpresasController : ControllerBase
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");
    private readonly IImportacaoEmpresasService _importacaoService;
    private readonly AppDbContext _context;
    private readonly ILogger<ImportacaoEmpresasController> _logger;
    private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB

    public ImportacaoEmpresasController(
        IImportacaoEmpresasService importacaoService,
        AppDbContext context,
        ILogger<ImportacaoEmpresasController> logger)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _importacaoService = importacaoService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Exporta o cadastro atual de empresas em CSV compatível com o parser de importação.
    /// Fluxo suportado: baixar arquivo, editar e reimportar para atualização em demanda (upsert por CNPJ).
    /// </summary>
    [HttpGet("empresas/exportar")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarEmpresasCsv(CancellationToken cancellationToken = default)
    {
        var empresas = await _context.Empresas
            .AsNoTracking()
            .OrderBy(e => e.NomeFantasia)
            .ToListAsync(cancellationToken);

        var conteudo = await GerarCsvEmpresasAsync(
            empresas,
            new UTF8Encoding(true),
            cancellationToken);

        var fileName = $"cadastro-empresas-atual-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return File(conteudo, "text/csv; charset=utf-8", fileName);
    }

    /// <summary>
    /// Exporta o cadastro atual em CSV com codificação ANSI (Windows-1252),
    /// útil para abertura direta em versões legadas do Excel no Windows.
    /// </summary>
    [HttpGet("empresas/exportar-ansi")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarEmpresasCsvAnsi(CancellationToken cancellationToken = default)
    {
        var empresas = await _context.Empresas
            .AsNoTracking()
            .OrderBy(e => e.NomeFantasia)
            .ToListAsync(cancellationToken);

        var encoding = Encoding.GetEncoding(1252);
        var conteudo = await GerarCsvEmpresasAsync(
            empresas,
            encoding,
            cancellationToken);

        var fileName = $"cadastro-empresas-atual-ansi-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return File(conteudo, "text/csv; charset=windows-1252", fileName);
    }

    private async Task<byte[]> GerarCsvEmpresasAsync(
        List<Empresa> empresas,
        Encoding encoding,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await using (var writer = new StreamWriter(stream, encoding, leaveOpen: true))
        await using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        }))
        {
            csv.WriteField("CNPJ");
            csv.WriteField("Razao_Social");
            csv.WriteField("Nome_Fantasia");
            csv.WriteField("CNAE_Principal");
            csv.WriteField("Setor");
            csv.WriteField("Porte");
            csv.WriteField("Numero_Funcionarios");
            csv.WriteField("Endereco");
            csv.WriteField("Telefone");
            csv.WriteField("CEP");
            csv.WriteField("Municipio");
            csv.WriteField("Descricao_CNAE");
            csv.WriteField("Matriz_ou_Filial");
            csv.WriteField("Latitude");
            csv.WriteField("Longitude");
            csv.WriteField("Situacao_Cadastral");
            csv.WriteField("Data_Importacao");
            csv.WriteField("Fonte_Origem");
            await csv.NextRecordAsync();

            foreach (var empresa in empresas)
            {
                csv.WriteField(FormatCnpjForSpreadsheet(empresa.Cnpj));
                csv.WriteField(empresa.RazaoSocial);
                csv.WriteField(empresa.NomeFantasia);
                csv.WriteField(empresa.CnaePrincipal);
                csv.WriteField(SetorParaImportacao(empresa.Setor));
                csv.WriteField(PorteParaImportacao(empresa.Porte));
                csv.WriteField(empresa.NumeroFuncionarios);
                csv.WriteField(empresa.Endereco);
                csv.WriteField(empresa.Telefone);
                csv.WriteField(empresa.Cep);
                csv.WriteField(empresa.Municipio);
                csv.WriteField(empresa.DescricaoCnae);
                csv.WriteField(MatrizOuFilialParaImportacao(empresa.MatrizOuFilial));
                csv.WriteField(FormatCoordinateForSpreadsheet(empresa.Latitude));
                csv.WriteField(FormatCoordinateForSpreadsheet(empresa.Longitude));
                csv.WriteField(SituacaoParaImportacao(empresa.SituacaoCadastral));
                csv.WriteField(empresa.DataCadastro.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
                csv.WriteField("Exportacao_Cadastro_Atual");
                await csv.NextRecordAsync();
            }

            await writer.FlushAsync(cancellationToken);
        }

        stream.Position = 0;
        return stream.ToArray();
    }

    /// <summary>
    /// Importa empresas a partir de um arquivo CSV ou JSON.
    /// </summary>
    /// <param name="request">Payload multipart contendo o arquivo a importar (CSV ou JSON).</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>
    /// 200 OK: Importação concluda (síncrona).
    /// 202 Accepted: Importação iniciada (assíncrona, para grandes volumes).
    /// 400 Bad Request: Arquivo ou formato inválido.
    /// 415 Unsupported Media Type: Tipo de arquivo não suportado.
    /// </returns>
    [HttpPost("empresas")]
    [ProducesResponseType(typeof(EmpresaImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<EmpresaImportResult>> ImportarEmpresas(
        [FromForm] ImportacaoEmpresasRequest request,
        CancellationToken cancellationToken = default)
    {
        var file = request.File;

        // Valida arquivo
        var validacao = ValidarArquivo(file);
        if (!validacao.Valido)
        {
            _logger.LogWarning("Arquivo rejeitado: {Motivo}", validacao.Motivo);
            return BadRequest(new { erro = validacao.Motivo });
        }

        // Identifica formato
        var formato = IdentificarFormato(file.FileName, file.ContentType);
        if (string.IsNullOrEmpty(formato))
        {
            _logger.LogWarning("Formato não reconhecido: {FileName}", file.FileName);
            return StatusCode(
                StatusCodes.Status415UnsupportedMediaType,
                new { erro = "Formato de arquivo não suportado. Use CSV ou JSON." });
        }

        try
        {
            _logger.LogInformation("Iniciando importação: {FileName} ({Tamanho} bytes, Formato: {Formato})",
                file.FileName,
                file.Length,
                formato);

            // Abre stream do arquivo
            await using var stream = file.OpenReadStream();

            // Executa importação
            var resultado = await _importacaoService.ImportarAsync(
                stream,
                formato,
                cancellationToken);

            // Define status HTTP baseado no resultado
            var statusCode = resultado.Status switch
            {
                "Completed" => StatusCodes.Status200OK,
                "CompletedWithErrors" => StatusCodes.Status200OK, // Retorna 200 mesmo com erros parciais
                "InProgress" => StatusCodes.Status202Accepted,
                "Pending" => StatusCodes.Status202Accepted,
                _ => StatusCodes.Status500InternalServerError
            };

            _logger.LogInformation(
                "Importação concluda: {Total} registros, {Inseridos} inseridos, {Atualizados} atualizados, {Ignorados} ignorados, {Erros} erros",
                resultado.TotalRecords,
                resultado.Inserted,
                resultado.Updated,
                resultado.Skipped,
                resultado.Errors.Count);

            return StatusCode(statusCode, resultado);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Importação cancelada pelo usuário");
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new { erro = "Importação cancelada." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante importação de empresas");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { erro = "Erro ao processar importação. Tente novamente mais tarde." });
        }
    }

    /// <summary>
    /// Identifica o formato do arquivo baseado na extensão e tipo MIME.
    /// </summary>
    private string? IdentificarFormato(string nomeArquivo, string contentType)
    {
        // Check por extensão
        var extensao = Path.GetExtension(nomeArquivo).ToLowerInvariant();
        if (extensao == ".csv" || contentType.Contains("csv"))
        {
            return "CSV";
        }

        if (extensao == ".json" || contentType.Contains("json"))
        {
            return "JSON";
        }

        return null;
    }

    /// <summary>
    /// Valida se o arquivo é aceitável para importação.
    /// </summary>
    private (bool Valido, string Motivo) ValidarArquivo(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "Nenhum arquivo foi fornecido.");
        }

        if (file.Length > MaxFileSize)
        {
            return (false, $"Arquivo muito grande. Tamanho máximo: {MaxFileSize / 1024 / 1024} MB.");
        }

        // Valida extensão
        var extensao = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extensao != ".csv" && extensao != ".json")
        {
            return (false, "Apenas arquivos CSV e JSON são aceitos.");
        }

        return (true, string.Empty);
    }

    private static string SetorParaImportacao(SetorEmpresa setor)
    {
        return setor switch
        {
            SetorEmpresa.Industria => "industria",
            SetorEmpresa.Comercio => "comercio",
            _ => "servicos"
        };
    }

    private static string PorteParaImportacao(PorteEmpresa porte)
    {
        return porte switch
        {
            PorteEmpresa.Mei => "MEI",
            PorteEmpresa.Me => "ME",
            PorteEmpresa.Epp => "EPP",
            PorteEmpresa.Ltda => "LTDA",
            _ => "SA"
        };
    }

    private static string SituacaoParaImportacao(SituacaoCadastral situacao)
    {
        return situacao switch
        {
            SituacaoCadastral.Ativa => "ativa",
            SituacaoCadastral.Inativa => "inativa",
            SituacaoCadastral.Suspensa => "suspensa",
            _ => "baixada"
        };
    }

    private static string MatrizOuFilialParaImportacao(MatrizOuFilialEmpresa matrizOuFilial)
    {
        return matrizOuFilial switch
        {
            MatrizOuFilialEmpresa.Matriz => "Matriz",
            _ => "Filial"
        };
    }

    private static string FormatCnpjForSpreadsheet(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
        {
            return string.Empty;
        }

        // Prefixo de apóstrofo força Excel a manter CNPJ como texto e evita notação científica.
        return $"'{cnpj.Trim()}";
    }

    private static string FormatCoordinateForSpreadsheet(decimal coordinate)
    {
        // Excel em pt-BR interpreta corretamente vírgula como separador decimal.
        return coordinate.ToString("0.######", PtBrCulture);
    }
}
