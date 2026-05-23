using AppTurismoIndustrial.Api.Application.DTOs;
using AppTurismoIndustrial.Api.Application.Queries;
using AppTurismoIndustrial.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Infrastructure.Persistence.Queries;

public class PontoInstitucionalQuery : IPontoInstitucionalQuery
{
    private readonly AppDbContext _context;

    public PontoInstitucionalQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<DTOPontoInstitucional>> ConsultarAsync(
        DTOPontoInstitucionalFiltroParams filtros,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PontosInstitucionais
            .AsNoTracking();

        if (filtros.Ativo.HasValue)
        {
            query = query.Where(p => (p.Ativo ?? true) == filtros.Ativo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Tipo))
        {
            var tipo = ParseTipo(filtros.Tipo);
            if (!tipo.HasValue)
            {
                return [];
            }

            query = query.Where(p => p.Tipo == tipo.Value);
        }

        var registros = await query
            .OrderBy(p => p.OrdemExibicao ?? 0)
            .ThenBy(p => p.Nome)
            .Select(p => new DTOPontoInstitucional
            {
                Id = p.Id,
                Nome = p.Nome,
                Tipo = p.Tipo.ToString().ToLowerInvariant(),
                Descricao = p.Descricao,
                Endereco = p.Endereco,
                Latitude = (double)p.Latitude,
                Longitude = (double)p.Longitude,
                AtividadesDisponiveis = p.AtividadesDisponiveis ?? string.Empty,
                EquipeGestao = p.EquipeGestao ?? string.Empty,
                ContatoNome = p.ContatoNome ?? string.Empty,
                ContatoTelefone = p.ContatoTelefone ?? string.Empty,
                ContatoEmail = p.ContatoEmail ?? string.Empty,
                ResponsavelFotoUrl = p.ResponsavelFotoUrl,
                LogoUrl = p.LogoUrl,
                CardFotoUrl = p.CardFotoUrl,
                CorMarcador = p.CorMarcador ?? "#0d9488",
                IconeMarcador = p.IconeMarcador ?? "institucional",
                OrdemExibicao = p.OrdemExibicao ?? 0,
                Ativo = p.Ativo ?? true
            })
            .ToListAsync(cancellationToken);

        return registros;
    }

    private static TipoPontoInstitucional? ParseTipo(string tipo)
    {
        return tipo.Trim().ToLowerInvariant() switch
        {
            "educacao" => TipoPontoInstitucional.Educacao,
            "comercio" => TipoPontoInstitucional.Comercio,
            "financeiro" => TipoPontoInstitucional.Financeiro,
            "servico" => TipoPontoInstitucional.Servico,
            "servicos" => TipoPontoInstitucional.Servico,
            "setor_prefeitura" => TipoPontoInstitucional.SetorPrefeitura,
            "setorprefeitura" => TipoPontoInstitucional.SetorPrefeitura,
            "sedecom" => TipoPontoInstitucional.SetorPrefeitura,
            "senai" => TipoPontoInstitucional.Educacao,
            "pontoturistico" => TipoPontoInstitucional.PontoTuristico,
            "hotel" => TipoPontoInstitucional.Hotel,
            "ecoturismo" => TipoPontoInstitucional.Ecoturismo,
            _ => null,
        };
    }
}
