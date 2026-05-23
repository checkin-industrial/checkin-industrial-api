using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais.Importacao;

/// <summary>
/// Helpers compartilhados pelos endpoints de exportacao e importacao de pontos institucionais.
/// </summary>
public static class PontoInstitucionalCsvFormatter
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

    public static async Task<byte[]> GerarCsvAsync(
        List<PontoInstitucional> pontos,
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
            csv.WriteField("Id");
            csv.WriteField("Nome");
            csv.WriteField("Tipo");
            csv.WriteField("Descricao");
            csv.WriteField("Endereco");
            csv.WriteField("Latitude");
            csv.WriteField("Longitude");
            csv.WriteField("Atividades_Disponiveis");
            csv.WriteField("Equipe_Gestao");
            csv.WriteField("Contato_Nome");
            csv.WriteField("Contato_Telefone");
            csv.WriteField("Contato_Email");
            csv.WriteField("Responsavel_Foto_Url");
            csv.WriteField("Logo_Url");
            csv.WriteField("Card_Foto_Url");
            csv.WriteField("Cor_Marcador");
            csv.WriteField("Icone_Marcador");
            csv.WriteField("Ordem_Exibicao");
            csv.WriteField("Ativo");
            await csv.NextRecordAsync();

            foreach (var ponto in pontos)
            {
                csv.WriteField(ponto.Id);
                csv.WriteField(ponto.Nome);
                csv.WriteField(TipoParaImportacao(ponto.Tipo));
                csv.WriteField(ponto.Descricao);
                csv.WriteField(ponto.Endereco);
                csv.WriteField(ponto.Latitude.ToString("0.######", PtBrCulture));
                csv.WriteField(ponto.Longitude.ToString("0.######", PtBrCulture));
                csv.WriteField(ponto.AtividadesDisponiveis);
                csv.WriteField(ponto.EquipeGestao);
                csv.WriteField(ponto.ContatoNome);
                csv.WriteField(ponto.ContatoTelefone);
                csv.WriteField(ponto.ContatoEmail);
                csv.WriteField(ponto.ResponsavelFotoUrl);
                csv.WriteField(ponto.LogoUrl);
                csv.WriteField(ponto.CardFotoUrl);
                csv.WriteField(ponto.CorMarcador);
                csv.WriteField(ponto.IconeMarcador);
                csv.WriteField(ponto.OrdemExibicao);
                csv.WriteField(ponto.Ativo);
                await csv.NextRecordAsync();
            }

            await writer.FlushAsync(cancellationToken);
        }

        stream.Position = 0;
        return stream.ToArray();
    }

    public static string GetField(CsvReader csv, string field)
    {
        try
        {
            return csv.GetField(field) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static TipoPontoInstitucional? ParseTipo(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return null;
        }

        var normalized = tipo.Trim().ToLowerInvariant();

        if (int.TryParse(normalized, out var intTipo) && Enum.IsDefined(typeof(TipoPontoInstitucional), intTipo))
        {
            return (TipoPontoInstitucional)intTipo;
        }

        return normalized switch
        {
            "educacao" => TipoPontoInstitucional.Educacao,
            "comercio" => TipoPontoInstitucional.Comercio,
            "financeiro" => TipoPontoInstitucional.Financeiro,
            "servico" => TipoPontoInstitucional.Servico,
            "servicos" => TipoPontoInstitucional.Servico,
            "setorprefeitura" => TipoPontoInstitucional.SetorPrefeitura,
            "setor_prefeitura" => TipoPontoInstitucional.SetorPrefeitura,
            "pontoturistico" => TipoPontoInstitucional.PontoTuristico,
            "ponto_turistico" => TipoPontoInstitucional.PontoTuristico,
            "hotel" => TipoPontoInstitucional.Hotel,
            "ecoturismo" => TipoPontoInstitucional.Ecoturismo,
            _ => null
        };
    }

    private static string TipoParaImportacao(TipoPontoInstitucional tipo) => tipo switch
    {
        TipoPontoInstitucional.Educacao => "educacao",
        TipoPontoInstitucional.Comercio => "comercio",
        TipoPontoInstitucional.Financeiro => "financeiro",
        TipoPontoInstitucional.Servico => "servico",
        TipoPontoInstitucional.SetorPrefeitura => "setor_prefeitura",
        TipoPontoInstitucional.PontoTuristico => "ponto_turistico",
        TipoPontoInstitucional.Hotel => "hotel",
        _ => "ecoturismo"
    };

    public static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().Replace(" ", string.Empty);
        var hasComma = normalized.Contains(',');
        var hasDot = normalized.Contains('.');

        if (hasComma && !hasDot
            && decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, PtBrCulture, out var ptbr))
        {
            return ptbr;
        }

        if (!hasComma && hasDot
            && decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var inv))
        {
            return inv;
        }

        if (decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, PtBrCulture, out var fallbackPtBr))
        {
            return fallbackPtBr;
        }

        if (decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var fallbackInv))
        {
            return fallbackInv;
        }

        return null;
    }

    public static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            || int.TryParse(normalized, NumberStyles.Integer, PtBrCulture, out parsed))
        {
            return parsed;
        }
        return null;
    }

    public static bool? ParseNullableBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "1" => true,
            "0" => false,
            "true" => true,
            "false" => false,
            "sim" => true,
            "nao" => false,
            "não" => false,
            "ativo" => true,
            "inativo" => false,
            _ => null
        };
    }

    public static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
