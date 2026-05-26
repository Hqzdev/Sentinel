using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Interfaces;

/// <summary>
/// Абстракция источника метрик системы.
/// Реализуется в Infrastructure (LibreHardwareMonitor, WMI и т.д.).
/// </summary>
public interface IMetricsProvider
{
    Task<MetricsSnapshot> GetAsync(CancellationToken cancellationToken = default);
}
