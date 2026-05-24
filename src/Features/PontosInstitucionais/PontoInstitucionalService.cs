

using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using AppTurismoIndustrial.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public class PontoInstitucionalService : IPontoInstitucionalService
{
    private readonly IPontoInstitucionalQuery _pontoInstitucionalQuery;
    private readonly AppDbContext _context;

    public PontoInstitucionalService(
        IPontoInstitucionalQuery pontoInstitucionalQuery,
        AppDbContext context)
    {
        _pontoInstitucionalQuery = pontoInstitucionalQuery;
        _context = context;
    }

    public async Task<IReadOnlyCollection<DTOPontoInstitucional>> ListarAsync(
        DTOPontoInstitucionalFiltroParams filtros,
        CancellationToken cancellationToken = default)
    {
        return await _pontoInstitucionalQuery.ConsultarAsync(filtros, cancellationToken);
    }

    public async Task<DTOPontoInstitucional?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ponto = await _context.PontosInstitucionais
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return ponto is null ? null : MapToDto(ponto);
    }

    public async Task<DTOPontoInstitucional> CriarAsync(DTOPontoInstitucionalCriar dto, CancellationToken cancellationToken = default)
    {
        var ponto = new PontoInstitucional
        {
            Nome = dto.Nome,
            Tipo = dto.Tipo,
            Descricao = dto.Descricao,
            Endereco = dto.Endereco,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            AtividadesDisponiveis = dto.AtividadesDisponiveis,
            EquipeGestao = dto.EquipeGestao,
            ContatoNome = dto.ContatoNome,
            ContatoTelefone = dto.ContatoTelefone,
            ContatoEmail = dto.ContatoEmail,
            ResponsavelFotoUrl = dto.ResponsavelFotoUrl,
            LogoUrl = dto.LogoUrl,
            CardFotoUrl = dto.CardFotoUrl,
            CorMarcador = dto.CorMarcador,
            IconeMarcador = dto.IconeMarcador,
            OrdemExibicao = dto.OrdemExibicao,
            Ativo = dto.Ativo,
        };

        _context.PontosInstitucionais.Add(ponto);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(ponto);
    }

    public async Task AtualizarAsync(Guid id, DTOPontoInstitucionalAtualizar dto, CancellationToken cancellationToken = default)
    {
        var ponto = await _context.PontosInstitucionais
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (ponto is null)
        {
            throw new NotFoundException($"Ponto institucional {id} nao encontrado.");
        }

        ponto.Nome = dto.Nome;
        ponto.Tipo = dto.Tipo;
        ponto.Descricao = dto.Descricao;
        ponto.Endereco = dto.Endereco;
        ponto.Latitude = dto.Latitude;
        ponto.Longitude = dto.Longitude;
        ponto.AtividadesDisponiveis = dto.AtividadesDisponiveis;
        ponto.EquipeGestao = dto.EquipeGestao;
        ponto.ContatoNome = dto.ContatoNome;
        ponto.ContatoTelefone = dto.ContatoTelefone;
        ponto.ContatoEmail = dto.ContatoEmail;
        ponto.ResponsavelFotoUrl = dto.ResponsavelFotoUrl;
        ponto.LogoUrl = dto.LogoUrl;
        ponto.CardFotoUrl = dto.CardFotoUrl;
        ponto.CorMarcador = dto.CorMarcador;
        ponto.IconeMarcador = dto.IconeMarcador;
        ponto.OrdemExibicao = dto.OrdemExibicao;
        ponto.Ativo = dto.Ativo;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ponto = await _context.PontosInstitucionais
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (ponto is null)
        {
            throw new NotFoundException($"Ponto institucional {id} nao encontrado.");
        }

        if (ponto.Ativo == false)
        {
            return;
        }

        ponto.Ativo = false;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static DTOPontoInstitucional MapToDto(PontoInstitucional ponto)
    {
        return new DTOPontoInstitucional
        {
            Id = ponto.Id,
            Nome = ponto.Nome,
            Tipo = ponto.Tipo.ToString().ToLowerInvariant(),
            Descricao = ponto.Descricao,
            Endereco = ponto.Endereco,
            Latitude = (double)ponto.Latitude,
            Longitude = (double)ponto.Longitude,
            AtividadesDisponiveis = ponto.AtividadesDisponiveis ?? string.Empty,
            EquipeGestao = ponto.EquipeGestao ?? string.Empty,
            ContatoNome = ponto.ContatoNome ?? string.Empty,
            ContatoTelefone = ponto.ContatoTelefone ?? string.Empty,
            ContatoEmail = ponto.ContatoEmail ?? string.Empty,
            ResponsavelFotoUrl = ponto.ResponsavelFotoUrl,
            LogoUrl = ponto.LogoUrl,
            CardFotoUrl = ponto.CardFotoUrl,
            CorMarcador = ponto.CorMarcador ?? "#0d9488",
            IconeMarcador = ponto.IconeMarcador ?? "institucional",
            OrdemExibicao = ponto.OrdemExibicao ?? 0,
            Ativo = ponto.Ativo ?? true,
        };
    }
}
