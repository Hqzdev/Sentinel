namespace SystemMonitor.Core.Models;

/// <summary>
/// Пользовательские настройки приложения, персистируемые через ISettingsRepository.
/// </summary>
public sealed record AppSettings(
    string BotToken,
    string ChatId,
    ThresholdConfig Thresholds,
    int PollingIntervalSeconds = 5)
{
    public static AppSettings Default => new(
        BotToken: string.Empty,
        ChatId: string.Empty,
        Thresholds: ThresholdConfig.Default);
}
