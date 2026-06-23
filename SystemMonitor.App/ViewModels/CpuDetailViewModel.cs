using System;
using System.Collections.ObjectModel;
using System.Timers;
using Avalonia.Threading;
using SystemMonitor.App.ViewModels.Base;

namespace SystemMonitor.App.ViewModels;

// viewmodel экрана процессора
// показывает среднюю нагрузку cpu и отдельную нагрузку каждого ядра
// сейчас использует демо-значения чтобы слой app был самостоятельным
public sealed class CpuDetailViewModel : ViewModelBase
{
    // таймер обновляет показатели процессора
    private readonly System.Timers.Timer _timer;

    // random нужен только пока нет подключения к core сервисам
    private readonly Random _random = new();

    // средняя загрузка всех ядер
    private double _totalUsagePercent;

    public CpuDetailViewModel()
    {
        // коллекция нужна потому что ItemsControl в xaml умеет автоматически отображать её элементы
        Cores = new ObservableCollection<CpuCoreViewModel>();

        // создаём по одной строке на каждое логическое ядро машины
        for (var index = 0; index < Environment.ProcessorCount; index++)
        {
            Cores.Add(new CpuCoreViewModel(index + 1));
        }

        // обновляем данные немного реже чем обзор чтобы экран не дёргался слишком часто
        _timer = new System.Timers.Timer(1200);
        _timer.Elapsed += (_, _) => Dispatcher.UIThread.Post(Refresh);
        _timer.Start();

        // первое заполнение сразу после создания viewmodel
        Refresh();
    }

    // список ядер который отображается на экране CPU
    public ObservableCollection<CpuCoreViewModel> Cores { get; }

    // среднее значение загрузки CPU сверху экрана
    public double TotalUsagePercent
    {
        get => _totalUsagePercent;
        private set => SetProperty(ref _totalUsagePercent, value);
    }

    // Обновляем общую нагрузку и нагрузку по каждому ядру.
    // Значения демо потому что App слой сейчас не зависит от Core.
    public void Refresh()
    {
        var total = 0.0;

        // проходим по всем ядрам и обновляем каждое значение
        foreach (var core in Cores)
        {
            var usage = Math.Round(_random.NextDouble() * 85 + 5, 1);
            core.UsagePercent = usage;
            total += usage;
        }

        // средняя загрузка нужна для верхней общей карточки
        TotalUsagePercent = Math.Round(total / Cores.Count, 1);
    }
}

// viewmodel одной строки ядра процессора
// отдельный класс удобнее чем хранить просто double потому что у ядра есть номер и имя
public sealed class CpuCoreViewModel : ViewModelBase
{
    // текущая загрузка конкретного ядра
    private double _usagePercent;

    public CpuCoreViewModel(int number)
    {
        Number = number;
    }

    // номер ядра для отображения пользователю
    public int Number { get; }

    // готовое название строки в интерфейсе
    public string Name => $"Ядро {Number}";

    // процент загрузки этого ядра
    public double UsagePercent
    {
        get => _usagePercent;
        set => SetProperty(ref _usagePercent, value);
    }
}
