using AppTurismoIndustrial.Api.Application.DTOs.Analytics;
using AppTurismoIndustrial.Api.Application.Queries;
using AppTurismoIndustrial.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Infrastructure.Persistence.Queries;

public class MapaCalorIndustrialQuery : IMapaCalorIndustrialQuery
{
    private readonly AppDbContext _context;
    private const double GridSize = 0.005d;

    public MapaCalorIndustrialQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<HeatmapQueryPoint>> ObterPontosAsync(
        string? cnae,
        SetorEmpresa? setor,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                SELECT
                    FLOOR(e."Latitude"::double precision / @grid_size::double precision) * @grid_size::double precision
                        + @grid_size::double precision / 2.0 AS latitude,
                    FLOOR(e."Longitude"::double precision / @grid_size::double precision) * @grid_size::double precision
                        + @grid_size::double precision / 2.0 AS longitude,
                    COUNT(*)::int AS peso
                FROM empresas e
                WHERE e."Latitude" <> 0
                    AND e."Longitude" <> 0
                    AND (@cnae::text IS NULL OR e."CnaePrincipal" = @cnae::text)
                    AND (@setor::integer IS NULL OR e."Setor" = @setor::integer)
                GROUP BY
                    FLOOR(e."Latitude"::double precision / @grid_size::double precision),
                    FLOOR(e."Longitude"::double precision / @grid_size::double precision)
                ORDER BY peso DESC;
            """;

        await using var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var cnaeParam = command.CreateParameter();
        cnaeParam.ParameterName = "@cnae";
        cnaeParam.Value = string.IsNullOrWhiteSpace(cnae) ? DBNull.Value : cnae.Trim();
        command.Parameters.Add(cnaeParam);

        var setorParam = command.CreateParameter();
        setorParam.ParameterName = "@setor";
        setorParam.Value = setor.HasValue ? (int)setor.Value : DBNull.Value;
        command.Parameters.Add(setorParam);

        var gridParam = command.CreateParameter();
        gridParam.ParameterName = "@grid_size";
        gridParam.Value = GridSize;
        command.Parameters.Add(gridParam);

        var resultados = new List<HeatmapQueryPoint>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            resultados.Add(new HeatmapQueryPoint
            {
                Latitude = reader.GetDouble(0),
                Longitude = reader.GetDouble(1),
                Density = reader.GetInt32(2)
            });
        }

        return resultados;
    }
}