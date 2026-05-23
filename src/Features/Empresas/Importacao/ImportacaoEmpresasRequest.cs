using Microsoft.AspNetCore.Http;

namespace AppTurismoIndustrial.Api.Features.Empresas.Importacao;

public class ImportacaoEmpresasRequest
{
    public IFormFile File { get; set; } = null!;
}