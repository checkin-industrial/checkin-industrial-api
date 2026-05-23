using Microsoft.AspNetCore.Http;

namespace AppTurismoIndustrial.Api.Application.DTOs.Import;

public class ImportacaoPontosInstitucionaisRequest
{
    public IFormFile File { get; set; } = null!;
}
