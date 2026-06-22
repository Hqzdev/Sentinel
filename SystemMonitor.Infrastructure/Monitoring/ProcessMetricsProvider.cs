using System.Collections.Concurrent;
using System.Diagnostics;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Infrastructure.Monitoring;

/// <summary>
/// Реализация IProcessMetricsProvider для получения списка процессов с наибольшей нагрузкой.
/// Класс использует System.Diagnostics.Process и хранит предыдущие замеры, потому что CPU процесса считается как разница процессорного времени между двумя опросами.
/// </summary>
public sealed class ProcessMetricsProvider : IProcessMetricsProvider
{
    private readonly ConcurrentDictionary<int, ProcessSample> _samples = new();

    /// <summary>
    /// Возвращает count процессов, отсортированных по CPU, а затем по памяти.
    /// Метод проходит по Process.GetProcesses, читает имя, процессорное время и WorkingSet64, после чего сравнивает текущий замер с предыдущим для того же process.Id.
    /// Если к процессу нет доступа или он завершился во время чтения, ошибка пропускается, чтобы весь мониторинг не падал из-за одного системного процесса.
    /// </summary>
    public Task<IReadOnlyList<ProcessSnapshot>> GetTopByCpuAsync(int count, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var processorCount = Environment.ProcessorCount;
        var snapshots = new List<ProcessSnapshot>();
        var alive = new HashSet<int>();

        foreach (var process in Process.GetProcesses())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (process)
                {
                    var id = process.Id;
                    var name = process.ProcessName;
                    var totalProcessorTime = process.TotalProcessorTime;
                    var workingSetBytes = process.WorkingSet64;
                    alive.Add(id);

                    var cpuPct = 0f;
                    if (_samples.TryGetValue(id, out var previous))
                    {
                        var elapsed = now - previous.Timestamp;
                        var processorDelta = totalProcessorTime - previous.TotalProcessorTime;
                        if (elapsed.TotalMilliseconds > 0)
                        {
                            var raw = processorDelta.TotalMilliseconds / (elapsed.TotalMilliseconds * processorCount) * 100d;
                            cpuPct = (float)Math.Clamp(raw, 0d, 100d);
                        }
                    }

                    _samples[id] = new ProcessSample(totalProcessorTime, now);
                    snapshots.Add(new ProcessSnapshot(name, cpuPct, workingSetBytes));
                }
            }
            catch
            {
            }
        }

        foreach (var id in _samples.Keys)
            if (!alive.Contains(id))
                _samples.TryRemove(id, out _);

        IReadOnlyList<ProcessSnapshot> top = snapshots
            .OrderByDescending(snapshot => snapshot.CpuPct)
            .ThenByDescending(snapshot => snapshot.WorkingSetBytes)
            .Take(Math.Max(1, count))
            .ToList();

        return Task.FromResult(top);
    }

    /// <summary>
    /// Внутренний предыдущий замер одного процесса.
    /// TotalProcessorTime нужен для расчета CPU, Timestamp показывает момент, когда этот замер был сохранен.
    /// </summary>
    private sealed record ProcessSample(TimeSpan TotalProcessorTime, DateTime Timestamp);
}
