namespace Dolar.Api.Models;

public class ExchangeRateResponse
{
    public decimal Value { get; init; }
    public DateTimeOffset RetrievedAt { get; init; }
    public string Source { get; init; } = string.Empty;
    public bool IsCached { get; init; }
}
