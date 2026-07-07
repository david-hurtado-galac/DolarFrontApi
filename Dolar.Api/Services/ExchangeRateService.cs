using Microsoft.Extensions.Caching.Memory;

namespace Dolar.Api.Services;

public class ExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BcvExchangeRateParser _parser;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
    private const string CacheKey = "bcv:usd";

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

    public async Task<Models.ExchangeRateResponse?> GetCurrentRateAsync(bool useCache = true, CancellationToken cancellationToken = default)
    {
        if (useCache && _cache.TryGetValue(CacheKey, out Models.ExchangeRateResponse? cached))
        {
            return cached;
        }

        var rate = await FetchAndParseAsync(cancellationToken);
        if (rate is null)
        {
            return null;
        }

        _cache.Set(CacheKey, rate, _cacheDuration);
        return rate;
    }

    public async Task<Models.ExchangeRateResponse?> RefreshAsync(CancellationToken cancellationToken = default)
    {
        var rate = await FetchAndParseAsync(cancellationToken);
        if (rate is null)
        {
            return null;
        }

        _cache.Set(CacheKey, rate, _cacheDuration);
        return rate;
    }

    private async Task<Models.ExchangeRateResponse?> FetchAndParseAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient("BcvClient");
            var html = await client.GetStringAsync("https://www.bcv.org.ve/", cancellationToken);
            var value = _parser.Parse(html);

            if (value is null)
            {
                _logger.LogWarning("Unable to parse USD rate from BCV response.");
                return null;
            }

            return new Models.ExchangeRateResponse
            {
                Value = value.Value,
                RetrievedAt = DateTimeOffset.UtcNow,
                Source = "Banco Central de Venezuela",
                IsCached = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch exchange rate from BCV.");
            return null;
        }
    }
}
