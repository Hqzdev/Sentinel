using Microsoft.Extensions.DependencyInjection;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Services;
using SystemMonitor.Infrastructure.Configuration;
using SystemMonitor.Infrastructure.Monitoring;
using SystemMonitor.Infrastructure.Notifications;
using SystemMonitor.Infrastructure.Persistence;
using SystemMonitor.Infrastructure.Verification;

namespace SystemMonitor.Infrastructure;

/// <summary>
/// Extension-метод для регистрации всех Infrastructure-зависимостей в DI-контейнере.
/// Вызывается из App.axaml.cs: services.AddInfrastructure()
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Configuration — читает .env файл рядом с exe
        services.AddSingleton<EnvConfig>();

        // Persistence
        services.AddSingleton<ISettingsRepository, JsonSettingsRepository>();

        // Monitoring
        services.AddSingleton<IMetricsProvider, HardwareMetricsProvider>();

        // Notifications
        services.AddSingleton<INotifier, TelegramNotifier>();

        // Telegram pairing (верификация кода при первом запуске)
        services.AddSingleton<ICodeVerificationService, CodeVerificationService>();

        // Core services
        services.AddSingleton<AlertEngine>();
        services.AddHostedService<MonitoringService>();

        return services;
    }
}
