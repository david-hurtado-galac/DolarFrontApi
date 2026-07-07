using Dolar.Api.Services;

namespace Dolar.Api.Tests.Services;

public class BcvExchangeRateParserTests
{
    [Fact]
    public void Parse_ShouldExtractUsdValue_WhenHtmlContainsUsdRate()
    {
        var parser = new BcvExchangeRateParser();
        const string html = "<html><body>USD 674,93050000 Fecha Valor: Martes, 07 Julio 2026</body></html>";

        var result = parser.Parse(html);

        Assert.Equal(674.93050000m, result);
    }

    [Fact]
    public void Parse_ShouldReturnNull_WhenHtmlDoesNotContainUsdRate()
    {
        var parser = new BcvExchangeRateParser();
        const string html = "<html><body>No exchange rate available</body></html>";

        var result = parser.Parse(html);

        Assert.Null(result);
    }
}
