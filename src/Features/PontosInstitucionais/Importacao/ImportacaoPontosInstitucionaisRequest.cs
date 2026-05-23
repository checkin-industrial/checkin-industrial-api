using Microsoft.AspNetCore.Http;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais.Importacao;

public class ImportacaoPontosInstitucionaisRequest
{
    public IFormFile File { get; set; } = null!;
}
