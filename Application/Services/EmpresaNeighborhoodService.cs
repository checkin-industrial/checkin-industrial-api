using AppTurismoIndustrial.Api.Application.DTOs;
using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Application.Services;

public class EmpresaNeighborhoodService : IEmpresaNeighborhoodService
{
    private const int MaxRadiusMeters = 50000;
    private const int MaxLimit = 20;
    private const double EarthRadiusMeters = 6371000d;
    private readonly AppDbContext _context;

    public EmpresaNeighborhoodService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DTOEmpresaVizinhancaResponse?> ObterVizinhancaAsync(
        Guid empresaId,
        int radiusMeters,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var safeRadius = Math.Clamp(radiusMeters, 1, MaxRadiusMeters);
        var safeLimit = Math.Clamp(limit, 1, MaxLimit);

        var empresaBase = await _context.Empresas
            .AsNoTracking()
            .Where(e => e.Id == empresaId)
            .Select(e => new
            {
                e.Id,
                e.NomeFantasia,
                e.CnaePrincipal,
                e.Setor,
                NumeroFuncionarios = e.NumeroFuncionarios ?? 0,
                e.Municipio,
                e.Latitude,
                e.Longitude
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (empresaBase is null)
        {
            return null;
        }

        var baseLatitude = (double)empresaBase.Latitude;
        var baseLongitude = (double)empresaBase.Longitude;

        if (!HasValidCoordinate(baseLatitude, baseLongitude))
        {
            return new DTOEmpresaVizinhancaResponse
            {
                EmpresaBase = MapBaseCompany(empresaBase),
                EmpresasProximas = []
            };
        }

        var latitudeDelta = safeRadius / 111320d;
        var longitudeDelta = safeRadius / (111320d * Math.Max(Math.Cos(ToRadians(baseLatitude)), 0.01d));
        var minLatitude = (decimal)Math.Max(-90d, baseLatitude - latitudeDelta);
        var maxLatitude = (decimal)Math.Min(90d, baseLatitude + latitudeDelta);
        var minLongitude = (decimal)Math.Max(-180d, baseLongitude - longitudeDelta);
        var maxLongitude = (decimal)Math.Min(180d, baseLongitude + longitudeDelta);

        var candidates = await _context.Empresas
            .AsNoTracking()
            .Where(e => e.Id != empresaId)
            .Where(e => e.Latitude >= minLatitude && e.Latitude <= maxLatitude)
            .Where(e => e.Longitude >= minLongitude && e.Longitude <= maxLongitude)
            .Select(e => new
            {
                e.Id,
                e.NomeFantasia,
                e.CnaePrincipal,
                e.Setor,
                NumeroFuncionarios = e.NumeroFuncionarios ?? 0,
                e.Municipio,
                e.Latitude,
                e.Longitude
            })
            .ToListAsync(cancellationToken);

        var empresasProximas = candidates
            .Select(candidate =>
            {
                var distanciaMetros = DistanceInMeters(
                    baseLatitude,
                    baseLongitude,
                    (double)candidate.Latitude,
                    (double)candidate.Longitude);

                return new DTOEmpresaVizinha
                {
                    Id = candidate.Id,
                    NomeFantasia = candidate.NomeFantasia,
                    CnaePrincipal = candidate.CnaePrincipal,
                    Setor = candidate.Setor.ToString().ToLowerInvariant(),
                    NumeroFuncionarios = candidate.NumeroFuncionarios,
                    Municipio = candidate.Municipio,
                    DistanciaMetros = Math.Round(distanciaMetros, 1),
                    MesmoCnae = string.Equals(candidate.CnaePrincipal, empresaBase.CnaePrincipal, StringComparison.Ordinal),
                    MesmoSetor = candidate.Setor == empresaBase.Setor
                };
            })
            .Where(item => item.DistanciaMetros <= safeRadius)
            .OrderBy(item => item.DistanciaMetros)
            .Take(safeLimit)
            .ToList();

        return new DTOEmpresaVizinhancaResponse
        {
            EmpresaBase = MapBaseCompany(empresaBase),
            EmpresasProximas = empresasProximas
        };
    }

    private static bool HasValidCoordinate(double latitude, double longitude)
    {
        return NumberIsFinite(latitude)
            && NumberIsFinite(longitude)
            && latitude is >= -90d and <= 90d
            && longitude is >= -180d and <= 180d
            && (latitude != 0d || longitude != 0d);
    }

    private static bool NumberIsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private static DTOEmpresaVizinhancaBase MapBaseCompany(dynamic empresaBase)
    {
        return new DTOEmpresaVizinhancaBase
        {
            Id = empresaBase.Id,
            NomeFantasia = empresaBase.NomeFantasia,
            CnaePrincipal = empresaBase.CnaePrincipal,
            Setor = empresaBase.Setor.ToString().ToLowerInvariant(),
            NumeroFuncionarios = empresaBase.NumeroFuncionarios,
            Municipio = empresaBase.Municipio,
            Latitude = (double)empresaBase.Latitude,
            Longitude = (double)empresaBase.Longitude
        };
    }

    private static double ToRadians(double value)
    {
        return value * Math.PI / 180d;
    }

    private static double DistanceInMeters(double fromLat, double fromLng, double toLat, double toLng)
    {
        var deltaLat = ToRadians(toLat - fromLat);
        var deltaLng = ToRadians(toLng - fromLng);
        var fromLatRad = ToRadians(fromLat);
        var toLatRad = ToRadians(toLat);

        var haversine = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
            + Math.Cos(fromLatRad) * Math.Cos(toLatRad) * Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);

        return 2d * EarthRadiusMeters * Math.Asin(Math.Sqrt(haversine));
    }
}