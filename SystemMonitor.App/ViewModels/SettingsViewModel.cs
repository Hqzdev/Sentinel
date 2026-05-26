using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.App.ViewModels;

/// <summary>
/// ViewModel для окна настроек: пороги алертов и Telegram-конфигурация.
/// </summary>
public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsRepository _settingsRepo;
    private readonly INotifier _notifier;

    // BotToken живёт в .env — здесь только read-only ChatId
    [ObservableProperty] private string _chatId = string.Empty;
    [ObservableProperty] private float _cpuThreshold = 80f;
    [ObservableProperty] private float _ramThreshold = 85f;
    [ObservableProperty] private int _pollingIntervalSeconds = 5;
    [ObservableProperty] private string _saveStatus = string.Empty;

    public SettingsViewModel(ISettingsRepository settingsRepo, INotifier notifier)
    {
        _settingsRepo = settingsRepo;
        _notifier = notifier;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var s = await _settingsRepo.LoadAsync();
        ChatId = s.ChatId;
        CpuThreshold = s.Thresholds.CpuPct;
        RamThreshold = s.Thresholds.RamPct;
        PollingIntervalSeconds = s.PollingIntervalSeconds;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Сохраняем порог и интервал; ChatId не меняем (управляется через Telegram pairing)
        var existing = await _settingsRepo.LoadAsync();
        var settings = existing with
        {
            Thresholds = new ThresholdConfig(CpuPct: CpuThreshold, RamPct: RamThreshold),
            PollingIntervalSeconds = PollingIntervalSeconds
        };

        await _settingsRepo.SaveAsync(settings);
        SaveStatus = "✓ Сохранено";
    }

    [RelayCommand]
    private async Task TestNotificationAsync()
    {
        var testAlert = new AlertMessage("TEST", 99f, 80f, DateTime.Now);
        await _notifier.NotifyAsync(testAlert);
        SaveStatus = "✓ Тестовое сообщение отправлено";
    }
}
