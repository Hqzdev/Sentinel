namespace SystemMonitor.Core.Models;

/// <summary>
/// Настройки приложения Sentinel.
/// Thresholds хранит пороги предупреждений, а PollingIntervalSeconds задает, как часто MonitoringService должен запрашивать новые метрики.
/// </summary>
public sealed record AppSettings(
    ThresholdConfig Thresholds,
    int PollingIntervalSeconds = 5)
{
    /// <summary>
    /// Настройки по умолчанию.
    /// Используются при первом запуске или если файл настроек не найден либо не смог быть прочитан.
    /// </summary>
    public static AppSettings Default => new(
        Thresholds: ThresholdConfig.Default);
}
