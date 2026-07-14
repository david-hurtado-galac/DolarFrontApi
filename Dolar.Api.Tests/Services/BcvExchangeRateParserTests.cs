using Dolar.Api.Services;

namespace Dolar.Api.Tests.Services;

public class BcvExchangeRateParserTests
{
    [Fact]
    public void Parse_ShouldExtractUsdAndEurValues_WhenHtmlContainsBothRates()
    {
        var parser = new BcvExchangeRateParser();
        const string html = "<html><body><table><tr><td>USD</td><td>674,93050000</td></tr><tr><td>EUR</td><td>728,12430000</td></tr></table></body></html>";

        var result = parser.Parse(html);

        Assert.NotNull(result);
        Assert.Equal(674.93050000m, result!.Usd);
        Assert.Equal(728.12430000m, result.Eur);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Parse_ShouldReturnNull_WhenHtmlDoesNotContainRates()
    {
        var parser = new BcvExchangeRateParser();
        const string html = "<html><body>No exchange rate available</body></html>";

        var result = parser.Parse(html);

        Assert.Null(result);
    }
}
