using Microsoft.Extensions.Caching.Memory;

namespace Dolar.Api.Services;

public class ExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BcvExchangeRateParser _parser;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExchangeRateService> _logger;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
    private const string CacheKey = "bcv:usd-eur";

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
            var rates = _parser.Parse(html);

            if (rates is null || (rates.Usd is null && rates.Eur is null))
            {
                _logger.LogWarning("Unable to parse USD or EUR rate from BCV response.");
                return null;
            }

            if (rates.Usd is null)
            {
                _logger.LogWarning("Unable to parse USD rate from BCV response.");
            }

            if (rates.Eur is null)
            {
                _logger.LogWarning("Unable to parse EUR rate from BCV response.");
            }

            return new Models.ExchangeRateResponse
            {
                Usd = rates.Usd,
                Eur = rates.Eur,
                RetrievedAt = DateTimeOffset.UtcNow,
                Source = "Banco Central de Venezuela",
                IsCached = false,
                Errors = rates.Errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch exchange rate from BCV.");
            return null;
        }
    }
}
