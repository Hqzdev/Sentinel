namespace SystemMonitor.Core.Models;

/// <summary>
/// Снимок метрик системы в момент опроса.
/// </summary>
public sealed record MetricsSnapshot(
    float CpuPct,
    float RamPct,
    float DiskReadMbs,
    float DiskWriteMbs,
    float NetworkSentMbs,
    float NetworkReceivedMbs,
    TimeSpan Uptime,
    DateTime Timestamp);
