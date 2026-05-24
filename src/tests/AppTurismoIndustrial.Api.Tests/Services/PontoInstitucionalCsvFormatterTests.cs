using Xunit;

namespace AppTurismoIndustrial.Api.Tests.Services;

// Re-adicionado apos remocao do teste antigo (que dependia de Moq + AppDbContext
// inviavel sem options). Testes diretos do formatter cobrem os parsers de
// coordenadas (pt-BR + invariant), tipo (string e int), nullable bool/int e
// roundtrip de Tipo via GerarCsvAsync.
public class PontoInstitucionalCsvFormatterTests
{
    [Theory]
    [InlineData("-22,650000", -22.65)]
    [InlineData("-22.650000", -22.65)]
    [InlineData("  -48,80  ", -48.8)]
    [InlineData("-48.80", -48.8)]
    [InlineData("0", 0.0)]
    public void ParseNullableDecimal_AceitaPtBrEInvariant(string entrada, double esperado)
    {
        var resultado = PontoInstitucionalCsvFormatter.ParseNullableDecimal(entrada);
        Assert.NotNull(resultado);
        Assert.Equal((decimal)esperado, resultado!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nao-e-numero")]
    public void ParseNullableDecimal_ValorInvalidoOuVazio_RetornaNull(string? entrada)
    {
        Assert.Null(PontoInstitucionalCsvFormatter.ParseNullableDecimal(entrada));
    }

    [Theory]
    [InlineData("educacao", TipoPontoInstitucional.Educacao)]
    [InlineData("comercio", TipoPontoInstitucional.Comercio)]
    [InlineData("financeiro", TipoPontoInstitucional.Financeiro)]
    [InlineData("servico", TipoPontoInstitucional.Servico)]
    [InlineData("servicos", TipoPontoInstitucional.Servico)]
    [InlineData("setor_prefeitura", TipoPontoInstitucional.SetorPrefeitura)]
    [InlineData("setorprefeitura", TipoPontoInstitucional.SetorPrefeitura)]
    [InlineData("ponto_turistico", TipoPontoInstitucional.PontoTuristico)]
    [InlineData("hotel", TipoPontoInstitucional.Hotel)]
    [InlineData("ecoturismo", TipoPontoInstitucional.Ecoturismo)]
    public void ParseTipo_AceitaStringsAmigaveis(string entrada, TipoPontoInstitucional esperado)
    {
        Assert.Equal(esperado, PontoInstitucionalCsvFormatter.ParseTipo(entrada));
    }

    [Theory]
    [InlineData("1", TipoPontoInstitucional.Educacao)]
    [InlineData("8", TipoPontoInstitucional.Ecoturismo)]
    public void ParseTipo_AceitaInteiros(string entrada, TipoPontoInstitucional esperado)
    {
        Assert.Equal(esperado, PontoInstitucionalCsvFormatter.ParseTipo(entrada));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("desconhecido")]
    [InlineData("999")]
    public void ParseTipo_ValorInvalidoOuVazio_RetornaNull(string? entrada)
    {
        Assert.Null(PontoInstitucionalCsvFormatter.ParseTipo(entrada));
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("sim", true)]
    [InlineData("nao", false)]
    [InlineData("não", false)]
    [InlineData("ativo", true)]
    [InlineData("inativo", false)]
    public void ParseNullableBool_AceitaSinonimos(string entrada, bool esperado)
    {
        Assert.Equal(esperado, PontoInstitucionalCsvFormatter.ParseNullableBool(entrada));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("talvez")]
    public void ParseNullableBool_ValorInvalidoOuVazio_RetornaNull(string? entrada)
    {
        Assert.Null(PontoInstitucionalCsvFormatter.ParseNullableBool(entrada));
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("  7  ", 7)]
    public void ParseNullableInt_AceitaInteirosValidos(string entrada, int esperado)
    {
        Assert.Equal(esperado, PontoInstitucionalCsvFormatter.ParseNullableInt(entrada));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    public void ParseNullableInt_ValorInvalidoOuVazio_RetornaNull(string? entrada)
    {
        Assert.Null(PontoInstitucionalCsvFormatter.ParseNullableInt(entrada));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("  abc  ", "abc")]
    [InlineData("xyz", "xyz")]
    public void NormalizeNullable_RemoveBrancosOuRetornaNull(string? entrada, string? esperado)
    {
        Assert.Equal(esperado, PontoInstitucionalCsvFormatter.NormalizeNullable(entrada));
    }

    [Fact]
    public async Task GerarCsvAsync_ProduzCabecalhoESublinhasComLatitudePtBr()
    {
        var pontos = new List<PontoInstitucional>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Nome = "Ponto A",
                Tipo = TipoPontoInstitucional.Educacao,
                Descricao = "desc",
                Endereco = "Rua 1",
                Latitude = -22.650000m,
                Longitude = -48.800000m,
                Ativo = true,
            },
        };

        var bytes = await PontoInstitucionalCsvFormatter.GerarCsvAsync(
            pontos,
            System.Text.Encoding.UTF8,
            CancellationToken.None);

        var conteudo = System.Text.Encoding.UTF8.GetString(bytes);

        // Cabecalho deve incluir colunas chave.
        Assert.Contains("Nome", conteudo);
        Assert.Contains("Latitude", conteudo);
        Assert.Contains("Tipo", conteudo);
        // Tipo deve estar normalizado pra string (nao int).
        Assert.Contains("educacao", conteudo);
        // Latitude/Longitude formatadas pt-BR (virgula como separador decimal).
        Assert.Contains("-22,65", conteudo);
        Assert.Contains("-48,8", conteudo);
    }
}
