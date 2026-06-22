using LibreHardwareMonitor.Hardware;
using System.Diagnostics;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Infrastructure.Monitoring;

/// <summary>
/// Реальная реализация IMetricsProvider для сбора аппаратных метрик.
/// Основной источник данных — LibreHardwareMonitor, который читает датчики CPU, памяти, дисков и сети.
/// Если библиотека не может открыться или датчики недоступны, класс возвращает fallback-снимок по текущему процессу приложения.
/// </summary>
public sealed class HardwareMetricsProvider : IMetricsProvider, IDisposable
{
    private readonly Computer? _computer;
    private readonly DateTime _startedAt = DateTime.Now;
    private DateTime _lastSampleAt = DateTime.Now;
    private TimeSpan _lastTotalProcessorTime = Process.GetCurrentProcess().TotalProcessorTime;

    /// <summary>
    /// Создает объект Computer из LibreHardwareMonitor и включает нужные группы датчиков.
    /// Если открытие датчиков завершается ошибкой, _computer остается null, а дальнейшие вызовы GetAsync используют fallback-логику.
    /// </summary>
    public HardwareMetricsProvider()
    {
        try
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsMemoryEnabled = true,
                IsStorageEnabled = true,
                IsNetworkEnabled = true,
            };
            _computer.Open();
        }
        catch
        {
            _computer = null;
        }
    }

    /// <summary>
    /// Получает один снимок метрик системы.
    /// Если LibreHardwareMonitor доступен, метод обновляет все датчики, ищет нужные значения и собирает MetricsSnapshot.
    /// Если датчики недоступны, возвращается безопасный fallback через GetFallbackSnapshot.
    /// </summary>
    public Task<MetricsSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_computer is null)
            return Task.FromResult(GetFallbackSnapshot());

        foreach (var hw in _computer.Hardware)
            hw.Update();

        float cpu = 0, ram = 0, ramUsedGb = 0, ramTotalGb = 0, diskRead = 0, diskWrite = 0, netSent = 0, netRecv = 0;

        foreach (var hw in _computer.Hardware)
        {
            switch (hw.HardwareType)
            {
                case HardwareType.Cpu:
                    cpu = GetSensorValue(hw, SensorType.Load, "CPU Total") ?? 0;
                    break;

                case HardwareType.Memory:
                    ram = GetSensorValue(hw, SensorType.Load, "Memory") ?? 0;
                    ramUsedGb = GetSensorValue(hw, SensorType.Data, "Memory Used") ?? 0;
                    ramTotalGb = GetMemoryTotalGb(hw, ramUsedGb, ram);
                    break;

                case HardwareType.Storage:
                    diskRead += GetSensorValue(hw, SensorType.Throughput, "Read Rate") ?? 0;
                    diskWrite += GetSensorValue(hw, SensorType.Throughput, "Write Rate") ?? 0;
                    break;

                case HardwareType.Network:
                    netSent += GetSensorValue(hw, SensorType.Throughput, "Upload Speed") ?? 0;
                    netRecv += GetSensorValue(hw, SensorType.Throughput, "Download Speed") ?? 0;
                    break;
            }
        }

        var snapshot = new MetricsSnapshot(
            CpuPct: cpu,
            RamPct: ram,
            RamUsedGb: ramUsedGb,
            RamTotalGb: ramTotalGb,
            DiskReadMbs: diskRead / 1_048_576f,
            DiskWriteMbs: diskWrite / 1_048_576f,
            NetworkSentMbs: netSent / 1_048_576f,
            NetworkReceivedMbs: netRecv / 1_048_576f,
            CpuCoreCount: Environment.ProcessorCount,
            CpuThreadCount: Environment.ProcessorCount,
            Uptime: TimeSpan.FromMilliseconds(Environment.TickCount64),
            Timestamp: DateTime.Now);

        return Task.FromResult(snapshot);
    }

    /// <summary>
    /// Формирует резервный снимок, когда аппаратные датчики недоступны.
    /// CPU рассчитывается по изменению TotalProcessorTime текущего процесса между двумя замерами, RAM берется из WorkingSet64.
    /// Диск и сеть в этом режиме равны нулю, потому что без аппаратных датчиков у этого fallback нет надежного источника таких данных.
    /// </summary>
    private MetricsSnapshot GetFallbackSnapshot()
    {
        using var currentProcess = Process.GetCurrentProcess();
        var now = DateTime.Now;
        var totalProcessorTime = currentProcess.TotalProcessorTime;
        var elapsed = now - _lastSampleAt;
        var processorDelta = totalProcessorTime - _lastTotalProcessorTime;

        var cpu = elapsed.TotalMilliseconds > 0
            ? processorDelta.TotalMilliseconds / (elapsed.TotalMilliseconds * Environment.ProcessorCount) * 100d
            : 0d;

        _lastSampleAt = now;
        _lastTotalProcessorTime = totalProcessorTime;

        var ram = GetProcessRamPercent(currentProcess);

        return new MetricsSnapshot(
            CpuPct: (float)Math.Clamp(cpu, 0d, 100d),
            RamPct: ram,
            RamUsedGb: currentProcess.WorkingSet64 / 1_073_741_824f,
            RamTotalGb: GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1_073_741_824f,
            DiskReadMbs: 0,
            DiskWriteMbs: 0,
            NetworkSentMbs: 0,
            NetworkReceivedMbs: 0,
            CpuCoreCount: Environment.ProcessorCount,
            CpuThreadCount: Environment.ProcessorCount,
            Uptime: now - _startedAt,
            Timestamp: now);
    }

    /// <summary>
    /// Рассчитывает процент памяти, который занимает текущий процесс относительно доступной памяти .NET runtime.
    /// Значение ограничивается диапазоном 0-100, чтобы UI не получил некорректный процент.
    /// </summary>
    private static float GetProcessRamPercent(Process process)
    {
        var totalMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        if (totalMemoryBytes <= 0) return 0;

        var percent = process.WorkingSet64 / (double)totalMemoryBytes * 100d;
        return (float)Math.Clamp(percent, 0d, 100d);
    }

    /// <summary>
    /// Ищет значение конкретного сенсора внутри устройства LibreHardwareMonitor.
    /// Фильтрация идет по типу сенсора и части имени, например Load + CPU Total или Throughput + Read Rate.
    /// Если подходящий сенсор не найден, возвращается null, а вызывающий код подставляет ноль или рассчитывает значение другим способом.
    /// </summary>
    private static float? GetSensorValue(IHardware hw, SensorType type, string namePart)
    {
        foreach (var sensor in hw.Sensors)
            if (sensor.SensorType == type && sensor.Name.Contains(namePart, StringComparison.OrdinalIgnoreCase))
                return sensor.Value;
        return null;
    }

    /// <summary>
    /// Определяет общий объем оперативной памяти в гигабайтах.
    /// Сначала пытается сложить Memory Used и Memory Available, а если доступная память не найдена, вычисляет общий объем через процент использования.
    /// Если данных недостаточно, возвращается 0, чтобы не показывать выдуманное значение.
    /// </summary>
    private static float GetMemoryTotalGb(IHardware hw, float usedGb, float usedPct)
    {
        var availableGb = GetSensorValue(hw, SensorType.Data, "Memory Available") ?? 0;
        if (usedGb > 0 && availableGb > 0)
            return usedGb + availableGb;

        if (usedGb > 0 && usedPct > 0)
            return usedGb / (usedPct / 100f);

        return 0;
    }

    /// <summary>
    /// Закрывает объект Computer и освобождает ресурсы LibreHardwareMonitor.
    /// Метод вызывается контейнером или вручную при завершении работы провайдера.
    /// </summary>
    public void Dispose() => _computer?.Close();
}
