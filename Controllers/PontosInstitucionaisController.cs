using AppTurismoIndustrial.Api.Application.DTOs;
using AppTurismoIndustrial.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AppTurismoIndustrial.Api.Controllers;

[ApiController]
[Route("api/pontos-institucionais")]
public class PontosInstitucionaisController : ControllerBase
{
    private readonly IPontoInstitucionalService _pontoInstitucionalService;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".svg"
    };
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/svg+xml"
    };
    private const long MaxImageBytes = 5 * 1024 * 1024;

    public PontosInstitucionaisController(IPontoInstitucionalService pontoInstitucionalService, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _pontoInstitucionalService = pontoInstitucionalService;
        _environment = environment;
        _configuration = configuration;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DTOPontoInstitucional>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DTOPontoInstitucional>>> Listar(
        [FromQuery] DTOPontoInstitucionalFiltroParams filtros,
        CancellationToken cancellationToken)
    {
        var pontos = await _pontoInstitucionalService.ListarAsync(filtros, cancellationToken);
        return Ok(pontos.ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DTOPontoInstitucional), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DTOPontoInstitucional>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var ponto = await _pontoInstitucionalService.ObterPorIdAsync(id, cancellationToken);

        if (ponto is null)
        {
            return NotFound();
        }

        return Ok(ponto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DTOPontoInstitucional), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DTOPontoInstitucional>> Criar(
        [FromBody] DTOPontoInstitucionalCriar dto,
        CancellationToken cancellationToken)
    {
        var pontoCriado = await _pontoInstitucionalService.CriarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = pontoCriado.Id }, pontoCriado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] DTOPontoInstitucionalAtualizar dto,
        CancellationToken cancellationToken)
    {
        var atualizado = await _pontoInstitucionalService.AtualizarAsync(id, dto, cancellationToken);

        if (!atualizado)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        var removido = await _pontoInstitucionalService.RemoverAsync(id, cancellationToken);

        if (!removido)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("upload-imagem")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DTOUploadArquivoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DTOUploadArquivoResponse>> UploadImagem(
        IFormFile file,
        [FromForm] string? categoria,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Arquivo nao informado." });
        }

        if (file.Length > MaxImageBytes)
        {
            return BadRequest(new { message = "Arquivo excede o limite de 5 MB." });
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Extensao de arquivo nao permitida." });
        }

        if (!AllowedMimeTypes.Contains(file.ContentType))
        {
            return BadRequest(new { message = "Tipo de arquivo nao permitido." });
        }

        var categoriaNormalizada = categoria?.Trim().ToLowerInvariant() switch
        {
            "logo" => "logo",
            "card" => "card",
            _ => "foto",
        };

        // Se UPLOADS_ROOT estiver definido, os arquivos vão direto para o volume (ex.: /uploads no Railway).
        var uploadsRoot = _configuration["UPLOADS_ROOT"];
        string absoluteDirectory;
        string relativeUrl;

        if (!string.IsNullOrWhiteSpace(uploadsRoot))
        {
            // Volume externo: /uploads/pontos-institucionais/<categoria>/
            absoluteDirectory = Path.Combine(uploadsRoot, "pontos-institucionais", categoriaNormalizada);
        }
        else
        {
            // Padrão local: wwwroot/uploads/pontos-institucionais/<categoria>/
            var rootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;
            absoluteDirectory = Path.Combine(rootPath, "uploads", "pontos-institucionais", categoriaNormalizada);
        }
        Directory.CreateDirectory(absoluteDirectory);

        var uniqueFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var absoluteFilePath = Path.Combine(absoluteDirectory, uniqueFileName);

        await using (var stream = System.IO.File.Create(absoluteFilePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        relativeUrl = $"/uploads/pontos-institucionais/{categoriaNormalizada}/{uniqueFileName}";

        return Ok(new DTOUploadArquivoResponse
        {
            Url = relativeUrl,
        });
    }
}
