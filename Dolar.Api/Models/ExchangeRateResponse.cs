using System;

namespace Dolar.Api.Models;

public class ExchangeRateResponse
{
    public string Currency { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public DateTimeOffset RetrievedAt { get; init; }
    public string Source { get; init; } = string.Empty;
    public bool IsCached { get; init; }
}
