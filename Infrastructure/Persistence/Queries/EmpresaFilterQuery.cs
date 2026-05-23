using System.Text.RegularExpressions;
using AppTurismoIndustrial.Api.Application.DTOs;
using AppTurismoIndustrial.Api.Application.Queries;
using AppTurismoIndustrial.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Infrastructure.Persistence.Queries;

public class EmpresaFilterQuery : IEmpresaFilterQuery
{
    private readonly AppDbContext _context;
    private const int MaxFilterRecords = 10000;

    public EmpresaFilterQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<EmpresaFilterDTO>> ConsultarAsync(
        EmpresaFilterParams filtros,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var safeLimit = Math.Min(Math.Max(limit, 1), MaxFilterRecords);
        var nomeFantasiaNormalizado = string.IsNullOrWhiteSpace(filtros.NomeFantasia)
            ? null
            : filtros.NomeFantasia.Trim().ToLowerInvariant();
        var cnaeNormalizado = string.IsNullOrWhiteSpace(filtros.Cnae)
            ? null
            : Regex.Replace(filtros.Cnae, @"\D", string.Empty);
        var municipioNormalizado = string.IsNullOrWhiteSpace(filtros.Municipio)
            ? null
            : filtros.Municipio.Trim().ToLowerInvariant();
        var setor = string.IsNullOrWhiteSpace(filtros.Setor) ? null : ParseSetor(filtros.Setor);
        var porte = string.IsNullOrWhiteSpace(filtros.Porte) ? null : ParsePorte(filtros.Porte);
        var situacao = string.IsNullOrWhiteSpace(filtros.Situacao) ? null : ParseSituacao(filtros.Situacao);

        if (!string.IsNullOrWhiteSpace(filtros.Setor) && !setor.HasValue)
        {
            return [];
        }

        if (!string.IsNullOrWhiteSpace(filtros.Porte) && !porte.HasValue)
        {
            return [];
        }

        if (!string.IsNullOrWhiteSpace(filtros.Situacao) && !situacao.HasValue)
        {
            return [];
        }

        var query = _context.Empresas
            .AsNoTracking()
            .Where(e => e.Latitude != 0 && e.Longitude != 0);

        if (!string.IsNullOrWhiteSpace(nomeFantasiaNormalizado))
        {
            query = query.Where(e => e.NomeFantasia.ToLower().Contains(nomeFantasiaNormalizado));
        }

        if (!string.IsNullOrWhiteSpace(cnaeNormalizado))
        {
            query = query.Where(e => EF.Functions.Like(e.CnaePrincipal, cnaeNormalizado + "%"));
        }

        if (!string.IsNullOrWhiteSpace(municipioNormalizado))
        {
            query = query.Where(e => e.Municipio.ToLower().Contains(municipioNormalizado));
        }

        if (setor.HasValue)
        {
            query = query.Where(e => e.Setor == setor.Value);
        }

        if (porte.HasValue)
        {
            query = query.Where(e => e.Porte == porte.Value);
        }

        if (situacao.HasValue)
        {
            query = query.Where(e => e.SituacaoCadastral == situacao.Value);
        }

        if (filtros.MinFuncionarios.HasValue)
        {
            query = query.Where(e => e.NumeroFuncionarios >= filtros.MinFuncionarios.Value);
        }

        if (filtros.MaxFuncionarios.HasValue)
        {
            query = query.Where(e => e.NumeroFuncionarios <= filtros.MaxFuncionarios.Value);
        }

        var resultados = await query
            .Take(safeLimit)
            .Select(e => new
            {
                e.Id,
                e.NomeFantasia,
                e.CnaePrincipal,
                e.DescricaoCnae,
                e.Endereco,
                e.Setor,
                e.Porte,
                e.Telefone,
                e.Cep,
                e.Municipio,
                e.MatrizOuFilial,
                e.Latitude,
                e.Longitude
            })
            .ToListAsync(cancellationToken);

        return resultados
            .Select(e => new EmpresaFilterDTO
            {
                Id = e.Id,
                NomeFantasia = e.NomeFantasia,
                CnaePrincipal = e.CnaePrincipal,
                DescricaoCnae = e.DescricaoCnae,
                Endereco = e.Endereco,
                Setor = e.Setor.ToString().ToLowerInvariant(),
                Porte = e.Porte.ToString().ToUpperInvariant(),
                Telefone = e.Telefone ?? string.Empty,
                Cep = e.Cep ?? string.Empty,
                Municipio = e.Municipio,
                MatrizOuFilial = e.MatrizOuFilial.ToString(),
                Latitude = (double)e.Latitude,
                Longitude = (double)e.Longitude
            })
            .ToList();
    }

    private static SetorEmpresa? ParseSetor(string setor)
    {
        return setor.Trim().ToLowerInvariant() switch
        {
            "industria" => SetorEmpresa.Industria,
            "comercio" => SetorEmpresa.Comercio,
            "servicos" => SetorEmpresa.Servicos,
            _ => null
        };
    }

    private static PorteEmpresa? ParsePorte(string porte)
    {
        return porte.Trim().ToUpperInvariant() switch
        {
            "MEI" => PorteEmpresa.Mei,
            "ME" => PorteEmpresa.Me,
            "EPP" => PorteEmpresa.Epp,
            "LTDA" => PorteEmpresa.Ltda,
            "S/A" => PorteEmpresa.Sa,
            "SA" => PorteEmpresa.Sa,
            _ => null
        };
    }

    private static SituacaoCadastral? ParseSituacao(string situacao)
    {
        return situacao.Trim().ToLowerInvariant() switch
        {
            "ativa" => SituacaoCadastral.Ativa,
            "inativa" => SituacaoCadastral.Inativa,
            "suspensa" => SituacaoCadastral.Suspensa,
            "baixada" => SituacaoCadastral.Baixada,
            _ => null
        };
    }
}