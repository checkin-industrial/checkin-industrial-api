

using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public class TelefoneUtilService : ITelefoneUtilService
{
    private readonly ITelefoneUtilQuery _telefoneUtilQuery;
    private readonly AppDbContext _context;

    public TelefoneUtilService(
        ITelefoneUtilQuery telefoneUtilQuery,
        AppDbContext context)
    {
        _telefoneUtilQuery = telefoneUtilQuery;
        _context = context;
    }

    public async Task<IReadOnlyCollection<DTOTelefoneUtil>> ListarAsync(
        DTOTelefoneUtilFiltroParams filtros,
        CancellationToken cancellationToken = default)
    {
        return await _telefoneUtilQuery.ConsultarAsync(filtros, cancellationToken);
    }

    public async Task<DTOTelefoneUtil?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var telefoneUtil = await _context.TelefonesUteis
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return telefoneUtil is null ? null : MapToDto(telefoneUtil);
    }

    public async Task<DTOTelefoneUtil> CriarAsync(DTOTelefoneUtilCriar dto, CancellationToken cancellationToken = default)
    {
        var telefoneUtil = new TelefoneUtil
        {
            Nome = dto.Nome,
            Categoria = dto.Categoria,
            Telefone = dto.Telefone,
            OrdemExibicao = dto.OrdemExibicao,
            Ativo = dto.Ativo,
        };

        _context.TelefonesUteis.Add(telefoneUtil);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(telefoneUtil);
    }

    public async Task<bool> AtualizarAsync(Guid id, DTOTelefoneUtilAtualizar dto, CancellationToken cancellationToken = default)
    {
        var telefoneUtil = await _context.TelefonesUteis
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (telefoneUtil is null)
        {
            return false;
        }

        telefoneUtil.Nome = dto.Nome;
        telefoneUtil.Categoria = dto.Categoria;
        telefoneUtil.Telefone = dto.Telefone;
        telefoneUtil.OrdemExibicao = dto.OrdemExibicao;
        telefoneUtil.Ativo = dto.Ativo;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var telefoneUtil = await _context.TelefonesUteis
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (telefoneUtil is null)
        {
            return false;
        }

        if (telefoneUtil.Ativo == false)
        {
            return true;
        }

        telefoneUtil.Ativo = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static DTOTelefoneUtil MapToDto(TelefoneUtil telefoneUtil)
    {
        return new DTOTelefoneUtil
        {
            Id = telefoneUtil.Id,
            Nome = telefoneUtil.Nome,
            Categoria = telefoneUtil.Categoria.ToString().ToLowerInvariant(),
            Telefone = telefoneUtil.Telefone,
            OrdemExibicao = telefoneUtil.OrdemExibicao ?? 0,
            Ativo = telefoneUtil.Ativo ?? true,
        };
    }
}
