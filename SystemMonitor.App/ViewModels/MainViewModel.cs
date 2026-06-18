using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SystemMonitor.Core.Models;
using SystemMonitor.Core.Services;

namespace SystemMonitor.App.ViewModels;

/// <summary>
/// Подписывается на MonitoringService.SnapshotReceived и обновляет
/// ObservableProperty через Dispatcher.UIThread.Post.
/// </summary>
public sealed partial class MainViewModel : ViewModelBase
{
    // ── Gauge metrics ──────────────────────────────────────────────────────
    [ObservableProperty] private double _cpuUsage;
    [ObservableProperty] private double _ramUsage;
    [ObservableProperty] private double _diskReadMbs;
    [ObservableProperty] private double _diskWriteMbs;
    [ObservableProperty] private double _networkSentMbs;
    [ObservableProperty] private double _networkReceivedMbs;

    // ── Status strings ─────────────────────────────────────────────────────
    [ObservableProperty] private string _uptime = "–";
    [ObservableProperty] private string _lastUpdate = "–";
    [ObservableProperty] private string _statusText = "Ожидание...";
    [ObservableProperty] private bool _isRunning;

    // ── Sidebar nav ────────────────────────────────────────────────────────
    [ObservableProperty] private bool _dashboardActive = true;
    [ObservableProperty] private bool _settingsActive;

    // ── Static info ────────────────────────────────────────────────────────
    public string AppTitle { get; } = "Sentinel";

    // ── Formatted text helpers ─────────────────────────────────────────────
    public string CpuSubtitle    => $"{CpuUsage:F1}%";
    public string RamSubtitle    => $"{RamUsage:F1}%";
    public string DiskSubtitle   => $"{DiskReadMbs:F1} MB/s ↓  {DiskWriteMbs:F1} ↑";
    public string NetworkSubtitle=> $"↓ {NetworkReceivedMbs:F2}  ↑ {NetworkSentMbs:F2} MB/s";

    // ── Chart data ─────────────────────────────────────────────────────────
    public IReadOnlyList<ChartPoint> CpuChartPoints { get; } =
    [
        new("20:00", 32), new("20:05", 41), new("20:10", 48),
        new("20:15", 44), new("20:20", 57), new("20:25", 63), new("20:30", 68)
    ];

    // ── Process list ───────────────────────────────────────────────────────
    public IReadOnlyList<ProcessMetric> TopProcesses { get; } =
    [
        new("Visual Studio", "18%", "1.2 GB", "#3b82f6"),
        new("Chrome",        "14%", "2.1 GB", "#a855f7"),
        new("Finder",         "5%", "320 MB",  "#f97316"),
        new("System",         "3%", "180 MB",  "#10b981"),
    ];

    // ── Alert log ──────────────────────────────────────────────────────────
    public IReadOnlyList<AlertLogEntry> AlertLog { get; } =
    [
        new("20:25", "CPU",  "Загрузка процессора выше 65%", "warning"),
        new("20:18", "RAM",  "Использование памяти выше 70%", "warning"),
        new("20:05", "Disk", "Диск заполнен на 82%",          "info"),
    ];

    // ── Constructor ────────────────────────────────────────────────────────
    public MainViewModel(MonitoringService monitoringService)
    {
        monitoringService.SnapshotReceived += OnSnapshotReceived;

        // Тестовые значения для design-time и без реального железа
        CpuUsage  = 68;
        RamUsage  = 74;
        DiskReadMbs  = 42;
        DiskWriteMbs = 18;
        NetworkReceivedMbs = 0.85;
        NetworkSentMbs     = 0.12;
        Uptime = "12 ч 34 мин";
        StatusText = "Тестовый режим";
        LastUpdate = $"Обновлено: {DateTime.Now:HH:mm:ss}";
    }

    // ── Snapshot handler ───────────────────────────────────────────────────
    private void OnSnapshotReceived(MetricsSnapshot snapshot)
    {
        Dispatcher.UIThread.Post(() =>
        {
            CpuUsage   = snapshot.CpuPct;
            RamUsage   = snapshot.RamPct;
            DiskReadMbs  = snapshot.DiskReadMbs;
            DiskWriteMbs = snapshot.DiskWriteMbs;
            NetworkSentMbs     = snapshot.NetworkSentMbs;
            NetworkReceivedMbs = snapshot.NetworkReceivedMbs;
            Uptime     = FormatUptime(snapshot.Uptime);
            StatusText = "Работает";
            IsRunning  = true;
            LastUpdate = $"Обновлено: {snapshot.Timestamp:HH:mm:ss}";

            // Уведомляем форматированные строки
            OnPropertyChanged(nameof(CpuSubtitle));
            OnPropertyChanged(nameof(RamSubtitle));
            OnPropertyChanged(nameof(DiskSubtitle));
            OnPropertyChanged(nameof(NetworkSubtitle));
        });
    }

    // ── Commands ───────────────────────────────────────────────────────────
    [RelayCommand]
    private void NavigateDashboard()
    {
        DashboardActive = true;
        SettingsActive  = false;
    }

    [RelayCommand]
    private void NavigateSettings()
    {
        DashboardActive = false;
        SettingsActive  = true;
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private static string FormatUptime(TimeSpan t) =>
        t.TotalHours >= 1
            ? $"{(int)t.TotalHours} ч {t.Minutes} мин"
            : $"{t.Minutes} мин {t.Seconds} с";
}

public sealed record ProcessMetric(string Name, string Cpu, string Memory, string HexColor)
{
    public Avalonia.Media.Color DotColor =>
        Avalonia.Media.Color.TryParse(HexColor, out var c) ? c : Avalonia.Media.Colors.Gray;
    public Avalonia.Media.IBrush DotBrush =>
        new Avalonia.Media.SolidColorBrush(DotColor);
};
public sealed record AlertLogEntry(string Time, string Type, string Message, string Level)
{
    public Avalonia.Media.IBrush LevelBrush => Level switch
    {
        "warning" => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(251, 191, 36)),
        "error"   => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(239, 68, 68)),
        _         => new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(100, 116, 139)),
    };
};
public sealed record ChartPoint(string Time, int Value);
