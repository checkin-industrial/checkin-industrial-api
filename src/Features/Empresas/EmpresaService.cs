

using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public class EmpresaService : IEmpresaService
{
    private readonly AppDbContext _context;

    public EmpresaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(DTORespostaEmpresa? empresa, bool cnpjDuplicado)> CriarAsync(DTOEmpresaCriar dto, CancellationToken cancellationToken = default)
    {
        var cnpjJaExiste = await _context.Empresas
            .AsNoTracking()
            .AnyAsync(e => e.Cnpj == dto.Cnpj, cancellationToken);

        if (cnpjJaExiste)
        {
            return (null, true);
        }

        var empresa = new Empresa
        {
            Cnpj = dto.Cnpj,
            RazaoSocial = dto.RazaoSocial,
            NomeFantasia = dto.NomeFantasia,
            CnaePrincipal = dto.CnaePrincipal,
            DescricaoCnae = dto.DescricaoCnae,
            Setor = dto.Setor,
            Porte = dto.Porte,
            NumeroFuncionarios = dto.NumeroFuncionarios,
            Endereco = dto.Endereco,
            Telefone = dto.Telefone,
            Cep = dto.Cep,
            Municipio = dto.Municipio,
            MatrizOuFilial = dto.MatrizOuFilial,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            SituacaoCadastral = dto.SituacaoCadastral,
            DataCadastro = dto.DataCadastro ?? DateTime.UtcNow,
            Ativo = dto.Ativo ?? true,
        };

        _context.Empresas.Add(empresa);
        await _context.SaveChangesAsync(cancellationToken);

        return (ToResponseDto(empresa), false);
    }

    public async Task<IReadOnlyCollection<DTORespostaEmpresa>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var empresas = await _context.Empresas
            .AsNoTracking()
            .Select(e => new DTORespostaEmpresa
            {
                Id = e.Id,
                Cnpj = e.Cnpj ?? string.Empty,
                RazaoSocial = e.RazaoSocial,
                NomeFantasia = e.NomeFantasia,
                CnaePrincipal = e.CnaePrincipal,
                DescricaoCnae = e.DescricaoCnae,
                Telefone = e.Telefone ?? string.Empty,
                Cep = e.Cep ?? string.Empty,
                Municipio = e.Municipio,
                MatrizOuFilial = e.MatrizOuFilial.ToString(),
                MatrizOuFilialCodigo = e.MatrizOuFilial,
                Setor = e.Setor,
                Porte = e.Porte,
                NumeroFuncionarios = e.NumeroFuncionarios ?? 0,
                Endereco = e.Endereco,
                SituacaoCadastral = e.SituacaoCadastral,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                CreatedAt = e.DataCadastro,
                Ativo = e.Ativo ?? true,
            })
            .ToListAsync(cancellationToken);

        return empresas;
    }

    public async Task<DTORespostaEmpresa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Empresas
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new DTORespostaEmpresa
            {
                Id = e.Id,
                Cnpj = e.Cnpj ?? string.Empty,
                RazaoSocial = e.RazaoSocial,
                NomeFantasia = e.NomeFantasia,
                CnaePrincipal = e.CnaePrincipal,
                DescricaoCnae = e.DescricaoCnae,
                Telefone = e.Telefone ?? string.Empty,
                Cep = e.Cep ?? string.Empty,
                Municipio = e.Municipio,
                MatrizOuFilial = e.MatrizOuFilial.ToString(),
                MatrizOuFilialCodigo = e.MatrizOuFilial,
                Setor = e.Setor,
                Porte = e.Porte,
                NumeroFuncionarios = e.NumeroFuncionarios ?? 0,
                Endereco = e.Endereco,
                SituacaoCadastral = e.SituacaoCadastral,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                CreatedAt = e.DataCadastro,
                Ativo = e.Ativo ?? true,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(bool atualizado, bool naoEncontrada, bool cnpjDuplicado)> AtualizarAsync(Guid id, DTOEmpresaAtualizar dto, CancellationToken cancellationToken = default)
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (empresa is null)
        {
            return (false, true, false);
        }

        var cnpjJaExisteEmOutraEmpresa = await _context.Empresas
            .AsNoTracking()
            .AnyAsync(e => e.Cnpj == dto.Cnpj && e.Id != id, cancellationToken);

        if (cnpjJaExisteEmOutraEmpresa)
        {
            return (false, false, true);
        }

        empresa.Cnpj = dto.Cnpj;
        empresa.RazaoSocial = dto.RazaoSocial;
        empresa.NomeFantasia = dto.NomeFantasia;
        empresa.CnaePrincipal = dto.CnaePrincipal;
        empresa.DescricaoCnae = dto.DescricaoCnae;
        empresa.Setor = dto.Setor;
        empresa.Porte = dto.Porte;
        empresa.NumeroFuncionarios = dto.NumeroFuncionarios;
        empresa.Endereco = dto.Endereco;
        empresa.Telefone = dto.Telefone;
        empresa.Cep = dto.Cep;
        empresa.Municipio = dto.Municipio;
        empresa.MatrizOuFilial = dto.MatrizOuFilial;
        empresa.Latitude = dto.Latitude;
        empresa.Longitude = dto.Longitude;
        empresa.SituacaoCadastral = dto.SituacaoCadastral;
        empresa.DataCadastro = dto.DataCadastro ?? empresa.DataCadastro;
        if (dto.Ativo.HasValue)
        {
            empresa.Ativo = dto.Ativo.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return (true, false, false);
    }

    public async Task<bool> RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Soft delete consistente com TelefoneUtil e PontoInstitucional: marca
        // Ativo=false em vez de remover a linha. Futuro painel de reativacao
        // listara empresas com Ativo=false e permitira restaurar via Update.
        var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (empresa is null)
        {
            return false;
        }

        if (empresa.Ativo == false)
        {
            return true;
        }

        empresa.Ativo = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static DTORespostaEmpresa ToResponseDto(Empresa empresa)
    {
        return new DTORespostaEmpresa
        {
            Id = empresa.Id,
            Cnpj = empresa.Cnpj ?? string.Empty,
            RazaoSocial = empresa.RazaoSocial,
            NomeFantasia = empresa.NomeFantasia,
            CnaePrincipal = empresa.CnaePrincipal,
            DescricaoCnae = empresa.DescricaoCnae,
            Telefone = empresa.Telefone ?? string.Empty,
            Cep = empresa.Cep ?? string.Empty,
            Municipio = empresa.Municipio,
            MatrizOuFilial = empresa.MatrizOuFilial.ToString(),
            MatrizOuFilialCodigo = empresa.MatrizOuFilial,
            Setor = empresa.Setor,
            Porte = empresa.Porte,
            NumeroFuncionarios = empresa.NumeroFuncionarios ?? 0,
            Endereco = empresa.Endereco,
            SituacaoCadastral = empresa.SituacaoCadastral,
            Latitude = empresa.Latitude,
            Longitude = empresa.Longitude,
            CreatedAt = empresa.DataCadastro,
            Ativo = empresa.Ativo ?? true,
        };
    }
}
