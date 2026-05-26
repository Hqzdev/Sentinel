using LibreHardwareMonitor.Hardware;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Infrastructure.Monitoring;

/// <summary>
/// Читает метрики через LibreHardwareMonitor и упаковывает в MetricsSnapshot.
/// Требует запуска с правами администратора для полного доступа к сенсорам.
/// </summary>
public sealed class HardwareMetricsProvider : IMetricsProvider, IDisposable
{
    private readonly Computer _computer;

    public HardwareMetricsProvider()
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

    public Task<MetricsSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        foreach (var hw in _computer.Hardware)
            hw.Update();

        float cpu = 0, ram = 0, diskRead = 0, diskWrite = 0, netSent = 0, netRecv = 0;

        foreach (var hw in _computer.Hardware)
        {
            switch (hw.HardwareType)
            {
                case HardwareType.Cpu:
                    cpu = GetSensorValue(hw, SensorType.Load, "CPU Total") ?? 0;
                    break;

                case HardwareType.Memory:
                    ram = GetSensorValue(hw, SensorType.Load, "Memory") ?? 0;
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
            DiskReadMbs: diskRead / 1_048_576f,   // bytes → MB/s
            DiskWriteMbs: diskWrite / 1_048_576f,
            NetworkSentMbs: netSent / 1_048_576f,
            NetworkReceivedMbs: netRecv / 1_048_576f,
            Uptime: TimeSpan.FromMilliseconds(Environment.TickCount64),
            Timestamp: DateTime.Now);

        return Task.FromResult(snapshot);
    }

    private static float? GetSensorValue(IHardware hw, SensorType type, string namePart)
    {
        foreach (var sensor in hw.Sensors)
            if (sensor.SensorType == type && sensor.Name.Contains(namePart, StringComparison.OrdinalIgnoreCase))
                return sensor.Value;
        return null;
    }

    public void Dispose() => _computer.Close();
}
