using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class UploadImagemPontoInstitucional
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".svg"
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/svg+xml"
    };

    private const long MaxImageBytes = 5 * 1024 * 1024;

    public static RouteGroupBuilder MapUploadImagemPontoInstitucional(this RouteGroupBuilder group)
    {
        group.MapPost("/upload-imagem", Handle)
            .WithName(nameof(UploadImagemPontoInstitucional))
            .DisableAntiforgery();
        return group;
    }

    private static async Task<Results<Ok<DTOUploadArquivoResponse>, BadRequest<object>>> Handle(
        IFormFile file,
        IFormCollection form,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return TypedResults.BadRequest<object>(new { message = "Arquivo nao informado." });
        }

        if (file.Length > MaxImageBytes)
        {
            return TypedResults.BadRequest<object>(new { message = "Arquivo excede o limite de 5 MB." });
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return TypedResults.BadRequest<object>(new { message = "Extensao de arquivo nao permitida." });
        }

        if (!AllowedMimeTypes.Contains(file.ContentType))
        {
            return TypedResults.BadRequest<object>(new { message = "Tipo de arquivo nao permitido." });
        }

        var categoria = form["categoria"].ToString();
        var categoriaNormalizada = categoria.Trim().ToLowerInvariant() switch
        {
            "logo" => "logo",
            "card" => "card",
            _ => "foto",
        };

        // Se UPLOADS_ROOT estiver definido, os arquivos vao direto para o volume (ex.: /uploads no Railway).
        var uploadsRoot = configuration["UPLOADS_ROOT"];
        string absoluteDirectory;

        if (!string.IsNullOrWhiteSpace(uploadsRoot))
        {
            absoluteDirectory = Path.Combine(uploadsRoot, "pontos-institucionais", categoriaNormalizada);
        }
        else
        {
            var rootPath = string.IsNullOrWhiteSpace(environment.WebRootPath)
                ? Path.Combine(environment.ContentRootPath, "wwwroot")
                : environment.WebRootPath;
            absoluteDirectory = Path.Combine(rootPath, "uploads", "pontos-institucionais", categoriaNormalizada);
        }

        Directory.CreateDirectory(absoluteDirectory);

        var uniqueFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var absoluteFilePath = Path.Combine(absoluteDirectory, uniqueFileName);

        await using (var stream = File.Create(absoluteFilePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativeUrl = $"/uploads/pontos-institucionais/{categoriaNormalizada}/{uniqueFileName}";
        return TypedResults.Ok(new DTOUploadArquivoResponse { Url = relativeUrl });
    }
}
