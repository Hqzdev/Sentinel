using Avalonia;
using System;

namespace SystemMonitor;

/// <summary>
/// Точка входа в настольное приложение Sentinel.
/// Этот класс запускается первым, создает конфигурацию Avalonia и передает управление оконному циклу приложения.
/// </summary>
sealed class Program
{
    /// <summary>
    /// Главная функция программы.
    /// Атрибут STAThread нужен для корректной работы настольного UI: окон, событий мыши, клавиатуры и системных компонентов Avalonia.
    /// args приходят от операционной системы при запуске приложения и передаются дальше в Avalonia lifetime.
    /// </summary>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// Собирает объект AppBuilder, который описывает, как именно запускать Avalonia-приложение.
    /// Здесь подключается класс App, автоматическое определение платформы, инструменты разработчика в Debug-режиме, шрифт Inter и логирование.
    /// Этот метод также используется дизайнером Avalonia, поэтому через него проходит и обычный запуск, и предпросмотр интерфейса.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
