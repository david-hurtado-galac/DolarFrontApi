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
    public void Parse_ShouldExtractUsdAndEurValues_FromActualBcvIdBlocks()
    {
        var parser = new BcvExchangeRateParser();
        const string html = @"<html><body>
            <div id='euro' class='col-sm-12 col-xs-12 '>
                <div class='field-content'>
                    <div class='row recuadrotsmc'>
                        <div class='col-sm-6 col-xs-6'><span> EUR </span></div>
                        <div class='col-sm-6 col-xs-6 centrado textp'><strong class='strong-tb'> 825,49641981</strong></div>
                    </div>
                </div>
            </div>
            <div id='dolar' class='col-sm-12 col-xs-12 '>
                <div class='field-content'>
                    <div class='row recuadrotsmc'>
                        <div class='col-sm-6 col-xs-6'><span> USD</span></div>
                        <div class='col-sm-6 col-xs-6 centrado textp'> <strong class='strong-tb'>723,99900000</strong></div>
                    </div>
                </div>
            </div>
        </body></html>";

        var result = parser.Parse(html);

        Assert.NotNull(result);
        Assert.Equal(723.99900000m, result!.Usd);
        Assert.Equal(825.49641981m, result.Eur);
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
