using Avalonia.Controls;

namespace SystemMonitor.Views;

/// <summary>
/// Code-behind главного окна.
/// В MVVM здесь не размещается бизнес-логика: класс только инициализирует XAML-разметку окна.
/// Все данные для отображения приходят через DataContext из MainWindowViewModel.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Создает окно и вызывает InitializeComponent, чтобы Avalonia загрузила элементы из MainWindow.axaml.
    /// После этого привязки XAML начинают читать свойства из ViewModel.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }
}
