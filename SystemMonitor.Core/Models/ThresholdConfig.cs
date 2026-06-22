namespace SystemMonitor.Core.Models;

/// <summary>
/// Пороговые значения, после которых приложение может считать нагрузку высокой.
/// Эти значения нужны для предупреждений: например, CPU выше 80 процентов или RAM выше 85 процентов.
/// </summary>
public sealed record ThresholdConfig(
    float CpuPct = 80f,
    float RamPct = 85f,
    float DiskReadMbs = 200f,
    float DiskWriteMbs = 200f)
{
    /// <summary>
    /// Стандартный набор порогов.
    /// Используется в AppSettings.Default и дает приложению рабочие значения без ручной настройки пользователем.
    /// </summary>
    public static ThresholdConfig Default => new();
}
