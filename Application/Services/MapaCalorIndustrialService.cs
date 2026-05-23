using AppTurismoIndustrial.Api.Application.DTOs.Analytics;
using AppTurismoIndustrial.Api.Application.Queries;
using AppTurismoIndustrial.Api.Domain.Entities;

namespace AppTurismoIndustrial.Api.Application.Services;

public class MapaCalorIndustrialService : IMapaCalorIndustrialService
{
    private readonly IMapaCalorIndustrialQuery _mapaCalorIndustrialQuery;

    public MapaCalorIndustrialService(IMapaCalorIndustrialQuery mapaCalorIndustrialQuery)
    {
        _mapaCalorIndustrialQuery = mapaCalorIndustrialQuery;
    }

    public async Task<IReadOnlyCollection<HeatmapPointDTO>> ObterMapaCalorAsync(
        DTOConsultaMapaCalorIndustrial consulta,
        CancellationToken cancellationToken = default)
    {
        var cnae = string.IsNullOrWhiteSpace(consulta.Cnae)
            ? null
            : consulta.Cnae.Trim();

        var setor = ParseSetor(consulta.Setor);

        var pontos = await _mapaCalorIndustrialQuery.ObterPontosAsync(cnae, setor, cancellationToken);

        return pontos
            .Select(p => new HeatmapPointDTO
            {
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Peso = p.Density
            })
            .ToList();
    }

    private static SetorEmpresa? ParseSetor(string? setor)
    {
        if (string.IsNullOrWhiteSpace(setor))
        {
            return null;
        }

        return setor.Trim().ToLowerInvariant() switch
        {
            "industria" => SetorEmpresa.Industria,
            "comercio" => SetorEmpresa.Comercio,
            "servicos" => SetorEmpresa.Servicos,
            _ => null
        };
    }
}