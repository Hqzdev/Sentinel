using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Services;
using SystemMonitor.Infrastructure.Monitoring;
using SystemMonitor.Infrastructure.Persistence;

namespace SystemMonitor.Infrastructure;

/// <summary>
/// Точка подключения инфраструктурного слоя к приложению.
/// Этот класс регистрирует реализации интерфейсов Core в контейнере Dependency Injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Добавляет в IServiceCollection все сервисы, которые нужны для реального мониторинга.
    /// ISettingsRepository связывается с JsonSettingsRepository, IMetricsProvider — с HardwareMetricsProvider, IProcessMetricsProvider — с ProcessMetricsProvider.
    /// MonitoringService регистрируется и как конкретный сервис, и как IHostedService, чтобы hosting-система могла запустить его в фоне.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISettingsRepository, JsonSettingsRepository>();
        services.AddSingleton<IMetricsProvider, HardwareMetricsProvider>();
        services.AddSingleton<IProcessMetricsProvider, ProcessMetricsProvider>();
        services.AddSingleton<MonitoringService>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<MonitoringService>());

        return services;
    }
}
