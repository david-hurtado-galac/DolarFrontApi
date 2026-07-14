namespace Dolar.Api.Models;

public class ExchangeRateResponse
{
    public decimal? Usd { get; init; }
    public decimal? Eur { get; init; }
    public DateTimeOffset RetrievedAt { get; init; }
    public string Source { get; init; } = string.Empty;
    public bool IsCached { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
