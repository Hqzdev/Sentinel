using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SystemMonitor.App.ViewModels;
using SystemMonitor.App.ViewModels.Views;

namespace SystemMonitor.App;

// главный класс avalonia приложения
// он связывает xaml ресурсы и реальное главное окно
// здесь только старт ui а не логика мониторинга
public partial class App : Application
{
    // Загружаем XAML ресурсы приложения.
    // Без этого App.axaml не прочитается и стили не подключатся.
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Создаём главное окно после старта Avalonia.
    // DataContext передаёт окну модель данных для привязок.
    public override void OnFrameworkInitializationCompleted()
    {
        // Проверяем что приложение запущено как обычное desktop приложение.
        // Тогда можно создать главное окно и показать его пользователю.
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // MainWindow это оболочка интерфейса.
            // MainWindowModel хранит команды меню и текущий открытый экран.
            desktop.MainWindow = new MainWindow
            {
                DataContext = MainWindowModel.Create()
            };
        }

        // Вызываем базовую реализацию чтобы Avalonia завершила свой стандартный запуск.
        base.OnFrameworkInitializationCompleted();
    }
}
