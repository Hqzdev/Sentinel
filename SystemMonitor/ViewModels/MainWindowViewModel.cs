using System.Collections.Generic;

namespace SystemMonitor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string AppTitle { get; } = "Sentinel";

    public string StatusText { get; } = "Тестовый режим";

    public int CpuUsage { get; } = 68;

    public int RamUsage { get; } = 74;

    public int DiskUsage { get; } = 82;

    public string Uptime { get; } = "12 ч 34 мин";

    public string CpuUsageText => $"{CpuUsage}%";

    public string RamUsageText => $"{RamUsage}%";

    public string DiskUsageText => $"{DiskUsage}%";

    public string TelegramStatus { get; } = "Telegram-бот подключен";

    public string LastUpdate { get; } = "Последнее обновление: 13.05.2026 20:30";

    public IReadOnlyList<ChartPoint> CpuChartPoints { get; } =
    [
        new("20:00", 32),
        new("20:05", 41),
        new("20:10", 48),
        new("20:15", 44),
        new("20:20", 57),
        new("20:25", 63),
        new("20:30", 68)
    ];

    public IReadOnlyList<ProcessMetric> TopProcesses { get; } =
    [
        new("Visual Studio", "18%", "1.2 GB"),
        new("Chrome", "14%", "2.1 GB"),
        new("Telegram", "5%", "320 MB"),
        new("System", "3%", "180 MB")
    ];

    public IReadOnlyList<AlertLogEntry> AlertLog { get; } =
    [
        new("20:25", "CPU", "Загрузка процессора выше 65%", "Warning"),
        new("20:18", "RAM", "Использование памяти выше 70%", "Warning"),
        new("20:05", "Disk", "Диск заполнен на 82%", "Info")
    ];
}

public sealed record ProcessMetric(string Name, string Cpu, string Memory);

public sealed record AlertLogEntry(string Time, string Type, string Message, string Level);

public sealed record ChartPoint(string Time, int Value);
