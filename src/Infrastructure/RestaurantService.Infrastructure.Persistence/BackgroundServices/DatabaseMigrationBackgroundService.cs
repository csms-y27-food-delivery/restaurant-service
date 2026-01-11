using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RestaurantService.Infrastructure.Persistence.BackgroundServices;

public class DatabaseMigrationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _rootProvider;
    private readonly ILogger<DatabaseMigrationBackgroundService> _logger;

    public DatabaseMigrationBackgroundService(IServiceProvider rootProvider, ILogger<DatabaseMigrationBackgroundService> logger)
    {
        _rootProvider = rootProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting DB migrations...");
        using IServiceScope scope = _rootProvider.CreateScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
        _logger.LogInformation("DB migrations completed successfully.");
        return Task.CompletedTask;
    }
}