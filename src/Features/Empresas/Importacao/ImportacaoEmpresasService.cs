

using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AppTurismoIndustrial.Api.Features.Empresas.Importacao;

/// <summary>
/// Serviço de importação de empresas com suporte a múltiplos formatos e grandes volumes.
/// Implementa validação, deduplicação e persistência em batches com logging detalhado.
/// </summary>
/// <remarks>
/// <para>
/// <b>Excecao consciente ao padrao "exceptions over bool":</b> os helpers <c>ValidarRegistro</c>,
/// <c>ConvertToEmpresa</c> e <c>AtualizarEmpresa</c> retornam bool/nullable em vez de lancar
/// excecoes. Isso e proposital — o pipeline de importacao processa em batch e ACUMULA erros
/// (em <c>resultado.Errors</c>) para reportar todas as falhas de uma vez, sem interromper as
/// linhas validas. Trocar por excecoes forcaria try/catch por linha (custo de perf + ruido
/// visual) ou quebraria o contrato de "best-effort batch". Para erros sistemicos (DB, IO),
/// continuamos usando o bloco catch externo + erro "SystemError" no resultado.
/// </para>
/// </remarks>
public class ImportacaoEmpresasService : IImportacaoEmpresasService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ImportacaoEmpresasService> _logger;
    private readonly int _batchSize;

    public ImportacaoEmpresasService(
        AppDbContext context,
        ILogger<ImportacaoEmpresasService> logger,
        int? batchSize = null,
        IOptions<LimitsOptions>? limits = null)
    {
        _context = context;
        _logger = logger;
        // Prioridade: override explicito (batchSize) > config (Limits:DefaultBatchSize) > default da classe.
        _batchSize = batchSize ?? (limits?.Value ?? new LimitsOptions()).DefaultBatchSize;
    }

    public async Task<EmpresaImportResult> ImportarAsync(
        Stream stream,
        string formato,
        CancellationToken cancellationToken = default)
    {
        // Identifica e cria parser apropriado
        var parser = CriarParser(formato);
        if (parser == null)
        {
            return CriarResultadoErro($"Formato '{formato}' não suportado", formato);
        }

        // Processa stream do parser
        var registros = parser.ParseAsync(stream, cancellationToken);
        return await ImportarRegistrosAsync(registros, formato, cancellationToken);
    }

    public async Task<EmpresaImportResult> ImportarRegistrosAsync(
        IAsyncEnumerable<EmpresaImportRecord> registros,
        string nomeOrigem,
        CancellationToken cancellationToken = default)
    {
        var resultado = new EmpresaImportResult
        {
            StartedAt = DateTime.UtcNow,
            ImportSource = nomeOrigem
        };

        var duplicatasNoBatch = 0;
        var atualizacoesNoBanco = 0;
        var errosValidacao = 0;
        var loteAtual = 0;

        try
        {
            _logger.LogInformation("Iniciando importação de {Origem} com batch size de {BatchSize} registros",
                nomeOrigem, _batchSize);

            var lote = new List<EmpresaImportRecord>(_batchSize);
            var cnpjsProcessados = new HashSet<string>();

            await foreach (var registro in registros.WithCancellation(cancellationToken))
            {
                resultado.TotalRecords++;

                // Valida registro
                var validacao = ValidarRegistro(registro);
                if (!validacao.Valido)
                {
                    resultado.Skipped++;
                    resultado.Errors.Add(validacao.Erro);
                    errosValidacao++;
                    
                    if (resultado.TotalRecords % 10000 == 0)
                    {
                        _logger.LogWarning("Progresso: {Total} registros processados, {Erros} erros de validação",
                            resultado.TotalRecords, errosValidacao);
                    }
                    continue;
                }

                // Normaliza CNPJ para apenas 14 dígitos (remove pontos, barras e traços)
                registro.Cnpj = System.Text.RegularExpressions.Regex.Replace(registro.Cnpj, @"\D", "");

                // Verifica duplicatas no lote atual
                if (cnpjsProcessados.Contains(registro.Cnpj))
                {
                    resultado.Skipped++;
                    duplicatasNoBatch++;
                    resultado.Errors.Add(new EmpresaImportError
                    {
                        RecordId = registro.RecordId,
                        Cnpj = registro.Cnpj,
                        ErrorType = "DuplicateInBatch",
                        Message = $"CNPJ duplicado dentro do lote de importação.",
                        FieldName = "Cnpj",
                        Stage = "Validation"
                    });
                    continue;
                }

                cnpjsProcessados.Add(registro.Cnpj);
                lote.Add(registro);

                // Processa lote quando atinge o tamanho máximo
                if (lote.Count >= _batchSize)
                {
                    loteAtual++;
                    var (inseridos, atualizados, erros) = await ProcessarLoteAsync(
                        lote,
                        loteAtual,
                        cancellationToken);

                    resultado.Inserted += inseridos;
                    resultado.Updated += atualizados;
                    resultado.Skipped += erros.Count;
                    resultado.Errors.AddRange(erros);
                    atualizacoesNoBanco += atualizados;
                    
                    _logger.LogInformation(
                        "Lote {NumLote} processado: {TotalRegistros} registros, {Inseridos} inseridos, {Atualizados} atualizados, {Erros} erros. Total geral: {TotalGeral}",
                        loteAtual,
                        lote.Count,
                        inseridos,
                        atualizados,
                        erros.Count,
                        resultado.TotalRecords);

                    lote.Clear();
                    cnpjsProcessados.Clear();
                }
            }

            // Processa último lote se houver registros pendentes
            if (lote.Count > 0)
            {
                loteAtual++;
                var (inseridos, atualizados, erros) = await ProcessarLoteAsync(
                    lote,
                    loteAtual,
                    cancellationToken);

                resultado.Inserted += inseridos;
                resultado.Updated += atualizados;
                resultado.Skipped += erros.Count;
                resultado.Errors.AddRange(erros);
                atualizacoesNoBanco += atualizados;
                
                _logger.LogInformation(
                    "Último lote {NumLote} processado: {TotalRegistros} registros, {Inseridos} inseridos, {Atualizados} atualizados, {Erros} erros. Total final: {TotalGeral}",
                    loteAtual,
                    lote.Count,
                    inseridos,
                    atualizados,
                    erros.Count,
                    resultado.TotalRecords);
            }

            resultado.Status = resultado.Errors.Count == 0 ? "Completed" : "CompletedWithErrors";
            
            _logger.LogInformation(
                "Importação concluída em {DurationMs}ms: {Total} registros, {Inseridos} inseridos, {Atualizados} atualizados, " +
                "{Ignorados} ignorados ({DuplicatasLote} no batch, {Validacao} validação). " +
                "Taxa de sucesso: {SuccessRate:F2}%",
                resultado.DurationMs,
                resultado.TotalRecords,
                resultado.Inserted,
                resultado.Updated,
                resultado.Skipped,
                duplicatasNoBatch,
                errosValidacao,
                resultado.SuccessRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro geral na importação de empresas");
            resultado.Status = "Failed";
            resultado.Message = ex.Message;
            resultado.Errors.Add(new EmpresaImportError
            {
                RecordId = "GLOBAL",
                ErrorType = "SystemError",
                Message = ex.Message,
                Stage = "Execution"
            });
        }
        finally
        {
            resultado.CompletedAt = DateTime.UtcNow;
        }

        return resultado;
    }

    /// <summary>
    /// Processa um lote de registros: insere novos CNPJs e atualiza os existentes.
    /// </summary>
    private async Task<(int inseridos, int atualizados, List<EmpresaImportError> erros)> ProcessarLoteAsync(
        List<EmpresaImportRecord> lote,
        int numeroLote,
        CancellationToken cancellationToken)
    {
        var inicioLote = DateTime.UtcNow;
        var inseridos = 0;
        var atualizados = 0;
        var errosConversao = 0;
        var erros = new List<EmpresaImportError>();
        var empresasParaInserir = new List<Empresa>();

        _logger.LogDebug("Iniciando processamento do lote {NumLote} com {QtdRegistros} registros",
            numeroLote, lote.Count);

        // Carrega empresas que já existem no banco para este lote.
        var cnpjsNoLote = lote.Select(r => r.Cnpj).ToList();
        var empresasExistentes = await _context.Empresas
            .Where(e => e.Cnpj != null && cnpjsNoLote.Contains(e.Cnpj))
            .ToListAsync(cancellationToken);
        var empresasPorCnpj = empresasExistentes.ToDictionary(e => e.Cnpj!, e => e);

        _logger.LogDebug("Lote {NumLote}: {QtdExistentes} CNPJs já existem no banco e serão atualizados",
            numeroLote, empresasExistentes.Count);

        // Processa cada registro do lote
        foreach (var registro in lote)
        {
            if (empresasPorCnpj.TryGetValue(registro.Cnpj, out var empresaExistente))
            {
                if (AtualizarEmpresa(empresaExistente, registro))
                {
                    atualizados++;
                }
                else
                {
                    errosConversao++;
                    erros.Add(new EmpresaImportError
                    {
                        RecordId = registro.RecordId,
                        Cnpj = registro.Cnpj,
                        ErrorType = "ConversionError",
                        Message = "Falha ao converter registro para atualização da entidade Empresa.",
                        Stage = "Normalization"
                    });
                }
                continue;
            }

            // Converte record para entidade Empresa
            var empresa = ConvertToEmpresa(registro);
            if (empresa != null)
            {
                empresasParaInserir.Add(empresa);
            }
            else
            {
                errosConversao++;
                erros.Add(new EmpresaImportError
                {
                    RecordId = registro.RecordId,
                    Cnpj = registro.Cnpj,
                    ErrorType = "ConversionError",
                    Message = "Falha ao converter registro para entidade Empresa.",
                    Stage = "Normalization"
                });
            }
        }

        // Persiste inserções e atualizações do lote.
        if (empresasParaInserir.Count > 0 || atualizados > 0)
        {
            try
            {
                var duracao = DateTime.UtcNow - inicioLote;
                _logger.LogDebug("Lote {NumLote}: Preparado para inserção. {QtdLinhas} registros prontos, " +
                    "{Atualizados} atualizações pendentes, {Erros} erros de conversão. Tempo: {DuracaoMs}ms",
                    numeroLote, empresasParaInserir.Count, atualizados, errosConversao, duracao.TotalMilliseconds);

                if (empresasParaInserir.Count > 0)
                {
                    _context.Empresas.AddRange(empresasParaInserir);
                }

                await _context.SaveChangesAsync(cancellationToken);
                inseridos = empresasParaInserir.Count;

                var duracaoTotal = DateTime.UtcNow - inicioLote;
                var processadosComSucesso = inseridos + atualizados;
                var taxaThroughput = processadosComSucesso / (duracaoTotal.TotalSeconds > 0 ? duracaoTotal.TotalSeconds : 1);
                
                _logger.LogInformation(
                    "Lote {NumLote}: {Inseridos} empresas inseridas e {Atualizados} atualizadas com sucesso. " +
                    "Throughput: {Taxa:.0f} registros/seg. Tempo total: {DuracaoMs}ms",
                    numeroLote, inseridos, atualizados, taxaThroughput, duracaoTotal.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inserir lote {NumLote} de empresas. Tentou inserir {Qtd} registros",
                    numeroLote, empresasParaInserir.Count);
                erros.Add(new EmpresaImportError
                {
                    RecordId = "BATCH",
                    ErrorType = "DatabaseError",
                    Message = $"Erro ao gravar lote: {ex.Message}",
                    Stage = "Persistence"
                });
            }
        }
        else
        {
            _logger.LogWarning("Lote {NumLote}: Nenhum registro válido para persistência. " +
                "{Atualizados} atualizados, {Erros} erros de conversão",
                numeroLote, atualizados, errosConversao);
        }

        return (inseridos, atualizados, erros);
    }

    /// <summary>
    /// Valida um registro importado antes de processar.
    /// </summary>
    private (bool Valido, EmpresaImportError Erro) ValidarRegistro(EmpresaImportRecord registro)
    {
        // Valida CNPJ obrigatório e formato
        if (string.IsNullOrWhiteSpace(registro.Cnpj))
        {
            return (false, new EmpresaImportError
            {
                RecordId = registro.RecordId,
                ErrorType = "ValidationError",
                Message = "CNPJ é obrigatório.",
                FieldName = "Cnpj",
                Stage = "Validation"
            });
        }

        if (!CnpjValidator.IsValid(registro.Cnpj))
        {
            return (false, new EmpresaImportError
            {
                RecordId = registro.RecordId,
                Cnpj = registro.Cnpj,
                ErrorType = "ValidationError",
                Message = "CNPJ inválido (formato esperado: 14 dígitos).",
                FieldName = "Cnpj",
                InvalidValue = registro.Cnpj,
                Stage = "Validation"
            });
        }

        // Valida campos obrigatórios
        if (string.IsNullOrWhiteSpace(registro.RazaoSocial))
        {
            return (false, new EmpresaImportError
            {
                RecordId = registro.RecordId,
                Cnpj = registro.Cnpj,
                ErrorType = "ValidationError",
                Message = "Razão Social é obrigatória.",
                FieldName = "RazaoSocial",
                Stage = "Validation"
            });
        }

        // Valida coordenadas se fornecidas
        if (registro.Latitude.HasValue && (registro.Latitude < -90 || registro.Latitude > 90))
        {
            return (false, new EmpresaImportError
            {
                RecordId = registro.RecordId,
                Cnpj = registro.Cnpj,
                ErrorType = "ValidationError",
                Message = "Latitude deve estar entre -90 e 90.",
                FieldName = "Latitude",
                InvalidValue = registro.Latitude.ToString(),
                Stage = "Validation"
            });
        }

        if (registro.Longitude.HasValue && (registro.Longitude < -180 || registro.Longitude > 180))
        {
            return (false, new EmpresaImportError
            {
                RecordId = registro.RecordId,
                Cnpj = registro.Cnpj,
                ErrorType = "ValidationError",
                Message = "Longitude deve estar entre -180 e 180.",
                FieldName = "Longitude",
                InvalidValue = registro.Longitude.ToString(),
                Stage = "Validation"
            });
        }

        return (true, null!);
    }

    /// <summary>
    /// Converte um EmpresaImportRecord em entidade Empresa.
    /// </summary>
    private Empresa? ConvertToEmpresa(EmpresaImportRecord record)
    {
        try
        {
            var empresa = new Empresa
            {
                Id = Guid.NewGuid(),
                Cnpj = record.Cnpj
            };

            return AtualizarEmpresa(empresa, record) ? empresa : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao converter registro {Cnpj}", record.Cnpj);
            return null;
        }
    }

    private bool AtualizarEmpresa(Empresa empresa, EmpresaImportRecord record)
    {
        var setor = ParseSetor(record.Setor);
        var porte = ParsePorte(record.Porte);
        var situacao = ParseSituacao(record.SituacaoCadastral);
        var matrizOuFilial = ParseMatrizOuFilial(record.MatrizOuFilial);

        if (!setor.HasValue || !porte.HasValue || !situacao.HasValue || !matrizOuFilial.HasValue)
        {
            return false;
        }

        empresa.Cnpj = record.Cnpj;
        empresa.RazaoSocial = record.RazaoSocial;
        empresa.NomeFantasia = record.NomeFantasia ?? string.Empty;
        empresa.CnaePrincipal = record.CnaePrincipal ?? string.Empty;
        empresa.Setor = setor.Value;
        empresa.Porte = porte.Value;
        empresa.NumeroFuncionarios = record.NumeroFuncionarios;
        empresa.Endereco = record.Endereco ?? string.Empty;
        empresa.Telefone = NullIfWhiteSpace(record.Telefone);
        empresa.Cep = NullIfWhiteSpace(record.Cep);
        empresa.Municipio = record.Municipio ?? string.Empty;
        empresa.DescricaoCnae = record.DescricaoCnae ?? string.Empty;
        empresa.MatrizOuFilial = matrizOuFilial.Value;
        empresa.Latitude = record.Latitude ?? 0;
        empresa.Longitude = record.Longitude ?? 0;
        empresa.SituacaoCadastral = situacao.Value;
        empresa.DataCadastro = EnsureUtc(record.DataImportacao) ?? empresa.DataCadastro;

        return true;
    }

    private static DateTime? EnsureUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        var date = value.Value;

        if (date.Kind == DateTimeKind.Utc)
        {
            return date;
        }

        if (date.Kind == DateTimeKind.Local)
        {
            return date.ToUniversalTime();
        }

        // Datas sem Kind (comum em CSV/Excel) são tratadas como horário local antes de converter para UTC.
        return DateTime.SpecifyKind(date, DateTimeKind.Local).ToUniversalTime();
    }

    private SetorEmpresa? ParseSetor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return SetorEmpresa.Servicos;
        return value.ToLowerInvariant() switch
        {
            "industria" => SetorEmpresa.Industria,
            "comercio" => SetorEmpresa.Comercio,
            "servicos" => SetorEmpresa.Servicos,
            _ => null
        };
    }

    private PorteEmpresa? ParsePorte(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return PorteEmpresa.Me;
        return value.ToUpperInvariant() switch
        {
            "MEI" => PorteEmpresa.Mei,
            "ME" => PorteEmpresa.Me,
            "EPP" => PorteEmpresa.Epp,
            "LTDA" => PorteEmpresa.Ltda,
            "SA" => PorteEmpresa.Sa,
            _ => null
        };
    }

    private SituacaoCadastral? ParseSituacao(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return SituacaoCadastral.Ativa;
        return value.ToLowerInvariant() switch
        {
            "ativa" => SituacaoCadastral.Ativa,
            "inativa" => SituacaoCadastral.Inativa,
            "suspensa" => SituacaoCadastral.Suspensa,
            "baixada" => SituacaoCadastral.Baixada,
            _ => null
        };
    }

    private MatrizOuFilialEmpresa? ParseMatrizOuFilial(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return MatrizOuFilialEmpresa.Matriz;
        return value.Trim().ToLowerInvariant() switch
        {
            "matriz" => MatrizOuFilialEmpresa.Matriz,
            "filial" => MatrizOuFilialEmpresa.Filial,
            _ => null
        };
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private IEmpresaImportParser? CriarParser(string formato)
    {
        return formato.ToUpperInvariant() switch
        {
            "CSV" => new CsvEmpresaParser(),
            // "JSON" => new JsonEmpresaParser(),
            // "API" => new RestApiEmpresaParser(),
            _ => null
        };
    }

    private EmpresaImportResult CriarResultadoErro(string mensagem, string origem)
    {
        return new EmpresaImportResult
        {
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Status = "Failed",
            Message = mensagem,
            ImportSource = origem,
            TotalRecords = 0,
            Inserted = 0,
            Skipped = 0,
            Errors = new List<EmpresaImportError>
            {
                new()
                {
                    RecordId = "GLOBAL",
                    ErrorType = "ConfigurationError",
                    Message = mensagem,
                    Stage = "Initialization"
                }
            }
        };
    }
}
