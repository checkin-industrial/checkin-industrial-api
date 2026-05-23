using System.Globalization;
using System.Text;
using AppTurismoIndustrial.Api.Application.DTOs.Import;
using AppTurismoIndustrial.Api.Domain.Entities;
using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Controllers;

[ApiController]
[Route("api/import")]
public class ImportacaoPontosInstitucionaisController : ControllerBase
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");
    private readonly AppDbContext _context;
    private readonly ILogger<ImportacaoPontosInstitucionaisController> _logger;
    private const long MaxFileSize = 20 * 1024 * 1024; // 20 MB

    public ImportacaoPontosInstitucionaisController(
        AppDbContext context,
        ILogger<ImportacaoPontosInstitucionaisController> logger)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _context = context;
        _logger = logger;
    }

    [HttpGet("pontos-institucionais/exportar")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarPontosInstitucionaisCsv(CancellationToken cancellationToken = default)
    {
        var pontos = await _context.PontosInstitucionais
            .AsNoTracking()
            .OrderBy(p => p.OrdemExibicao ?? 0)
            .ThenBy(p => p.Nome)
            .ToListAsync(cancellationToken);

        var conteudo = await GerarCsvPontosInstitucionaisAsync(pontos, new UTF8Encoding(true), cancellationToken);
        var fileName = $"cadastro-pontos-institucionais-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(conteudo, "text/csv; charset=utf-8", fileName);
    }

    [HttpGet("pontos-institucionais/exportar-ansi")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarPontosInstitucionaisCsvAnsi(CancellationToken cancellationToken = default)
    {
        var pontos = await _context.PontosInstitucionais
            .AsNoTracking()
            .OrderBy(p => p.OrdemExibicao ?? 0)
            .ThenBy(p => p.Nome)
            .ToListAsync(cancellationToken);

        var conteudo = await GerarCsvPontosInstitucionaisAsync(pontos, Encoding.GetEncoding(1252), cancellationToken);
        var fileName = $"cadastro-pontos-institucionais-ansi-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(conteudo, "text/csv; charset=windows-1252", fileName);
    }

    [HttpPost("pontos-institucionais")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PontoInstitucionalImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<PontoInstitucionalImportResult>> ImportarPontosInstitucionais(
        [FromForm] ImportacaoPontosInstitucionaisRequest request,
        CancellationToken cancellationToken = default)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { erro = "Nenhum arquivo foi fornecido." });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { erro = "Arquivo muito grande. Tamanho maximo: 20 MB." });
        }

        var extensao = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extensao != ".csv")
        {
            return StatusCode(StatusCodes.Status415UnsupportedMediaType,
                new { erro = "Formato de arquivo nao suportado. Use CSV." });
        }

        var resultado = new PontoInstitucionalImportResult();

        try
        {
            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";",
                BadDataFound = null,
                MissingFieldFound = null
            };

            using var csv = new CsvReader(reader, config);

            if (!await csv.ReadAsync())
            {
                return Ok(resultado);
            }

            csv.ReadHeader();
            var lineNumber = 1;

            while (await csv.ReadAsync())
            {
                lineNumber++;
                resultado.TotalRecords++;

                var validationErrors = new List<PontoInstitucionalImportError>();

                var idValue = GetField(csv, "Id");
                var nome = GetField(csv, "Nome");
                var tipoText = GetField(csv, "Tipo");
                var descricao = GetField(csv, "Descricao");
                var endereco = GetField(csv, "Endereco");
                var latitudeText = GetField(csv, "Latitude");
                var longitudeText = GetField(csv, "Longitude");

                if (string.IsNullOrWhiteSpace(nome))
                {
                    validationErrors.Add(new PontoInstitucionalImportError { LineNumber = lineNumber, FieldName = "Nome", Message = "Nome e obrigatorio." });
                }

                if (string.IsNullOrWhiteSpace(descricao))
                {
                    validationErrors.Add(new PontoInstitucionalImportError { LineNumber = lineNumber, FieldName = "Descricao", Message = "Descricao e obrigatoria." });
                }

                if (string.IsNullOrWhiteSpace(endereco))
                {
                    validationErrors.Add(new PontoInstitucionalImportError { LineNumber = lineNumber, FieldName = "Endereco", Message = "Endereco e obrigatorio." });
                }

                var tipo = ParseTipo(tipoText);
                if (!tipo.HasValue)
                {
                    validationErrors.Add(new PontoInstitucionalImportError { LineNumber = lineNumber, FieldName = "Tipo", Message = "Tipo invalido." });
                }

                var latitude = ParseNullableDecimal(latitudeText);
                if (!latitude.HasValue || latitude < -90m || latitude > 90m)
                {
                    validationErrors.Add(new PontoInstitucionalImportError { LineNumber = lineNumber, FieldName = "Latitude", Message = "Latitude deve estar entre -90 e 90." });
                }

                var longitude = ParseNullableDecimal(longitudeText);
                if (!longitude.HasValue || longitude < -180m || longitude > 180m)
                {
                    validationErrors.Add(new PontoInstitucionalImportError { LineNumber = lineNumber, FieldName = "Longitude", Message = "Longitude deve estar entre -180 e 180." });
                }

                if (validationErrors.Count > 0)
                {
                    resultado.Skipped++;
                    resultado.Errors.AddRange(validationErrors);
                    continue;
                }

                var id = Guid.TryParse(idValue, out var parsedId) ? parsedId : (Guid?)null;

                PontoInstitucional? pontoExistente;
                if (id.HasValue)
                {
                    pontoExistente = await _context.PontosInstitucionais.FirstOrDefaultAsync(p => p.Id == id.Value, cancellationToken);
                }
                else
                {
                    pontoExistente = await _context.PontosInstitucionais.FirstOrDefaultAsync(
                        p => p.Nome == nome && p.Endereco == endereco && p.Tipo == tipo!.Value,
                        cancellationToken);
                }

                if (pontoExistente == null)
                {
                    pontoExistente = new PontoInstitucional
                    {
                        Id = id ?? Guid.NewGuid()
                    };
                    _context.PontosInstitucionais.Add(pontoExistente);
                    resultado.Inserted++;
                }
                else
                {
                    resultado.Updated++;
                }

                pontoExistente.Nome = nome!.Trim();
                pontoExistente.Tipo = tipo!.Value;
                pontoExistente.Descricao = descricao!.Trim();
                pontoExistente.Endereco = endereco!.Trim();
                pontoExistente.Latitude = latitude!.Value;
                pontoExistente.Longitude = longitude!.Value;
                pontoExistente.AtividadesDisponiveis = NormalizeNullable(GetField(csv, "Atividades_Disponiveis"));
                pontoExistente.EquipeGestao = NormalizeNullable(GetField(csv, "Equipe_Gestao"));
                pontoExistente.ContatoNome = NormalizeNullable(GetField(csv, "Contato_Nome"));
                pontoExistente.ContatoTelefone = NormalizeNullable(GetField(csv, "Contato_Telefone"));
                pontoExistente.ContatoEmail = NormalizeNullable(GetField(csv, "Contato_Email"));
                pontoExistente.ResponsavelFotoUrl = NormalizeNullable(GetField(csv, "Responsavel_Foto_Url"));
                pontoExistente.LogoUrl = NormalizeNullable(GetField(csv, "Logo_Url"));
                pontoExistente.CardFotoUrl = NormalizeNullable(GetField(csv, "Card_Foto_Url"));
                pontoExistente.CorMarcador = NormalizeNullable(GetField(csv, "Cor_Marcador"));
                pontoExistente.IconeMarcador = NormalizeNullable(GetField(csv, "Icone_Marcador"));
                pontoExistente.OrdemExibicao = ParseNullableInt(GetField(csv, "Ordem_Exibicao"));
                pontoExistente.Ativo = ParseNullableBool(GetField(csv, "Ativo"));
            }

            await _context.SaveChangesAsync(cancellationToken);

            resultado.Status = resultado.Errors.Count == 0 ? "Completed" : "CompletedWithErrors";
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao importar pontos institucionais");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { erro = "Erro ao processar importacao de pontos institucionais." });
        }
    }

    private static async Task<byte[]> GerarCsvPontosInstitucionaisAsync(
        List<PontoInstitucional> pontos,
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
            csv.WriteField("Id");
            csv.WriteField("Nome");
            csv.WriteField("Tipo");
            csv.WriteField("Descricao");
            csv.WriteField("Endereco");
            csv.WriteField("Latitude");
            csv.WriteField("Longitude");
            csv.WriteField("Atividades_Disponiveis");
            csv.WriteField("Equipe_Gestao");
            csv.WriteField("Contato_Nome");
            csv.WriteField("Contato_Telefone");
            csv.WriteField("Contato_Email");
            csv.WriteField("Responsavel_Foto_Url");
            csv.WriteField("Logo_Url");
            csv.WriteField("Card_Foto_Url");
            csv.WriteField("Cor_Marcador");
            csv.WriteField("Icone_Marcador");
            csv.WriteField("Ordem_Exibicao");
            csv.WriteField("Ativo");
            await csv.NextRecordAsync();

            foreach (var ponto in pontos)
            {
                csv.WriteField(ponto.Id);
                csv.WriteField(ponto.Nome);
                csv.WriteField(TipoParaImportacao(ponto.Tipo));
                csv.WriteField(ponto.Descricao);
                csv.WriteField(ponto.Endereco);
                csv.WriteField(ponto.Latitude.ToString("0.######", PtBrCulture));
                csv.WriteField(ponto.Longitude.ToString("0.######", PtBrCulture));
                csv.WriteField(ponto.AtividadesDisponiveis);
                csv.WriteField(ponto.EquipeGestao);
                csv.WriteField(ponto.ContatoNome);
                csv.WriteField(ponto.ContatoTelefone);
                csv.WriteField(ponto.ContatoEmail);
                csv.WriteField(ponto.ResponsavelFotoUrl);
                csv.WriteField(ponto.LogoUrl);
                csv.WriteField(ponto.CardFotoUrl);
                csv.WriteField(ponto.CorMarcador);
                csv.WriteField(ponto.IconeMarcador);
                csv.WriteField(ponto.OrdemExibicao);
                csv.WriteField(ponto.Ativo);
                await csv.NextRecordAsync();
            }

            await writer.FlushAsync(cancellationToken);
        }

        stream.Position = 0;
        return stream.ToArray();
    }

    private static string GetField(CsvReader csv, string field)
    {
        try
        {
            return csv.GetField(field) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static TipoPontoInstitucional? ParseTipo(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return null;
        }

        var normalized = tipo.Trim().ToLowerInvariant();

        if (int.TryParse(normalized, out var intTipo) && Enum.IsDefined(typeof(TipoPontoInstitucional), intTipo))
        {
            return (TipoPontoInstitucional)intTipo;
        }

        return normalized switch
        {
            "educacao" => TipoPontoInstitucional.Educacao,
            "comercio" => TipoPontoInstitucional.Comercio,
            "financeiro" => TipoPontoInstitucional.Financeiro,
            "servico" => TipoPontoInstitucional.Servico,
            "servicos" => TipoPontoInstitucional.Servico,
            "setorprefeitura" => TipoPontoInstitucional.SetorPrefeitura,
            "setor_prefeitura" => TipoPontoInstitucional.SetorPrefeitura,
            "pontoturistico" => TipoPontoInstitucional.PontoTuristico,
            "ponto_turistico" => TipoPontoInstitucional.PontoTuristico,
            "hotel" => TipoPontoInstitucional.Hotel,
            "ecoturismo" => TipoPontoInstitucional.Ecoturismo,
            _ => null
        };
    }

    private static string TipoParaImportacao(TipoPontoInstitucional tipo)
    {
        return tipo switch
        {
            TipoPontoInstitucional.Educacao => "educacao",
            TipoPontoInstitucional.Comercio => "comercio",
            TipoPontoInstitucional.Financeiro => "financeiro",
            TipoPontoInstitucional.Servico => "servico",
            TipoPontoInstitucional.SetorPrefeitura => "setor_prefeitura",
            TipoPontoInstitucional.PontoTuristico => "ponto_turistico",
            TipoPontoInstitucional.Hotel => "hotel",
            _ => "ecoturismo"
        };
    }

    private static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().Replace(" ", string.Empty);

        var hasComma = normalized.Contains(',');
        var hasDot = normalized.Contains('.');

        if (hasComma && !hasDot)
        {
            if (decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, PtBrCulture, out var ptbr))
            {
                return ptbr;
            }
        }

        if (!hasComma && hasDot)
        {
            if (decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var inv))
            {
                return inv;
            }
        }

        if (decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, PtBrCulture, out var fallbackPtBr))
        {
            return fallbackPtBr;
        }

        if (decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var fallbackInv))
        {
            return fallbackInv;
        }

        return null;
    }

    private static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            || int.TryParse(normalized, NumberStyles.Integer, PtBrCulture, out parsed))
        {
            return parsed;
        }

        return null;
    }

    private static bool? ParseNullableBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "1" => true,
            "0" => false,
            "true" => true,
            "false" => false,
            "sim" => true,
            "nao" => false,
            "não" => false,
            "ativo" => true,
            "inativo" => false,
            _ => null
        };
    }

    private static string? NormalizeNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
