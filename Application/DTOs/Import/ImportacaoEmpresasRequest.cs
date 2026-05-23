using Microsoft.AspNetCore.Http;

namespace AppTurismoIndustrial.Api.Application.DTOs.Import;

public class ImportacaoEmpresasRequest
{
    public IFormFile File { get; set; } = null!;
}