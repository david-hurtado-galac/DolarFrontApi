using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Dolar.Api.Services;

public class BcvExchangeRateParser
{
    private static readonly string[] UsdTerms = { "usd", "dólar", "dolar", "dólares", "dolares" };
    private static readonly string[] EurTerms = { "eur", "euro", "euros" };
    private static readonly Regex NumberRegex = new(@"\d{1,3}(?:[.,]\d{3})*(?:[.,]\d{1,8})?", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public BcvExchangeRates? Parse(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var document = new HtmlDocument();
        document.LoadHtml(html);

        var usd = ExtractRate(document, UsdTerms);
        var eur = ExtractRate(document, EurTerms);

        if (usd is null && eur is null)
        {
            return null;
        }

        var errors = new List<string>();
        if (usd is null)
        {
            errors.Add("No se pudo extraer la tasa USD.");
        }

        if (eur is null)
        {
            errors.Add("No se pudo extraer la tasa EUR.");
        }

        return new BcvExchangeRates
        {
            Usd = usd,
            Eur = eur,
            Errors = errors
        };
    }

    private static decimal? ExtractRate(HtmlDocument document, string[] currencyTerms)
    {
        var rows = document.DocumentNode.SelectNodes("//tr");
        if (rows is not null)
        {
            foreach (var row in rows)
            {
                var rowText = NormalizeText(row.InnerText).ToLowerInvariant();
                if (ContainsAny(rowText, currencyTerms))
                {
                    var rate = ParseDecimalFromRow(row, currencyTerms);
                    if (rate is not null)
                    {
                        return rate;
                    }
                }
            }
        }

        var blocks = document.DocumentNode.SelectNodes("//div|//span|//p|//li");
        if (blocks is not null)
        {
            foreach (var block in blocks)
            {
                var blockText = NormalizeText(block.InnerText).ToLowerInvariant();
                if (ContainsAny(blockText, currencyTerms))
                {
                    var rate = ParseDecimalFromNode(block);
                    if (rate is not null)
                    {
                        return rate;
                    }
                }
            }
        }

        return null;
    }

    private static decimal? ParseDecimalFromRow(HtmlNode row, string[] currencyTerms)
    {
        var cells = row.SelectNodes(".//td|.//th");
        if (cells is not null)
        {
            foreach (var cell in cells)
            {
                if (ContainsAny(NormalizeText(cell.InnerText).ToLowerInvariant(), currencyTerms))
                {
                    continue;
                }

                var value = ParseDecimalFromText(cell.InnerText);
                if (value is not null)
                {
                    return value;
                }
            }
        }

        return ParseDecimalFromText(row.InnerText);
    }

    private static decimal? ParseDecimalFromNode(HtmlNode node)
    {
        var value = ParseDecimalFromText(node.InnerText);
        if (value is not null)
        {
            return value;
        }

        var siblingText = node.ParentNode?.InnerText;
        return siblingText is not null ? ParseDecimalFromText(siblingText) : null;
    }

    private static decimal? ParseDecimalFromText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var decodedText = WebUtility.HtmlDecode(text);
        foreach (Match match in NumberRegex.Matches(decodedText))
        {
            var parsed = ParseDecimal(match.Value);
            if (parsed is not null)
            {
                return parsed;
            }
        }

        return null;
    }

    private static decimal? ParseDecimal(string rawValue)
    {
        var normalizedValue = rawValue.Trim().Replace("\u00A0", string.Empty).Replace(" ", string.Empty);
        var dotCount = normalizedValue.Count(c => c == '.');
        var commaCount = normalizedValue.Count(c => c == ',');

        if (dotCount > 0 && commaCount > 0)
        {
            normalizedValue = normalizedValue.LastIndexOf('.') > normalizedValue.LastIndexOf(',')
                ? normalizedValue.Replace(",", string.Empty)
                : normalizedValue.Replace(".", string.Empty).Replace(",", ".");
        }
        else if (commaCount > 0)
        {
            normalizedValue = normalizedValue.Replace(",", ".");
        }

        return decimal.TryParse(normalizedValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static bool ContainsAny(string text, string[] terms)
        => terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));

    private static string NormalizeText(string? text)
        => string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : Regex.Replace(WebUtility.HtmlDecode(text), @"\s+", " ").Trim();
}

public class BcvExchangeRates
{
    public decimal? Usd { get; init; }
    public decimal? Eur { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
