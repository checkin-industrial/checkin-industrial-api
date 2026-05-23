using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace AppTurismoIndustrial.Api.Features.Empresas.Importacao;

/// <summary>
/// Parser de CSV para importação de empresas com suporte a streaming.
/// Processa arquivos grandes sem carregar tudo na memória.
/// </summary>
public class CsvEmpresaParser : IEmpresaImportParser
{
    public string FormatType => "CSV";

    public bool SupportsFormat(string contentType)
    {
        return contentType switch
        {
            "text/csv" => true,
            "application/csv" => true,
            "application/x-csv" => true,
            _ => false
        };
    }

    public async IAsyncEnumerable<EmpresaImportRecord> ParseAsync(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";",
            BadDataFound = null,
            MissingFieldFound = null,
            Encoding = System.Text.Encoding.UTF8
        };

        using var csv = new CsvReader(reader, config);

        var nullableDecimalOptions = new TypeConverterOptions
        {
            NullValues = { string.Empty, "Não encontrado", "Nao encontrado" }
        };
        csv.Context.TypeConverterOptionsCache.AddOptions<decimal?>(nullableDecimalOptions);

        csv.Context.RegisterClassMap<EmpresaImportRecordCsvMap>();

        var lineNumber = 1;
        if (!await csv.ReadAsync())
        {
            yield break;
        }

        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            lineNumber++;
            cancellationToken.ThrowIfCancellationRequested();

            EmpresaImportRecord? record;
            try
            {
                record = csv.GetRecord<EmpresaImportRecord>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao parsear linha {lineNumber}: {ex.Message}");
                // Fallback: não perde o CNPJ por causa de erro em outra coluna.
                record = BuildRecordFromRawFields(csv, lineNumber);
            }

            if (record == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(record.RecordId))
            {
                record.RecordId = $"LineNumber_{lineNumber}";
            }

            if (string.IsNullOrEmpty(record.FonteOrigem))
            {
                record.FonteOrigem = "CSV_Upload";
            }

            if (record.DataImportacao == null)
            {
                record.DataImportacao = DateTime.UtcNow;
            }

            yield return record;
        }
    }

    private static EmpresaImportRecord BuildRecordFromRawFields(CsvReader csv, int lineNumber)
    {
        var cnpjRaw = GetFieldOrEmpty(csv, "CNPJ");

        return new EmpresaImportRecord
        {
            RecordId = $"LineNumber_{lineNumber}",
            Cnpj = CsvParseUtils.NormalizeCnpj(cnpjRaw),
            RazaoSocial = GetFieldOrEmpty(csv, "Razao_Social"),
            NomeFantasia = GetFieldOrEmpty(csv, "Nome_Fantasia"),
            CnaePrincipal = GetFieldOrEmpty(csv, "CNAE_Principal"),
            Setor = GetFieldOrEmpty(csv, "Setor"),
            Porte = GetFieldOrEmpty(csv, "Porte"),
            NumeroFuncionarios = ParseNullableInt(GetFieldOrEmpty(csv, "Numero_Funcionarios")),
            Endereco = GetFieldOrEmpty(csv, "Endereco"),
            Telefone = GetFieldOrEmpty(csv, "Telefone"),
            Cep = GetFieldOrEmpty(csv, "CEP"),
            Municipio = GetFieldOrEmpty(csv, "Municipio"),
            DescricaoCnae = GetFieldOrEmpty(csv, "Descricao_CNAE"),
            MatrizOuFilial = GetFieldOrEmpty(csv, "Matriz_ou_Filial"),
            Latitude = CsvParseUtils.ParseNullableDecimal(GetFieldOrEmpty(csv, "Latitude")),
            Longitude = CsvParseUtils.ParseNullableDecimal(GetFieldOrEmpty(csv, "Longitude")),
            SituacaoCadastral = GetFieldOrEmpty(csv, "Situacao_Cadastral"),
            DataImportacao = CsvParseUtils.ParseNullableDateTime(GetFieldOrEmpty(csv, "Data_Importacao")) ?? DateTime.UtcNow,
            FonteOrigem = string.IsNullOrWhiteSpace(GetFieldOrEmpty(csv, "Fonte_Origem"))
                ? "CSV_Upload"
                : GetFieldOrEmpty(csv, "Fonte_Origem")
        };
    }

    private static string GetFieldOrEmpty(CsvReader csv, string columnName)
    {
        try
        {
            return csv.GetField(columnName) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static int? ParseNullableInt(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var normalized = text.Trim();
        return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            || int.TryParse(normalized, NumberStyles.Integer, CultureInfo.GetCultureInfo("pt-BR"), out value)
            ? value
            : null;
    }
}

/// <summary>
/// Mapeamento de colunas CSV para propriedades de EmpresaImportRecord.
/// </summary>
public class EmpresaImportRecordCsvMap : ClassMap<EmpresaImportRecord>
{
    public EmpresaImportRecordCsvMap()
    {
        Map(m => m.Cnpj).Name("CNPJ").Optional().TypeConverter<FlexibleCnpjConverter>();
        Map(m => m.RazaoSocial).Name("Razao_Social").Optional();
        Map(m => m.NomeFantasia).Name("Nome_Fantasia").Optional();
        Map(m => m.CnaePrincipal).Name("CNAE_Principal").Optional();
        Map(m => m.Setor).Name("Setor").Optional();
        Map(m => m.Porte).Name("Porte").Optional();
        Map(m => m.NumeroFuncionarios).Name("Numero_Funcionarios").Optional();
        Map(m => m.Endereco).Name("Endereco").Optional();
        Map(m => m.Telefone).Name("Telefone").Optional();
        Map(m => m.Cep).Name("CEP").Optional();
        Map(m => m.Municipio).Name("Municipio").Optional();
        Map(m => m.DescricaoCnae).Name("Descricao_CNAE").Optional();
        Map(m => m.MatrizOuFilial).Name("Matriz_ou_Filial").Optional();
        Map(m => m.Latitude).Name("Latitude").Optional().TypeConverter<FlexibleNullableDecimalConverter>();
        Map(m => m.Longitude).Name("Longitude").Optional().TypeConverter<FlexibleNullableDecimalConverter>();
        Map(m => m.SituacaoCadastral).Name("Situacao_Cadastral").Optional();
        Map(m => m.DataImportacao).Name("Data_Importacao").Optional().TypeConverter<FlexibleNullableDateTimeConverter>();
    }
}

public sealed class FlexibleNullableDecimalConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return CsvParseUtils.ParseNullableDecimal(text);
    }
}

public sealed class FlexibleCnpjConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return CsvParseUtils.NormalizeCnpj(text);
    }
}

public sealed class FlexibleNullableDateTimeConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return CsvParseUtils.ParseNullableDateTime(text);
    }
}

internal static class CsvParseUtils
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static decimal? ParseNullableDecimal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var normalized = text.Trim();
        if (string.Equals(normalized, "Não encontrado", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "Nao encontrado", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        normalized = normalized.Replace(" ", string.Empty);

        var hasComma = normalized.Contains(',');
        var hasDot = normalized.Contains('.');

        if (hasComma && !hasDot)
        {
            if (decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, PtBr, out var valuePtBr))
            {
                return valuePtBr;
            }
        }
        else if (!hasComma && hasDot)
        {
            if (decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, Invariant, out var valueInvariant))
            {
                return valueInvariant;
            }
        }
        else if (hasComma && hasDot)
        {
            var normalizedForInvariant = normalized;
            if (normalized.LastIndexOf(',') > normalized.LastIndexOf('.'))
            {
                normalizedForInvariant = normalizedForInvariant.Replace(".", string.Empty).Replace(',', '.');
            }
            else
            {
                normalizedForInvariant = normalizedForInvariant.Replace(",", string.Empty);
            }

            if (decimal.TryParse(normalizedForInvariant, NumberStyles.Number | NumberStyles.AllowLeadingSign, Invariant, out var valueMixed))
            {
                return valueMixed;
            }
        }

        if (decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, PtBr, out var fallbackPtBr))
        {
            return fallbackPtBr;
        }

        if (decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, Invariant, out var fallbackInvariant))
        {
            return fallbackInvariant;
        }

        return null;
    }

    public static DateTime? ParseNullableDateTime(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var normalized = Regex.Replace(text.Trim(), @"\s+", " ");
        var formats = new[]
        {
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss",
            "dd/MM/yyyy HH:mm:ss",
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy HHmmss"
        };

        if (DateTime.TryParseExact(normalized, formats, Invariant, DateTimeStyles.AssumeLocal, out var parsed)
            || DateTime.TryParseExact(normalized, formats, PtBr, DateTimeStyles.AssumeLocal, out parsed)
            || DateTime.TryParse(normalized, Invariant, DateTimeStyles.AssumeLocal, out parsed)
            || DateTime.TryParse(normalized, PtBr, DateTimeStyles.AssumeLocal, out parsed))
        {
            return parsed;
        }

        return null;
    }

    public static string NormalizeCnpj(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Trim();

        if (normalized.StartsWith("=\"", StringComparison.Ordinal) && normalized.EndsWith("\"", StringComparison.Ordinal))
        {
            normalized = normalized[2..^1];
        }

        if (normalized.StartsWith("'", StringComparison.Ordinal))
        {
            normalized = normalized[1..];
        }

        var scientificNotation = false;
        if (normalized.Contains('E', StringComparison.OrdinalIgnoreCase)
            && (decimal.TryParse(normalized, NumberStyles.Float, Invariant, out var scientific)
                || decimal.TryParse(normalized, NumberStyles.Float, PtBr, out scientific)))
        {
            scientificNotation = true;
            normalized = decimal.Truncate(scientific).ToString("0", Invariant);
        }

        var digitsOnly = Regex.Replace(normalized, @"\D", string.Empty);

        if (scientificNotation && digitsOnly.Length is > 0 and < 14)
        {
            digitsOnly = digitsOnly.PadLeft(14, '0');
        }

        return digitsOnly;
    }
}
