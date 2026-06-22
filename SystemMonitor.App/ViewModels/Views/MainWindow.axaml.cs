using Avalonia.Controls;

namespace SystemMonitor.App.ViewModels.Views;

// code-behind главного окна
// в нём нет бизнес логики потому что логика находится в MainWindowModel
public partial class MainWindow : Window
{
    // Code-behind нужен только для InitializeComponent.
    // Логика окна находится в MainWindowModel.
    public MainWindow()
    {
        InitializeComponent();
    }
}
