using Xunit;

namespace AppTurismoIndustrial.Api.Tests.Shared.Filters;

public class FilterHelpersTests
{
    [Theory]
    [InlineData("true")]
    [InlineData("True")]
    [InlineData("TRUE")]
    [InlineData(" true ")]
    [InlineData("ativo")]
    [InlineData("ATIVOS")]
    public void ParseAtivo_ReturnsTrue_ForTruthyValues(string input)
    {
        Assert.True(FilterHelpers.ParseAtivo(input));
    }

    [Theory]
    [InlineData("false")]
    [InlineData("False")]
    [InlineData("FALSE")]
    [InlineData(" false ")]
    [InlineData("inativo")]
    [InlineData("INATIVOS")]
    public void ParseAtivo_ReturnsFalse_ForFalsyValues(string input)
    {
        Assert.False(FilterHelpers.ParseAtivo(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("todos")]
    [InlineData("TODOS")]
    [InlineData("qualquer-string-invalida")]
    [InlineData("1")]   // intencional: nao aceita 1/0, so true/false
    [InlineData("0")]
    public void ParseAtivo_ReturnsNull_ForBlankOrInvalidValues(string? input)
    {
        Assert.Null(FilterHelpers.ParseAtivo(input));
    }
}
