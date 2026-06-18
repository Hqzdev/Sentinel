using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Services;
using SystemMonitor.Infrastructure.Monitoring;
using SystemMonitor.Infrastructure.Persistence;

namespace SystemMonitor.Infrastructure;

/// <summary>
/// Extension-метод для регистрации всех Infrastructure-зависимостей в DI-контейнере.
/// Вызывается из App.axaml.cs: services.AddInfrastructure()
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Persistence
        services.AddSingleton<ISettingsRepository, JsonSettingsRepository>();

        // Monitoring
        services.AddSingleton<IMetricsProvider, HardwareMetricsProvider>();

        // Core services
        services.AddSingleton<MonitoringService>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<MonitoringService>());

        return services;
    }
}
