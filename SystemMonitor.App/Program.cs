using Avalonia;
using System;

namespace SystemMonitor.App;

// главный файл запуска приложения
// он нужен чтобы запустить avalonia и открыть главное окно
// тут не пишем бизнес логику потому что program только стартует приложение
internal static class Program
{
    // Точка входа приложения.
    // Здесь нельзя создавать окна напрямую потому что Avalonia ещё не инициализирована.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Создаём и настраиваем Avalonia приложение.
    // Этот метод использует дизайнер Avalonia поэтому его держим отдельно.
    public static AppBuilder BuildAvaloniaApp()
    {
        // Configure<App>() говорит Avalonia какой класс приложения использовать.
        // UsePlatformDetect() сам выбирает настройки под Windows, macOS или Linux.
        // WithInterFont() подключает нормальный шрифт для интерфейса.
        // LogToTrace() пишет служебные сообщения Avalonia в отладочный вывод.
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
