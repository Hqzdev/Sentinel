using Avalonia.Controls;

namespace SystemMonitor.App.ViewModels.Views;

// code-behind для экрана cpu
// файл нужен avalonia чтобы связать CpuDetailView.axaml с C# классом
public partial class CpuDetailView : UserControl
{
    // Экран CPU только отображает данные.
    // Расчёт значений находится во ViewModel.
    public CpuDetailView()
    {
        InitializeComponent();
    }
}
