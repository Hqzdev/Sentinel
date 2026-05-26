namespace SystemMonitor.Core.Models;

/// <summary>
/// Пороговые значения для срабатывания алертов (в процентах).
/// </summary>
public sealed record ThresholdConfig(
    float CpuPct = 80f,
    float RamPct = 85f,
    float DiskReadMbs = 200f,
    float DiskWriteMbs = 200f)
{
    public static ThresholdConfig Default => new();
}
