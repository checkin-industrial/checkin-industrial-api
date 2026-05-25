

using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public class EmpresaMapService : IEmpresaMapService
{
    private readonly AppDbContext _context;
    private readonly int _maxMapRecords;
    private const double DefaultMapCenterLatitude = -22.60d;
    private const double DefaultMapCenterLongitude = -48.80d;

    public EmpresaMapService(AppDbContext context, IOptions<LimitsOptions>? limits = null)
    {
        _context = context;
        _maxMapRecords = (limits?.Value ?? new LimitsOptions()).MaxMapRecords;
    }

    public async Task<IReadOnlyCollection<EmpresaMapDTO>> ListarParaMapaAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                e."Id",
                e."NomeFantasia",
                e."Latitude"::double precision AS latitude,
                e."Longitude"::double precision AS longitude,
                e."Setor",
                e."CnaePrincipal",
                e."DescricaoCnae",
                e."Endereco",
                e."Telefone",
                e."Cep",
                e."Municipio",
                e."MatrizOuFilial",
                e."NumeroFuncionarios"
            FROM empresas e
            WHERE e."Latitude" <> 0
              AND e."Longitude" <> 0
            ORDER BY ST_SetSRID(ST_MakePoint(e."Longitude"::double precision, e."Latitude"::double precision), 4326)
                     <-> ST_SetSRID(ST_MakePoint(@center_lon::double precision, @center_lat::double precision), 4326)
            LIMIT @limit::integer;
            """;

        await using var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var centerLonParam = command.CreateParameter();
        centerLonParam.ParameterName = "@center_lon";
        centerLonParam.Value = DefaultMapCenterLongitude;
        command.Parameters.Add(centerLonParam);

        var centerLatParam = command.CreateParameter();
        centerLatParam.ParameterName = "@center_lat";
        centerLatParam.Value = DefaultMapCenterLatitude;
        command.Parameters.Add(centerLatParam);

        var limitParam = command.CreateParameter();
        limitParam.ParameterName = "@limit";
        limitParam.Value = _maxMapRecords;
        command.Parameters.Add(limitParam);

        var resultados = new List<EmpresaMapDTO>(_maxMapRecords);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var setorEnum = (SetorEmpresa)reader.GetInt32(4);

            resultados.Add(new EmpresaMapDTO
            {
                Id = reader.GetGuid(0),
                NomeFantasia = reader.GetString(1),
                Latitude = reader.GetDouble(2),
                Longitude = reader.GetDouble(3),
                Setor = setorEnum.ToString().ToLowerInvariant(),
                Cnae = reader.GetString(5),
                DescricaoCnae = reader.GetString(6),
                Endereco = reader.GetString(7),
                Telefone = GetNullableString(reader, 8),
                Cep = GetNullableString(reader, 9),
                Municipio = reader.GetString(10),
                MatrizOuFilial = ((MatrizOuFilialEmpresa)reader.GetInt32(11)).ToString(),
                NumeroFuncionarios = GetNullableInt32(reader, 12)
            });
        }

        return resultados;
    }

    private static string GetNullableString(DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static int GetNullableInt32(DbDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
    }
}