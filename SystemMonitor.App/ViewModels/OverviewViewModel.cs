using System;
using System.Timers;
using Avalonia.Threading;
using SystemMonitor.App.ViewModels.Base;

namespace SystemMonitor.App.ViewModels;

// viewmodel экрана обзора
// хранит основные показатели которые пользователь видит сразу после запуска
// сейчас данные демонстрационные потому что app слой не подключен к core
public sealed class OverviewViewModel : ViewModelBase
{
    // таймер обновляет значения раз в секунду
    private readonly Timer _timer;

    // random нужен только для демо-метрик
    private readonly Random _random = new();

    // поля ниже хранят значения которые показываются в карточках overview
    private double _cpuUsagePercent;
    private double _memoryUsagePercent;
    private int _processCount;
    private string _lastUpdate = string.Empty;

    public OverviewViewModel()
    {
        // таймер работает не на ui потоке
        // поэтому обновление отправляем через Dispatcher.UIThread
        _timer = new Timer(1000);
        _timer.Elapsed += (_, _) => Dispatcher.UIThread.Post(Refresh);
        _timer.Start();

        // сразу заполняем экран чтобы при открытии не было пустых карточек
        Refresh();
    }

    // общий процент загрузки процессора для карточки CPU
    public double CpuUsagePercent
    {
        get => _cpuUsagePercent;
        private set => SetProperty(ref _cpuUsagePercent, value);
    }

    // общий процент использования оперативной памяти
    public double MemoryUsagePercent
    {
        get => _memoryUsagePercent;
        private set => SetProperty(ref _memoryUsagePercent, value);
    }

    // количество процессов которое показывается в третьей карточке
    public int ProcessCount
    {
        get => _processCount;
        private set => SetProperty(ref _processCount, value);
    }

    // время последнего обновления
    // нужно чтобы пользователь понимал что данные живые
    public string LastUpdate
    {
        get => _lastUpdate;
        private set => SetProperty(ref _lastUpdate, value);
    }

    // Пока Core слой не подключён к сборке, экран показывает простые демо-метрики.
    // Это нужно чтобы UI слой был заполнен и его можно было объяснить отдельно.
    public void Refresh()
    {
        CpuUsagePercent = Math.Round(_random.NextDouble() * 70 + 10, 1);
        MemoryUsagePercent = Math.Round(_random.NextDouble() * 55 + 25, 1);
        ProcessCount = Random.Shared.Next(80, 180);
        LastUpdate = DateTime.Now.ToString("HH:mm:ss");
    }
}
