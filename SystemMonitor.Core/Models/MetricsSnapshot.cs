namespace SystemMonitor.Core.Models;

/// <summary>
/// Один снимок состояния компьютера в конкретный момент времени.
/// Такой объект создает IMetricsProvider, затем MonitoringService передает его подписчикам, а UI может разложить поля по карточкам CPU, RAM, Disk и Network.
/// </summary>
public sealed record MetricsSnapshot(
    float CpuPct,
    float RamPct,
    float RamUsedGb,
    float RamTotalGb,
    float DiskReadMbs,
    float DiskWriteMbs,
    float NetworkSentMbs,
    float NetworkReceivedMbs,
    int CpuCoreCount,
    int CpuThreadCount,
    TimeSpan Uptime,
    DateTime Timestamp);
