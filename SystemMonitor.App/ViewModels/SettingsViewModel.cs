using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.App.ViewModels;

/// <summary>
/// ViewModel для окна настроек: пороги алертов и параметры мониторинга.
/// </summary>
public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsRepository _settingsRepo;

    [ObservableProperty] private float _cpuThreshold = 80f;
    [ObservableProperty] private float _ramThreshold = 85f;
    [ObservableProperty] private int _pollingIntervalSeconds = 5;
    [ObservableProperty] private string _saveStatus = string.Empty;

    public SettingsViewModel(ISettingsRepository settingsRepo)
    {
        _settingsRepo = settingsRepo;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var s = await _settingsRepo.LoadAsync();
        CpuThreshold = s.Thresholds.CpuPct;
        RamThreshold = s.Thresholds.RamPct;
        PollingIntervalSeconds = s.PollingIntervalSeconds;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var existing = await _settingsRepo.LoadAsync();
        var settings = existing with
        {
            Thresholds = new ThresholdConfig(CpuPct: CpuThreshold, RamPct: RamThreshold),
            PollingIntervalSeconds = PollingIntervalSeconds
        };

        await _settingsRepo.SaveAsync(settings);
        SaveStatus = "✓ Сохранено";
    }
}
