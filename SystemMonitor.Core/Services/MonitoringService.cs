using Microsoft.Extensions.Hosting;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Services;

/// <summary>
/// Фоновый сервис мониторинга.
/// Он работает как бесконечный цикл: загружает настройки, запрашивает метрики у IMetricsProvider, публикует MetricsSnapshot через событие и ждет следующий интервал.
/// Наследование от BackgroundService позволяет запускать сервис через стандартный hosting-механизм Microsoft.Extensions.Hosting.
/// </summary>
public sealed class MonitoringService : BackgroundService
{
    private readonly IMetricsProvider _provider;
    private readonly ISettingsRepository _settingsRepo;

    /// <summary>
    /// Событие нового снимка метрик.
    /// На него должна подписываться ViewModel: сервис не знает про UI, а просто сообщает, что появились свежие данные.
    /// Событие вызывается из фонового потока, поэтому ViewModel должна переносить обновление свойств в UI-поток Avalonia.
    /// </summary>
    public event Action<MetricsSnapshot>? SnapshotReceived;

    /// <summary>
    /// Получает зависимости через конструктор.
    /// provider отвечает за фактический сбор метрик, settingsRepo отвечает за чтение интервала и порогов из настроек.
    /// Такой подход соответствует Dependency Injection: сервис не создает реализации сам и остается независимым от Infrastructure.
    /// </summary>
    public MonitoringService(
        IMetricsProvider provider,
        ISettingsRepository settingsRepo)
    {
        _provider = provider;
        _settingsRepo = settingsRepo;
    }

    /// <summary>
    /// Основной рабочий цикл мониторинга.
    /// Метод вызывается hosting-системой после старта фонового сервиса и работает до тех пор, пока не будет запрошена остановка приложения.
    /// На каждом круге он заново читает настройки, поэтому изменение интервала может примениться без переписывания логики сервиса.
    /// </summary>
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
