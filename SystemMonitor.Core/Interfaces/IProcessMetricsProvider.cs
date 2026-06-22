using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Interfaces;

/// <summary>
/// Интерфейс источника информации о процессах.
/// Он нужен отдельно от IMetricsProvider, потому что список процессов — это детальная таблица, а MetricsSnapshot — общий снимок состояния системы.
/// </summary>
public interface IProcessMetricsProvider
{
    /// <summary>
    /// Возвращает count процессов, отсортированных по загрузке CPU.
    /// Данные используются для блока "Самые нагруженные процессы" и могут обновляться независимо от общих метрик системы.
    /// </summary>
    Task<IReadOnlyList<ProcessSnapshot>> GetTopByCpuAsync(int count, CancellationToken cancellationToken = default);
}
