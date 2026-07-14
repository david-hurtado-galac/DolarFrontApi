using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace Dolar.Api.Services;

public class ExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BcvExchangeRateParser _parser;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
    private const string CacheKey = "bcv:rates";

   

   

    public ExchangeRateService(
        IHttpClientFactory httpClientFactory,
        BcvExchangeRateParser parser,
        IMemoryCache cache,
        ILogger<ExchangeRateService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _parser = parser;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<Models.ExchangeRateResponse>?> GetCurrentRateAsync(bool useCache = true, CancellationToken cancellationToken = default)
    {
        if (useCache && _cache.TryGetValue(CacheKey, out List<Models.ExchangeRateResponse>? cached))
        {
            // Create a new list with IsCached set to true for each item
            var cachedWithFlag = cached
                .Select(r => new Models.ExchangeRateResponse
                {
                    Currency = r.Currency,
                    Value = r.Value,
                    RetrievedAt = r.RetrievedAt,
                    Source = r.Source,
                    IsCached = true
                })
                .ToList();
            return cachedWithFlag;
        }

        var rates = await FetchAndParseAsync(cancellationToken);
        if (rates is null)  
        {
            return null;
        }

        _cache.Set(CacheKey, rates, _cacheDuration);
        return rates;
    }

    public async Task<List<Models.ExchangeRateResponse>?> RefreshAsync(CancellationToken cancellationToken = default)
    {
        var rates = await FetchAndParseAsync(cancellationToken);
        if (rates is null)
        {
            return null;
        }

        _cache.Set(CacheKey, rates, _cacheDuration);
        return rates;
    }

    private async Task<List<Models.ExchangeRateResponse>?> FetchAndParseAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient("BcvClient");
            var html = await client.GetStringAsync("https://www.bcv.org.ve/", cancellationToken);

            var results = new List<Models.ExchangeRateResponse>();
            var now = DateTimeOffset.UtcNow;
            const string source = "Banco Central de Venezuela";

            // USD using existing parser
            var usdValue = _parser.Parse(html);
            if (usdValue is not null)
            {
                results.Add(new Models.ExchangeRateResponse
                {
                    Currency = "USD",
                    Value = usdValue.Value,
                    RetrievedAt = now,
                    Source = source,
                    IsCached = false
                });
            }
            else
            {
                _logger.LogWarning("Unable to parse USD rate from BCV response.");
            }

            // CNY using local parser logic (similar normalization & parsing)
            var cnyValue = _parser.ParseCnyFromHtml(html);
            if (cnyValue is not null)
            {
                results.Add(new Models.ExchangeRateResponse
                {
                    Currency = "CNY",
                    Value = cnyValue.Value,
                    RetrievedAt = now,
                    Source = source,
                    IsCached = false
                });
            }
            else
            {
                _logger.LogInformation("CNY rate not found in BCV response.");
            }

            if (results.Count == 0)
            {
                _logger.LogWarning("No exchange rates could be parsed from BCV.");
                return null;
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch exchange rate from BCV.");
            return null;
        }
    }
}
