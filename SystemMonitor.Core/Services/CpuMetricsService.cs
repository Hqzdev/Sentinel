// PerformanceCounter  стандартный механизм Windows для чтения
// системных метрик. Используется самим Task Manager.
// Требует инициализации счётчиков при старте — это занимает время,
// поэтому сервис создаётся один раз через DI как Singleton.

using System.Diagnostics;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Services;


/// Читает метрики CPU через PerformanceCounter.
// sealed надо чтобы не создавать наследование
public sealed class CpuMetricsService : ICpuMetricsService
{
   
    // Счётчики PerformanceCounter
    // Каждый счётчик — отдельный канал чтения данных от Windows.
    // Инициализируются один раз в конструкторе.
    
    /// Счётчик суммарной загрузки всех ядер.
    /// Категория "Processor", экземпляр "_Total" — специальный экземпляр
    /// который Windows автоматически агрегирует по всем ядрам.
    /// read чтобы не вносили измененеия
    private readonly PerformanceCounter _totalCpuCounter;


    /// Счётчики загрузки каждого логического ядра по отдельности.
    /// Индекс массива = номер ядра (0, 1, 2...).

    private readonly PerformanceCounter[] _perCoreCounters;

    public int LogicalCoreCount { get; }

    // Конструктор — инициализация всех счётчиков
  

    /// Инициализирует счётчики CPU для всех логических ядер.
    /// Вызывается один раз при старте приложения через DI.

    public CpuMetricsService()
    {
        // Узнаём количество логических ядер через Environment.
        // Используем это число чтобы создать правильное количество счётчиков.
        LogicalCoreCount = Environment.ProcessorCount;

        // Создаём счётчик для суммарной загрузки всех ядер.
        // "Processor" — категория счётчиков CPU в Windows.
        // "% Processor Time" — конкретная метрика загрузки.
        // "_Total" — означающий сумму по всем ядрам.
        _totalCpuCounter = new PerformanceCounter(
            categoryName: "Processor",
            counterName: "% Processor Time",
            instanceName: "_Total"
        );

        // Создаём массив счётчиков — по одному на каждое логическое ядро.
        _perCoreCounters = new PerformanceCounter[LogicalCoreCount];

        for (int i = 0; i < LogicalCoreCount; i++)
        {
            // Каждое ядро это отдельный экземпляр с именем "0", "1", "2"...
            // Windows именует ядра числами начиная с нуля.
            _perCoreCounters[i] = new PerformanceCounter(
                categoryName: "Processor",
                counterName: "% Processor Time",
                instanceName: i.ToString()
            );
        }

        // ВАЖНО: первый вызов NextValue() на любом PerformanceCounter
        // всегда возвращает 0.0 — счётчику нужны два замера чтобы
        // посчитать дельту. Делаем холостой вызов здесь чтобы
        // первый реальный GetSnapshot() уже вернул корректные данные.
        _totalCpuCounter.NextValue();
        foreach (var counter in _perCoreCounters)
            counter.NextValue();
    }


    // Реализация интерфейса

    public CpuSnapshot GetSnapshot()
    {
        // Читаем суммарную загрузку всех ядер.
        // NextValue() блокирует поток на ~1ms и возвращает float.
        // Конвертируем в double для единообразия с остальными метриками.
        var totalUsage = (double)_totalCpuCounter.NextValue();

        // Читаем загрузку каждого ядра по отдельности.
        // Создаём массив и заполняем его значениями каждого счётчика.
        var perCoreUsage = new double[LogicalCoreCount];
        for (int i = 0; i < LogicalCoreCount; i++)
        {
            perCoreUsage[i] = (double)_perCoreCounters[i].NextValue();
        }

        // Собираем снапшот и возвращаем.
        // Timestamp — UTC чтобы не было проблем с часовыми поясами на графиках.
        return new CpuSnapshot
        {
            Timestamp = DateTime.UtcNow,
            TotalUsagePercent = totalUsage,
            PerCoreUsagePercent = perCoreUsage
        };
    }

    // Освобождение ресурсов

    // Освобождает все PerformanceCounter объекты.
    // Вызывается автоматически DI-контейнером при завершении приложения.

    public void Dispose()
    {
        // Освобождаем суммарный счётчик.
        _totalCpuCounter.Dispose();

        // Освобождаем каждый per-core счётчик.
        foreach (var counter in _perCoreCounters)
            counter.Dispose();
    }
}
