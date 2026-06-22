
// Сложнее двух предыдущих сервисов потому что:
// 1. Работает со списком процессов, а не одним значением
// 2. Хранит состояние между вызовами для подсчёта дельты CPU
// 3. Должен обрабатывать процессы которые исчезли между вызовами

using System.Diagnostics;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Services;


/// Читает метрики всех запущенных процессов через System.Diagnostics.Process.
/// Хранит предыдущие замеры CPU времени для подсчёта дельты.

public sealed class ProcessMetricsService : IProcessMetricsService

    // Состояние между вызовами


    /// Предыдущие замеры CPU времени каждого процесса.
    /// Ключ: PID процесса.
    /// Значение: кортеж (сколько всего CPU времени процесс потратил, когда мы это замерили).
    /// Нужен чтобы посчитать сколько CPU времени процесс потратил ЗА ПОСЛЕДНИЙ ИНТЕРВАЛ.

    private Dictionary<int, (TimeSpan CpuTime, DateTime MeasuredAt)> _previousMeasurements
        = new();


    public double CpuThresholdPercent { get; set; } = 0.0;

    // Реализация интерфейса

    public IReadOnlyList<ProcessEntry> GetSnapshot()
    {
        // Запоминаем точное время начала замера.
        // Используем это чтобы посчитать реальный интервал между вызовами.
        // Нельзя использовать фиксированный интервал — вызовы могут
        // приходить с разными задержками.
        var now = DateTime.UtcNow;

        // Получаем список всех процессов от OS.
        // Это относительно дорогой вызов — 50-200ms на большинстве систем.
        // Поэтому GetSnapshot() должен вызываться только из фонового потока.
        Process[] processes = Process.GetProcesses();

        // Список результатов который будем заполнять.
        var results = new List<ProcessEntry>(processes.Length);

        // Новый словарь замеров который заменит _previousMeasurements
        // в конце этого вызова.
        var newMeasurements = new Dictionary<int, (TimeSpan, DateTime)>(processes.Length);

        foreach (var process in processes)
        {
            try
            {
                // Читаем сколько всего CPU времени процесс потратил с момента запуска.
                // Это накопительное значение — растёт монотонно.
                // Само по себе бесполезно — нам нужна дельта за последний интервал.
                var currentCpuTime = process.TotalProcessorTime;

                // Читаем потребление RAM (Working Set).
                // WorkingSet64 — реальная физическая память без свопа.
                var memoryBytes = process.WorkingSet64;

                // Считаем CPU% только если у нас есть предыдущий замер для этого PID.
                // При первом вызове _previousMeasurements пуст — все процессы получат 0.0.
                double cpuPercent = 0.0;

                if (_previousMeasurements.TryGetValue(process.Id, out var previous))
                {
                    // Сколько CPU времени процесс потратил за последний интервал.
                    var cpuDelta = currentCpuTime - previous.CpuTime;

                    // Сколько реального времени прошло между замерами.
                    var timeDelta = now - previous.MeasuredAt;

                    // CPU% = (CPU время за интервал) / (реальное время интервала)
                    // Делим на ProcessorCount потому что процесс может использовать
                    // несколько ядер одновременно. Без деления на многоядерных машинах
                    // можно получить значения > 100%.
                    if (timeDelta.TotalSeconds > 0)
                    {
                        cpuPercent = cpuDelta.TotalSeconds
                            / timeDelta.TotalSeconds
                            / Environment.ProcessorCount
                            * 100.0;
                    }
                }

                // Запоминаем текущий замер для следующего вызова.
                newMeasurements[process.Id] = (currentCpuTime, now);

                // Применяем фильтр по порогу CPU.
                // Если CpuThresholdPercent = 0.0 — добавляем все процессы.
                if (cpuPercent < CpuThresholdPercent)
                    continue;

                results.Add(new ProcessEntry
                {
                    ProcessId = process.Id,
                    Name = process.ProcessName,
                    CpuUsagePercent = Math.Round(cpuPercent, 1),
                    MemoryBytes = memoryBytes,
                    IsResponding = process.Responding
                });
            }
            catch (Exception)
            {
                // Процесс мог завершиться между GetProcesses() и чтением его свойств.
                // Также системные процессы могут выбросить UnauthorizedAccessException.
                // В обоих случаях — просто пропускаем этот процесс.
                // Не логируем каждый пропуск — это нормальная ситуация.
            }
            finally
            {
                // Process реализует IDisposable — освобождаем хендл после чтения.
                process.Dispose();
            }
        }

        // Заменяем старые замеры новыми.
        // Процессы которых больше нет в системе автоматически исчезнут
        // из словаря — их PID не попадёт в newMeasurements.
        _previousMeasurements = newMeasurements;

        // Сортируем по CPU DESC чтобы самые прожорливые процессы были вверху.
        // Это та же сортировка что в Task Manager по умолчанию.
        return results
            .OrderByDescending(p => p.CpuUsagePercent)
            .ThenByDescending(p => p.MemoryBytes)
            .ToList();
    }


    // Освобождение ресурсов

    /// <inheritdoc/>
    public void Dispose()
    {
        // Очищаем словарь предыдущих замеров.
        // Process объекты уже были освобождены в finally блоке выше.
        _previousMeasurements.Clear();
    }
}