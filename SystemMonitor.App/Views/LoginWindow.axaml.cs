using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using SystemMonitor.App.ViewModels;

namespace SystemMonitor.App.Views;

public partial class LoginWindow : Window
{
    private LoginViewModel? _vm;

    public LoginWindow()
    {
        InitializeComponent();

        Opened += async (_, _) => await FadeInAsync();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // Отписываемся от старой VM
        if (_vm is not null)
        {
            _vm.ShakeRequested -= OnShakeRequested;
            _vm.LoginSucceeded -= OnLoginSucceeded;
        }

        _vm = DataContext as LoginViewModel;

        if (_vm is not null)
        {
            _vm.ShakeRequested += OnShakeRequested;
            _vm.LoginSucceeded += OnLoginSucceeded;
        }
    }

    // ── Fade-in при открытии ────────────────────────────────────────────────

    private async Task FadeInAsync()
    {
        var anim = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(500),
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(OpacityProperty, 0d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(OpacityProperty, 1d) }
                }
            }
        };
        await anim.RunAsync(this);
        Opacity = 1;
    }

    // ── Shake при ошибке ────────────────────────────────────────────────────

    private void OnShakeRequested()
    {
        // Запускаем анимацию shake на FormPanel из UI-потока
        Dispatcher.UIThread.Post(async () => await ShakeAsync());
    }

    private async Task ShakeAsync()
    {
        var panel = this.FindControl<StackPanel>("FormPanel");
        if (panel is null) return;

        var transform = new TranslateTransform();
        panel.RenderTransform = transform;

        // Используем Transitions для плавного interpolation между шагами
        transform.Transitions = new Transitions
        {
            new DoubleTransition
            {
                Property = TranslateTransform.XProperty,
                Duration = TimeSpan.FromMilliseconds(45),
                Easing = new SineEaseInOut()
            }
        };

        double[] steps = [0, -9, 9, -7, 7, -4, 4, -2, 0];
        foreach (var offset in steps)
        {
            transform.X = offset;
            await Task.Delay(45);
        }

        panel.RenderTransform = null;
    }

    // ── Slide transition к MainWindow ───────────────────────────────────────

    private void OnLoginSucceeded()
    {
        Dispatcher.UIThread.Post(async () =>
        {
            // 1. Fade-out логина
            var fadeOut = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(350),
                Easing = new CubicEaseIn(),
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(OpacityProperty, 1d) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(OpacityProperty, 0d) }
                    }
                }
            };
            await fadeOut.RunAsync(this);

            // 2. Сообщаем хост-приложению что нужно открыть MainWindow
            //    Используем событие на уровне App
            LoginWindowClosed?.Invoke();
        });
    }

    /// <summary>
    /// Вызывается когда анимация закрытия завершена — App открывает MainWindow.
    /// </summary>
    public event Action? LoginWindowClosed;

    protected override void OnClosed(EventArgs e)
    {
        if (_vm is not null)
        {
            _vm.ShakeRequested -= OnShakeRequested;
            _vm.LoginSucceeded -= OnLoginSucceeded;
        }
        base.OnClosed(e);
    }
}
