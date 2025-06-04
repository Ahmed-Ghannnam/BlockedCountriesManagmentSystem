using BlockedCountries.BL.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TemporalBlockCleanupService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<TemporalBlockCleanupService> _logger;

    public TemporalBlockCleanupService(
        IServiceProvider provider,
        ILogger<TemporalBlockCleanupService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Temporal Block Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Running cleanup cycle at {time}", DateTime.UtcNow);

            try
            {
                using var scope = _provider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<BlockedCountryService>();

                var removed = service.RemoveExpiredBlocks();
                _logger.LogInformation("Removed {count} expired blocks", removed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup");
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}