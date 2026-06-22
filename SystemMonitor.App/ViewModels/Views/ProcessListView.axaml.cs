using Avalonia.Controls;

namespace SystemMonitor.App.ViewModels.Views;

// code-behind экрана процессов
// вся логика списка лежит в ProcessListViewModel
public partial class ProcessListView : UserControl
{
    // Экран процессов содержит только таблицу.
    // Заполнение таблицы делает ProcessListViewModel.
    public ProcessListView()
    {
        InitializeComponent();
    }
}
