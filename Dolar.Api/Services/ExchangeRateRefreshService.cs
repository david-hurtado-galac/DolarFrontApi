namespace Dolar.Api.Services;

public class ExchangeRateRefreshService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExchangeRateRefreshService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    public ExchangeRateRefreshService(IServiceProvider serviceProvider, ILogger<ExchangeRateRefreshService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ExchangeRateService>();
                await service.RefreshAsync(stoppingToken);
                _logger.LogInformation("Exchange rate refresh completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exchange rate refresh failed.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
