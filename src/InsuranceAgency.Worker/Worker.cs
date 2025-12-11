using InsuranceAgency.Worker.Services;

namespace InsuranceAgency.Worker;

/// <summary>
/// Фоновый сервис для проверки проблемных договоров
/// </summary>
public class ProblematicContractsWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProblematicContractsWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Проверка каждый час

    public ProblematicContractsWorker(
        IServiceProvider serviceProvider,
        ILogger<ProblematicContractsWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProblematicContractsWorker started at {Time}", DateTimeOffset.Now);

        // Первая проверка сразу при запуске (с небольшой задержкой для инициализации БД)
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckProblematicContractsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking problematic contracts");
            }

            // Ожидание до следующей проверки
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("ProblematicContractsWorker stopped at {Time}", DateTimeOffset.Now);
    }

    private async Task CheckProblematicContractsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var checker = scope.ServiceProvider.GetRequiredService<ProblematicContractsChecker>();

        await checker.CheckAndProcessProblematicContractsAsync(cancellationToken);
    }
}
