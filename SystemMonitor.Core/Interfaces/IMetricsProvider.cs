using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Interfaces;

/// <summary>
/// Интерфейс источника системных метрик.
/// Core описывает только контракт: приложение просит MetricsSnapshot, но не знает, берутся ли данные из LibreHardwareMonitor, WMI, операционной системы или тестовой заглушки.
/// Реальная реализация находится в Infrastructure, чтобы бизнес-логика не зависела от конкретной внешней библиотеки.
/// </summary>
public interface IMetricsProvider
{
    /// <summary>
    /// Асинхронно получает один актуальный снимок состояния компьютера.
    /// cancellationToken приходит от фонового сервиса и позволяет остановить чтение метрик при закрытии приложения.
    /// </summary>
    Task<MetricsSnapshot> GetAsync(CancellationToken cancellationToken = default);
}
