using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SystemMonitor.ViewModels;
using SystemMonitor.Views;

namespace SystemMonitor;

/// <summary>
/// Главный класс Avalonia-приложения.
/// Он загружает XAML-ресурсы, создает главное окно и связывает окно с ViewModel по правилам MVVM.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Загружает ресурсы из App.axaml: тему, шаблоны и глобальные настройки приложения.
    /// Метод вызывается Avalonia до создания главного окна.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Завершает инициализацию фреймворка и создает главное окно для desktop-режима.
    /// MainWindow является View, а MainWindowViewModel становится DataContext, откуда XAML берет значения через Binding.
    /// В текущей версии ViewModel содержит демонстрационные данные; инфраструктурный MonitoringService реализован отдельно и не подключен здесь напрямую.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
