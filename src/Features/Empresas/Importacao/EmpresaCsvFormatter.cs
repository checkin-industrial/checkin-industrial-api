using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace AppTurismoIndustrial.Api.Features.Empresas.Importacao;

/// <summary>
/// Helpers compartilhados pelos endpoints de exportacao e importacao de empresas.
/// Mantem o formato do CSV consistente entre exportar / exportar-ansi / importar.
/// </summary>
public static class EmpresaCsvFormatter
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

    public static async Task<byte[]> GerarCsvAsync(
        List<Empresa> empresas,
        Encoding encoding,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await using (var writer = new StreamWriter(stream, encoding, leaveOpen: true))
        await using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        }))
        {
            csv.WriteField("CNPJ");
            csv.WriteField("Razao_Social");
            csv.WriteField("Nome_Fantasia");
            csv.WriteField("CNAE_Principal");
            csv.WriteField("Setor");
            csv.WriteField("Porte");
            csv.WriteField("Numero_Funcionarios");
            csv.WriteField("Endereco");
            csv.WriteField("Telefone");
            csv.WriteField("CEP");
            csv.WriteField("Municipio");
            csv.WriteField("Descricao_CNAE");
            csv.WriteField("Matriz_ou_Filial");
            csv.WriteField("Latitude");
            csv.WriteField("Longitude");
            csv.WriteField("Situacao_Cadastral");
            csv.WriteField("Data_Importacao");
            csv.WriteField("Fonte_Origem");
            await csv.NextRecordAsync();

            foreach (var empresa in empresas)
            {
                csv.WriteField(FormatCnpjForSpreadsheet(empresa.Cnpj));
                csv.WriteField(empresa.RazaoSocial);
                csv.WriteField(empresa.NomeFantasia);
                csv.WriteField(empresa.CnaePrincipal);
                csv.WriteField(SetorParaImportacao(empresa.Setor));
                csv.WriteField(PorteParaImportacao(empresa.Porte));
                csv.WriteField(empresa.NumeroFuncionarios);
                csv.WriteField(empresa.Endereco);
                csv.WriteField(empresa.Telefone);
                csv.WriteField(empresa.Cep);
                csv.WriteField(empresa.Municipio);
                csv.WriteField(empresa.DescricaoCnae);
                csv.WriteField(MatrizOuFilialParaImportacao(empresa.MatrizOuFilial));
                csv.WriteField(FormatCoordinateForSpreadsheet(empresa.Latitude));
                csv.WriteField(FormatCoordinateForSpreadsheet(empresa.Longitude));
                csv.WriteField(SituacaoParaImportacao(empresa.SituacaoCadastral));
                csv.WriteField(empresa.DataCadastro.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
                csv.WriteField("Exportacao_Cadastro_Atual");
                await csv.NextRecordAsync();
            }

            await writer.FlushAsync(cancellationToken);
        }

        stream.Position = 0;
        return stream.ToArray();
    }

    private static string SetorParaImportacao(SetorEmpresa setor) => setor switch
    {
        SetorEmpresa.Industria => "industria",
        SetorEmpresa.Comercio => "comercio",
        _ => "servicos"
    };

    private static string PorteParaImportacao(PorteEmpresa porte) => porte switch
    {
        PorteEmpresa.Mei => "MEI",
        PorteEmpresa.Me => "ME",
        PorteEmpresa.Epp => "EPP",
        PorteEmpresa.Ltda => "LTDA",
        _ => "SA"
    };

    private static string SituacaoParaImportacao(SituacaoCadastral situacao) => situacao switch
    {
        SituacaoCadastral.Ativa => "ativa",
        SituacaoCadastral.Inativa => "inativa",
        SituacaoCadastral.Suspensa => "suspensa",
        _ => "baixada"
    };

    private static string MatrizOuFilialParaImportacao(MatrizOuFilialEmpresa m) => m switch
    {
        MatrizOuFilialEmpresa.Matriz => "Matriz",
        _ => "Filial"
    };

    private static string FormatCnpjForSpreadsheet(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
        {
            return string.Empty;
        }
        // Prefixo de apostrofo forca Excel a manter CNPJ como texto e evita notacao cientifica.
        return $"'{cnpj.Trim()}";
    }

    private static string FormatCoordinateForSpreadsheet(decimal coordinate)
    {
        // Excel em pt-BR interpreta corretamente virgula como separador decimal.
        return coordinate.ToString("0.######", PtBrCulture);
    }
}
