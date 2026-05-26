using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemMonitor.App.ViewModels;
using SystemMonitor.App.Views;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Infrastructure;

namespace SystemMonitor.App;

public partial class App : Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddInfrastructure();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<TrayViewModel>();
                services.AddTransient<LoginViewModel>();
            })
            .Build();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit += (_, _) => _host.StopAsync().GetAwaiter().GetResult();

            // Проверяем нужен ли экран входа (показываем если ChatId ещё не привязан)
            var settingsRepo = _host.Services.GetRequiredService<ISettingsRepository>();
            var settings = settingsRepo.LoadAsync().GetAwaiter().GetResult();
            bool needsSetup = string.IsNullOrWhiteSpace(settings.ChatId);

            if (needsSetup)
            {
                // ── Первый запуск: показываем LoginWindow ──────────────────
                var loginVm = _host.Services.GetRequiredService<LoginViewModel>();
                var loginWindow = new LoginWindow { DataContext = loginVm };

                loginWindow.LoginWindowClosed += () =>
                {
                    // Fade-in MainWindow после закрытия логина
                    Dispatcher.UIThread.Post(async () =>
                    {
                        var mainWindow = new MainWindow
                        {
                            DataContext = _host.Services.GetRequiredService<MainViewModel>(),
                            Opacity = 0
                        };
                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();

                        // Slide-in + fade-in главного окна
                        await SlideInAsync(mainWindow);

                        loginWindow.Close();

                        // Запускаем background monitoring
                        await _host.StartAsync();
                    });
                };

                desktop.MainWindow = loginWindow;
            }
            else
            {
                // ── Повторный запуск: сразу главное окно ──────────────────
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _host.Services.GetRequiredService<MainViewModel>(),
                };
                _host.StartAsync();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    // ── Slide-in анимация главного окна ────────────────────────────────────

    private static async Task SlideInAsync(Window window)
    {
        // Начальный сдвиг вниз
        window.RenderTransform = new TranslateTransform { Y = 30 };

        // Параллельно: fade-in + slide-up
        var fadeAnim = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(450),
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(Visual.OpacityProperty, 0d) } },
                new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(Visual.OpacityProperty, 1d) } }
            }
        };

        var transform = (TranslateTransform)window.RenderTransform;
        transform.Transitions = new Transitions
        {
            new DoubleTransition
            {
                Property = TranslateTransform.YProperty,
                Duration = TimeSpan.FromMilliseconds(450),
                Easing = new CubicEaseOut()
            }
        };

        // Запускаем обе анимации
        var fadeTask = fadeAnim.RunAsync(window);
        transform.Y = 0;

        await fadeTask;

        window.Opacity = 1;
        window.RenderTransform = null;
    }
}
