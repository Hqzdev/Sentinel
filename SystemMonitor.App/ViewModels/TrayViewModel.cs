using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SystemMonitor.App.ViewModels;

/// <summary>
/// Управляет иконкой системного трея и контекстным меню.
/// Placeholder — полная реализация через Avalonia TrayIcon в MainWindow.axaml.
/// </summary>
public sealed partial class TrayViewModel : ViewModelBase
{
    [ObservableProperty] private string _tooltipText = "Sentinel — мониторинг работает";

    [RelayCommand]
    private void ShowMainWindow()
    {
        // Поднимаем главное окно из трея
        // Реализация через WindowManager или Messenger
    }

    [RelayCommand]
    private void ExitApplication()
    {
        // Корректное завершение через IHostApplicationLifetime
        Environment.Exit(0);
    }
}
