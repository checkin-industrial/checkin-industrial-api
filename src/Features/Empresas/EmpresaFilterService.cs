
namespace AppTurismoIndustrial.Api.Features.Empresas;

public class EmpresaFilterService : IEmpresaFilterService
{
    private readonly IEmpresaFilterQuery _empresaFilterQuery;
    private const int MaxFilterRecords = 10000;

    public EmpresaFilterService(IEmpresaFilterQuery empresaFilterQuery)
    {
        _empresaFilterQuery = empresaFilterQuery;
    }

    public async Task<IReadOnlyCollection<EmpresaFilterDTO>> FiltrarAsync(
        EmpresaFilterParams filtros,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _empresaFilterQuery.ConsultarAsync(
            filtros,
            MaxFilterRecords,
            cancellationToken);

        // Mapeamento explícito para manter o contrato de saída estável na camada de serviço.
        // Status: o EmpresaFilterQuery ja preenche, mas precisamos propagar aqui senao o
        // EmpresaFilterDTO default (StatusEmpresa.Ativo) sobrescreve no novo DTO criado,
        // fazendo TODA empresa do filter responder como "ativo" no JSON - independente
        // do estado real no banco.
        return resultado
            .Select(item => new EmpresaFilterDTO
            {
                Id = item.Id,
                NomeFantasia = item.NomeFantasia,
                CnaePrincipal = item.CnaePrincipal,
                DescricaoCnae = item.DescricaoCnae,
                Endereco = item.Endereco,
                Setor = item.Setor,
                Porte = item.Porte,
                Telefone = item.Telefone,
                Cep = item.Cep,
                Municipio = item.Municipio,
                MatrizOuFilial = item.MatrizOuFilial,
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                Status = item.Status,
            })
            .Take(MaxFilterRecords)
            .ToList();
    }
}