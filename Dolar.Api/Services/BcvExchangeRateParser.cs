using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Dolar.Api.Services;

public class BcvExchangeRateParser
{
    private static readonly string[] UsdIds = { "dolar", "dólar", "usd" };
    private static readonly string[] EurIds = { "euro", "eur" };
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

        var usd = ExtractRateById(document, UsdIds) ?? ExtractRateUsingCurrencyLabel(document, UsdTerms);
        var eur = ExtractRateById(document, EurIds) ?? ExtractRateUsingCurrencyLabel(document, EurTerms);

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

    private static decimal? ExtractRateById(HtmlDocument document, string[] ids)
    {
        foreach (var id in ids)
        {
            var node = document.DocumentNode.SelectSingleNode($"//div[@id='{id}']");
            if (node is null)
            {
                continue;
            }

            var value = ParseDecimalFromNode(node);
            if (value is not null)
            {
                return value;
            }
        }

        return null;
    }

    private static decimal? ExtractRateUsingCurrencyLabel(HtmlDocument document, string[] currencyTerms)
    {
        var nodes = document.DocumentNode.SelectNodes("//div|//span|//p|//li|//td|//th|//strong");
        if (nodes is null)
        {
            return null;
        }

        foreach (var node in nodes)
        {
            var normalized = NormalizeText(node.InnerText).ToLowerInvariant();
            if (!ContainsAny(normalized, currencyTerms))
            {
                continue;
            }

            var value = ParseDecimalFromNode(node);
            if (value is not null)
            {
                return value;
            }

            value = ParseDecimalFromSiblingNodes(node);
            if (value is not null)
            {
                return value;
            }
        }

        return null;
    }

    private static decimal? ParseDecimalFromSiblingNodes(HtmlNode node)
    {
        var sibling = node.NextSibling;
        while (sibling is not null)
        {
            var value = ParseDecimalFromNode(sibling);
            if (value is not null)
            {
                return value;
            }

            sibling = sibling.NextSibling;
        }

        return node.ParentNode is not null ? ParseDecimalFromNode(node.ParentNode) : null;
    }

    private static decimal? ParseDecimalFromNode(HtmlNode node)
    {
        var candidates = node.SelectNodes(".//strong|.//b|.//span|.//td|.//th|.//div");
        if (candidates is not null)
        {
            foreach (var candidate in candidates)
            {
                if (candidate == node)
                {
                    continue;
                }

                var value = ParseDecimalFromText(candidate.InnerText);
                if (value is not null)
                {
                    return value;
                }
            }
        }

        return ParseDecimalFromText(node.InnerText);
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
