using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AppTurismoIndustrial.Api.Shared.Swagger;

// Adiciona o X-Api-Key como requirement so nos endpoints que tem .RequireAuthorization().
// Sem isso, ou todos os endpoints aparecem com cadeado (impreciso, polui a UI), ou
// nenhum aparece (cadeado global ausente). Filtro por endpoint mantem a UI honesta.
public sealed class ApiKeySecurityOperationFilter : IOperationFilter
{
    private static readonly OpenApiSecuritySchemeReference SchemeRef = new(Auth.ApiKeyAuthenticationOptions.Scheme);

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuth = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<AuthorizeAttribute>()
            .Any();

        if (!hasAuth)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [SchemeRef] = new List<string>(),
        });
    }
}
