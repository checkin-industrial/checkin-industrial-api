

using AppTurismoIndustrial.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public class EmpresaService : IEmpresaService
{
    private readonly AppDbContext _context;

    public EmpresaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DTORespostaEmpresa> CriarAsync(DTOEmpresaCriar dto, CancellationToken cancellationToken = default)
    {
        var cnpjJaExiste = await _context.Empresas
            .AsNoTracking()
            .AnyAsync(e => e.Cnpj == dto.Cnpj, cancellationToken);

        if (cnpjJaExiste)
        {
            throw new ConflictException("Ja existe uma empresa cadastrada com este CNPJ.");
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
            Status = dto.Status ?? StatusEmpresa.Ativo,
        };

        _context.Empresas.Add(empresa);
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponseDto(empresa);
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
                Status = e.Status,
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
                Status = e.Status,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DTORespostaEmpresa> AtualizarAsync(Guid id, DTOEmpresaAtualizar dto, CancellationToken cancellationToken = default)
    {
        var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (empresa is null)
        {
            throw new NotFoundException($"Empresa {id} nao encontrada.");
        }

        var cnpjJaExisteEmOutraEmpresa = await _context.Empresas
            .AsNoTracking()
            .AnyAsync(e => e.Cnpj == dto.Cnpj && e.Id != id, cancellationToken);

        if (cnpjJaExisteEmOutraEmpresa)
        {
            throw new ConflictException("Ja existe uma empresa cadastrada com este CNPJ.");
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
        if (dto.Status.HasValue)
        {
            empresa.Status = dto.Status.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ToResponseDto(empresa);
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Soft delete: marca Status=Inativo em vez de remover a linha. Empresas
        // com Status=AguardandoRevisao (vindas de import) tambem caem para Inativo
        // via delete - admin que quiser "rejeitar" um import faz delete.
        var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (empresa is null)
        {
            throw new NotFoundException($"Empresa {id} nao encontrada.");
        }

        if (empresa.Status == StatusEmpresa.Inativo)
        {
            return;
        }

        empresa.Status = StatusEmpresa.Inativo;
        await _context.SaveChangesAsync(cancellationToken);
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
            Status = empresa.Status,
        };
    }
}
