
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
                Longitude = item.Longitude
            })
            .Take(MaxFilterRecords)
            .ToList();
    }
}