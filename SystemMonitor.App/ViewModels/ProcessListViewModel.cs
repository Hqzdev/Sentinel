using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using Avalonia.Threading;
using SystemMonitor.App.ViewModels.Base;

namespace SystemMonitor.App.ViewModels;

// viewmodel экрана процессов
// отвечает за список строк которые отображаются в таблице процессов
// сейчас данные демонстрационные и нужны для заполнения ui слоя
public sealed class ProcessListViewModel : ViewModelBase
{
    // таймер периодически пересобирает список процессов
    private readonly Timer _timer;

    // набор имён для демонстрационных процессов
    // позже эти имена должны прийти из ProcessMetricsService
    private readonly string[] _names =
    {
        "System",
        "Explorer",
        "Browser",
        "Code",
        "Terminal",
        "Antivirus",
        "Updater",
        "Monitor"
    };

    public ProcessListViewModel()
    {
        // ObservableCollection уведомляет ui когда элементы добавляются или удаляются
        Processes = new ObservableCollection<ProcessRowViewModel>();

        // обновляем таблицу раз в полторы секунды
        _timer = new Timer(1500);
        _timer.Elapsed += (_, _) => Dispatcher.UIThread.Post(Refresh);
        _timer.Start();

        // первое заполнение таблицы сразу при создании экрана
        Refresh();
    }

    // строки таблицы процессов
    public ObservableCollection<ProcessRowViewModel> Processes { get; }

    // Пересобираем список процессов простыми демо-данными.
    // Для учебного UI этого достаточно чтобы показать таблицу и сортировку.
    public void Refresh()
    {
        // создаём новые строки и сортируем их по CPU как в диспетчере задач
        var rows = _names
            .Select((name, index) => new ProcessRowViewModel(
                index + 1000,
                name,
                Math.Round(Random.Shared.NextDouble() * 40, 1),
                Random.Shared.Next(80, 900),
                Random.Shared.Next(0, 10) > 1))
            .OrderByDescending(process => process.CpuUsagePercent)
            .ToList();

        // полностью заменяем список чтобы таблица показала новые значения
        Processes.Clear();

        foreach (var row in rows)
        {
            Processes.Add(row);
        }
    }
}

// модель одной строки таблицы процессов
// это не core модель а именно viewmodel для отображения в ui
public sealed class ProcessRowViewModel
{
    public ProcessRowViewModel(
        int processId,
        string name,
        double cpuUsagePercent,
        int memoryMegabytes,
        bool isResponding)
    {
        ProcessId = processId;
        Name = name;
        CpuUsagePercent = cpuUsagePercent;
        MemoryMegabytes = memoryMegabytes;
        IsResponding = isResponding;
    }

    // id процесса в системе
    public int ProcessId { get; }

    // имя процесса которое видит пользователь
    public string Name { get; }

    // процент CPU для сортировки и вывода в таблице
    public double CpuUsagePercent { get; }

    // память выводим сразу в мегабайтах чтобы xaml был проще
    public int MemoryMegabytes { get; }

    // true значит процесс отвечает
    public bool IsResponding { get; }

    // готовый текст статуса для таблицы
    public string Status => IsResponding ? "Работает" : "Не отвечает";
}
