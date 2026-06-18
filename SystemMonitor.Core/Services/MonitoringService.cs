using Microsoft.Extensions.Hosting;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Services;

/// <summary>
/// IHostedService — фоновый таймер опроса метрик.
/// Публикует MetricsSnapshot через событие SnapshotReceived.
/// </summary>
public sealed class MonitoringService : BackgroundService
{
    private readonly IMetricsProvider _provider;
    private readonly ISettingsRepository _settingsRepo;

    /// <summary>
    /// Подписчики (обычно MainViewModel) получают свежий снимок метрик.
    /// Вызывается из фонового потока — биндинг должен маршалировать в UI-поток.
    /// </summary>
    public event Action<MetricsSnapshot>? SnapshotReceived;

    public MonitoringService(
        IMetricsProvider provider,
        ISettingsRepository settingsRepo)
    {
        _provider = provider;
        _settingsRepo = settingsRepo;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var settings = await _settingsRepo.LoadAsync(stoppingToken);
            var interval = TimeSpan.FromSeconds(settings.PollingIntervalSeconds);

            var snapshot = await _provider.GetAsync(stoppingToken);

            SnapshotReceived?.Invoke(snapshot);

            await Task.Delay(interval, stoppingToken);
        }
    }
}
