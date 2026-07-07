using Dolar.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dolar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly ExchangeRateService _exchangeRateService;

    public ExchangeRatesController(ExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.GetCurrentRateAsync(cancellationToken: cancellationToken);
        return result is null
            ? NotFound(new { message = "No se pudo obtener la tasa de cambio en este momento." })
            : Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var result = await _exchangeRateService.RefreshAsync(cancellationToken);
        return result is null
            ? StatusCode(502, new { message = "No se pudo actualizar la tasa de cambio." })
            : Ok(result);
    }
}
