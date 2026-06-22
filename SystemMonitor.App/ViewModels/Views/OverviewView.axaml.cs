using Avalonia.Controls;

namespace SystemMonitor.App.ViewModels.Views;

// code-behind экрана обзора
// нужен только чтобы Avalonia могла загрузить OverviewView.axaml
public partial class OverviewView : UserControl
{
    // Экран обзора не содержит логики.
    // Все данные приходят из OverviewViewModel.
    public OverviewView()
    {
        InitializeComponent();
    }
}
