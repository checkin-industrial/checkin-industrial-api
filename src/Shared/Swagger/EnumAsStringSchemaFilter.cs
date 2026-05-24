using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AppTurismoIndustrial.Api.Shared.Swagger;

/// <summary>
/// Mostra enums como strings no schema do OpenAPI (em vez do int default do
/// Swashbuckle). Espelha o comportamento de runtime configurado em
/// Program.cs com <c>JsonStringEnumConverter(JsonNamingPolicy.CamelCase)</c>.
///
/// Sem este filter, o Swagger continuaria documentando enums como inteiros,
/// gerando confusao entre OpenAPI vs payload real.
/// </summary>
public sealed class EnumAsStringSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum || schema is not OpenApiSchema concrete)
        {
            return;
        }

        var names = Enum.GetNames(context.Type)
            .Select(n => JsonNamingPolicy.CamelCase.ConvertName(n))
            .Select(n => (JsonNode)JsonValue.Create(n)!)
            .ToList();

        concrete.Type = JsonSchemaType.String;
        concrete.Format = null;
        concrete.Enum = names;
    }
}
