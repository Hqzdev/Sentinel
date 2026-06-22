using System;
using System.Windows.Input;
using SystemMonitor.App.ViewModels.Base;

namespace SystemMonitor.App.ViewModels;

// главная viewmodel окна
// она хранит все экраны и решает какой экран сейчас показывается
// само окно только отображает CurrentPage и не знает деталей логики
public sealed class MainWindowModel : ViewModelBase
{
    // текущая страница которую показывает ContentControl в MainWindow.axaml
    private ViewModelBase _currentPage;

    // заголовок сверху в главном окне
    private string _title;

    private MainWindowModel(
        OverviewViewModel overview,
        CpuDetailViewModel cpuDetail,
        ProcessListViewModel processList)
    {
        // сохраняем готовые viewmodel чтобы не создавать их каждый раз заново
        // состояние экранов остаётся внутри объектов
        Overview = overview;
        CpuDetail = cpuDetail;
        ProcessList = processList;

        // при старте показываем экран обзора
        _currentPage = overview;
        _title = "Обзор системы";

        // команды привязаны к кнопкам меню в MainWindow.axaml
        ShowOverviewCommand = new RelayCommand(ShowOverview);
        ShowCpuCommand = new RelayCommand(ShowCpu);
        ShowProcessesCommand = new RelayCommand(ShowProcesses);
    }

    // отдельные экраны приложения
    public OverviewViewModel Overview { get; }
    public CpuDetailViewModel CpuDetail { get; }
    public ProcessListViewModel ProcessList { get; }

    // команды меню слева
    public ICommand ShowOverviewCommand { get; }
    public ICommand ShowCpuCommand { get; }
    public ICommand ShowProcessesCommand { get; }

    // выбранный экран
    // когда значение меняется ContentControl показывает другой view
    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    // текст заголовка для текущего раздела
    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    // Создаём всю структуру главного окна в одном месте.
    // Так App.axaml.cs не знает детали отдельных экранов.
    public static MainWindowModel Create()
    {
        var overview = new OverviewViewModel();
        var cpuDetail = new CpuDetailViewModel();
        var processList = new ProcessListViewModel();

        return new MainWindowModel(overview, cpuDetail, processList);
    }

    private void ShowOverview()
    {
        // переключаемся на главный экран с общей информацией
        Title = "Обзор системы";
        CurrentPage = Overview;
    }

    private void ShowCpu()
    {
        // переключаемся на экран подробной информации о процессоре
        Title = "Процессор";
        CurrentPage = CpuDetail;
    }

    private void ShowProcesses()
    {
        // переключаемся на список процессов
        Title = "Процессы";
        CurrentPage = ProcessList;
    }

    // простая реализация ICommand
    // нужна чтобы кнопки в xaml могли вызывать обычные методы viewmodel
    private sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            // команда всегда доступна потому что все экраны создаются при старте
            return true;
        }

        public void Execute(object? parameter)
        {
            // запускаем действие которое передали в конструктор
            _execute();
        }
    }
}
