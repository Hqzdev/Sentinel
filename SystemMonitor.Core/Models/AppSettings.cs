namespace SystemMonitor.Core.Models;

/// <summary>
/// Пользовательские настройки приложения, персистируемые через ISettingsRepository.
/// </summary>
public sealed record AppSettings(
    ThresholdConfig Thresholds,
    int PollingIntervalSeconds = 5)
{
    public static AppSettings Default => new(
        Thresholds: ThresholdConfig.Default);
}
