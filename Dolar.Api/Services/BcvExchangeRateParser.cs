using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace Dolar.Api.Services;

public class BcvExchangeRateParser
{
    private static readonly Regex CurrencyContextRegex = new(
        @"(?i)(?:USD|Fecha Valor)[^<]{0,120}?(?<value>\d{1,3}(?:[.,]\d{1,8})+)",
        RegexOptions.CultureInvariant);

    private static readonly Regex GeneralNumberRegex = new(
        @"(?<value>\d{1,3}(?:[.,]\d{1,8})+)",
        RegexOptions.CultureInvariant);

    public decimal? Parse(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var normalized = WebUtility.HtmlDecode(html);
        normalized = Regex.Replace(normalized, "<[^>]+>", " ");
        normalized = Regex.Replace(normalized, @"\s+", " ");

        var contextMatch = CurrencyContextRegex.Match(normalized);
        if (contextMatch.Success)
        {
            return ParseCandidate(contextMatch.Groups["value"].Value);
        }

        foreach (Match match in GeneralNumberRegex.Matches(normalized))
        {
            var value = ParseCandidate(match.Groups["value"].Value);
            if (value is not null)
            {
                return value;
            }
        }

        return null;
    }

    private static decimal? ParseCandidate(string rawValue)
    {
        var normalizedValue = rawValue.Replace(".", "").Replace(",", ".");
        return decimal.TryParse(normalizedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }
}
