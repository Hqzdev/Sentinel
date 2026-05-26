using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Services;

/// <summary>
/// Сравнивает метрики с порогами и генерирует AlertMessage.
/// Cooldown словарь предотвращает спам одинаковых алертов.
/// </summary>
public sealed class AlertEngine
{
    private readonly INotifier _notifier;
    private readonly TimeSpan _cooldown;
    private readonly Dictionary<string, DateTime> _lastAlertTime = new();

    public AlertEngine(INotifier notifier, TimeSpan? cooldown = null)
    {
        _notifier = notifier;
        _cooldown = cooldown ?? TimeSpan.FromMinutes(5);
    }

    public async Task ProcessAsync(
        MetricsSnapshot snapshot,
        ThresholdConfig thresholds,
        CancellationToken cancellationToken = default)
    {
        await CheckAndNotifyAsync("CPU", snapshot.CpuPct, thresholds.CpuPct, snapshot.Timestamp, cancellationToken);
        await CheckAndNotifyAsync("RAM", snapshot.RamPct, thresholds.RamPct, snapshot.Timestamp, cancellationToken);
        await CheckAndNotifyAsync("Disk Read", snapshot.DiskReadMbs, thresholds.DiskReadMbs, snapshot.Timestamp, cancellationToken);
        await CheckAndNotifyAsync("Disk Write", snapshot.DiskWriteMbs, thresholds.DiskWriteMbs, snapshot.Timestamp, cancellationToken);
    }

    private async Task CheckAndNotifyAsync(
        string metric,
        float value,
        float threshold,
        DateTime time,
        CancellationToken cancellationToken)
    {
        if (value < threshold) return;
        if (_lastAlertTime.TryGetValue(metric, out var last) && time - last < _cooldown) return;

        _lastAlertTime[metric] = time;
        var alert = new AlertMessage(metric, value, threshold, time);
        await _notifier.NotifyAsync(alert, cancellationToken);
    }
}
