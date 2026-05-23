using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais.Importacao;

public static class ImportPontosInstitucionais
{
    private const long MaxFileSize = 20 * 1024 * 1024; // 20 MB

    public static RouteHandlerBuilder MapImportPontosInstitucionais(this RouteGroupBuilder group)
    {
        return group.MapPost("/pontos-institucionais", Handle)
            .WithName(nameof(ImportPontosInstitucionais))
            .DisableAntiforgery();
    }

    private static async Task<IResult> Handle(
        IFormFile file,
        AppDbContext context,
        ILogger<PontoInstitucionalImportResult> logger,
        CancellationToken cancellationToken)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (file is null || file.Length == 0)
        {
            return TypedResults.BadRequest(new { erro = "Nenhum arquivo foi fornecido." });
        }

        if (file.Length > MaxFileSize)
        {
            return TypedResults.BadRequest(new { erro = "Arquivo muito grande. Tamanho maximo: 20 MB." });
        }

        var extensao = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extensao != ".csv")
        {
            return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
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
                return TypedResults.Ok(resultado);
            }

            csv.ReadHeader();
            var lineNumber = 1;

            while (await csv.ReadAsync())
            {
                lineNumber++;
                resultado.TotalRecords++;

                var validationErrors = new List<PontoInstitucionalImportError>();

                var idValue = PontoInstitucionalCsvFormatter.GetField(csv, "Id");
                var nome = PontoInstitucionalCsvFormatter.GetField(csv, "Nome");
                var tipoText = PontoInstitucionalCsvFormatter.GetField(csv, "Tipo");
                var descricao = PontoInstitucionalCsvFormatter.GetField(csv, "Descricao");
                var endereco = PontoInstitucionalCsvFormatter.GetField(csv, "Endereco");
                var latitudeText = PontoInstitucionalCsvFormatter.GetField(csv, "Latitude");
                var longitudeText = PontoInstitucionalCsvFormatter.GetField(csv, "Longitude");

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

                var tipo = PontoInstitucionalCsvFormatter.ParseTipo(tipoText);
                if (!tipo.HasValue)
                {
                    validationErrors.Add(new PontoInstitucionalImportError { LineNumber = lineNumber, FieldName = "Tipo", Message = "Tipo invalido." });
                }

                var latitude = PontoInstitucionalCsvFormatter.ParseNullableDecimal(latitudeText);
                if (!latitude.HasValue || latitude < -90m || latitude > 90m)
                {
                    validationErrors.Add(new PontoInstitucionalImportError { LineNumber = lineNumber, FieldName = "Latitude", Message = "Latitude deve estar entre -90 e 90." });
                }

                var longitude = PontoInstitucionalCsvFormatter.ParseNullableDecimal(longitudeText);
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
                    pontoExistente = await context.PontosInstitucionais.FirstOrDefaultAsync(p => p.Id == id.Value, cancellationToken);
                }
                else
                {
                    pontoExistente = await context.PontosInstitucionais.FirstOrDefaultAsync(
                        p => p.Nome == nome && p.Endereco == endereco && p.Tipo == tipo!.Value,
                        cancellationToken);
                }

                if (pontoExistente is null)
                {
                    pontoExistente = new PontoInstitucional { Id = id ?? Guid.NewGuid() };
                    context.PontosInstitucionais.Add(pontoExistente);
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
                pontoExistente.AtividadesDisponiveis = PontoInstitucionalCsvFormatter.NormalizeNullable(PontoInstitucionalCsvFormatter.GetField(csv, "Atividades_Disponiveis"));
                pontoExistente.EquipeGestao = PontoInstitucionalCsvFormatter.NormalizeNullable(PontoInstitucionalCsvFormatter.GetField(csv, "Equipe_Gestao"));
                pontoExistente.ContatoNome = PontoInstitucionalCsvFormatter.NormalizeNullable(PontoInstitucionalCsvFormatter.GetField(csv, "Contato_Nome"));
                pontoExistente.ContatoTelefone = PontoInstitucionalCsvFormatter.NormalizeNullable(PontoInstitucionalCsvFormatter.GetField(csv, "Contato_Telefone"));
                pontoExistente.ContatoEmail = PontoInstitucionalCsvFormatter.NormalizeNullable(PontoInstitucionalCsvFormatter.GetField(csv, "Contato_Email"));
                pontoExistente.ResponsavelFotoUrl = PontoInstitucionalCsvFormatter.NormalizeNullable(PontoInstitucionalCsvFormatter.GetField(csv, "Responsavel_Foto_Url"));
                pontoExistente.LogoUrl = PontoInstitucionalCsvFormatter.NormalizeNullable(PontoInstitucionalCsvFormatter.GetField(csv, "Logo_Url"));
                pontoExistente.CardFotoUrl = PontoInstitucionalCsvFormatter.NormalizeNullable(PontoInstitucionalCsvFormatter.GetField(csv, "Card_Foto_Url"));
                pontoExistente.CorMarcador = PontoInstitucionalCsvFormatter.NormalizeNullable(PontoInstitucionalCsvFormatter.GetField(csv, "Cor_Marcador"));
                pontoExistente.IconeMarcador = PontoInstitucionalCsvFormatter.NormalizeNullable(PontoInstitucionalCsvFormatter.GetField(csv, "Icone_Marcador"));
                pontoExistente.OrdemExibicao = PontoInstitucionalCsvFormatter.ParseNullableInt(PontoInstitucionalCsvFormatter.GetField(csv, "Ordem_Exibicao"));
                pontoExistente.Ativo = PontoInstitucionalCsvFormatter.ParseNullableBool(PontoInstitucionalCsvFormatter.GetField(csv, "Ativo"));
            }

            await context.SaveChangesAsync(cancellationToken);

            resultado.Status = resultado.Errors.Count == 0 ? "Completed" : "CompletedWithErrors";
            return TypedResults.Ok(resultado);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao importar pontos institucionais");
            return Results.Problem(
                detail: "Erro ao processar importacao de pontos institucionais.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
