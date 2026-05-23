namespace AppTurismoIndustrial.Api.Domain.Rules;

/// <summary>
/// Validador de CNPJ seguindo as regras da Receita Federal.
/// </summary>
public static class CnpjValidator
{
    /// <summary>
    /// Valida se um CNPJ é válido (formato e dígitos verificadores).
    /// </summary>
    public static bool IsValid(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        // Remove caracteres não numéricos
        var cnpjLimpo = System.Text.RegularExpressions.Regex.Replace(cnpj, @"\D", "");

        // Deve ter exatamente 14 dígitos
        if (cnpjLimpo.Length != 14)
            return false;

        // Rejeita CNPJs com todos os dígitos iguais
        if (cnpjLimpo == new string(cnpjLimpo[0], 14))
            return false;

        // Valida primeiro dígito verificador
        var tamanho = cnpjLimpo.Length - 2;
        var numeros = cnpjLimpo.Substring(0, tamanho);
        var digitos = cnpjLimpo.Substring(tamanho);

        var soma = 0;
        var multiplicador = 2;

        for (int i = tamanho - 1; i >= 0; i--)
        {
            soma += int.Parse(numeros[i].ToString()) * multiplicador;
            multiplicador++;

            if (multiplicador > 9)
                multiplicador = 2;
        }

        var resto = soma % 11;
        var digitoUm = resto < 2 ? 0 : 11 - resto;

        if (digitos[0] != char.Parse(digitoUm.ToString()))
            return false;

        // Valida segundo dígito verificador
        tamanho = tamanho + 1;
        numeros = cnpjLimpo.Substring(0, tamanho);
        soma = 0;
        multiplicador = 2;

        for (int i = tamanho - 1; i >= 0; i--)
        {
            soma += int.Parse(numeros[i].ToString()) * multiplicador;
            multiplicador++;

            if (multiplicador > 9)
                multiplicador = 2;
        }

        resto = soma % 11;
        var digitoDois = resto < 2 ? 0 : 11 - resto;

        return digitos[1] == char.Parse(digitoDois.ToString());
    }
}
