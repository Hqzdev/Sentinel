namespace SystemMonitor.Core.Models;

/// <summary>
/// Снимок состояния одного процесса.
/// Name берется из Process.ProcessName, CpuPct рассчитывается по изменению TotalProcessorTime, WorkingSetBytes показывает объем занятой оперативной памяти.
/// </summary>
public sealed record ProcessSnapshot(
    string Name,
    float CpuPct,
    long WorkingSetBytes);
