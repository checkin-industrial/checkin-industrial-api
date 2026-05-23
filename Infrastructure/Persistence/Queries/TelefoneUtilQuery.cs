using AppTurismoIndustrial.Api.Application.DTOs;
using AppTurismoIndustrial.Api.Application.Queries;
using AppTurismoIndustrial.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Infrastructure.Persistence.Queries;

public class TelefoneUtilQuery : ITelefoneUtilQuery
{
    private readonly AppDbContext _context;

    public TelefoneUtilQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<DTOTelefoneUtil>> ConsultarAsync(
        DTOTelefoneUtilFiltroParams filtros,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TelefonesUteis
            .AsNoTracking();

        if (filtros.Ativo.HasValue)
        {
            query = query.Where(t => (t.Ativo ?? true) == filtros.Ativo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Categoria))
        {
            var categoria = ParseCategoria(filtros.Categoria);
            if (!categoria.HasValue)
            {
                return [];
            }

            query = query.Where(t => t.Categoria == categoria.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Termo))
        {
            var termo = filtros.Termo.Trim().ToLowerInvariant();
            query = query.Where(t =>
                t.Nome.ToLower().Contains(termo)
                || t.Telefone.ToLower().Contains(termo));
        }

        var registros = await query
            .OrderBy(t => t.Categoria)
            .ThenBy(t => t.OrdemExibicao ?? 0)
            .ThenBy(t => t.Nome)
            .Select(t => new DTOTelefoneUtil
            {
                Id = t.Id,
                Nome = t.Nome,
                Categoria = t.Categoria.ToString().ToLowerInvariant(),
                Telefone = t.Telefone,
                OrdemExibicao = t.OrdemExibicao ?? 0,
                Ativo = t.Ativo ?? true,
            })
            .ToListAsync(cancellationToken);

        return registros;
    }

    private static CategoriaTelefoneUtil? ParseCategoria(string categoria)
    {
        return categoria.Trim().ToLowerInvariant() switch
        {
            "emergenciaservicospublicos" => CategoriaTelefoneUtil.EmergenciaServicosPublicos,
            "emergencia_servicos_publicos" => CategoriaTelefoneUtil.EmergenciaServicosPublicos,
            "transportecultura" => CategoriaTelefoneUtil.TransporteCultura,
            "transporte_cultura" => CategoriaTelefoneUtil.TransporteCultura,
            "hoteispousadas" => CategoriaTelefoneUtil.HoteisPousadas,
            "hoteis_pousadas" => CategoriaTelefoneUtil.HoteisPousadas,
            _ => null,
        };
    }
}
