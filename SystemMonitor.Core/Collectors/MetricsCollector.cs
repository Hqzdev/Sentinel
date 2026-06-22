
// Задача: Оркестратор — запускает таймер и каждые N секунд дёргает
//         все три сервиса, упаковывает результаты и пушит подписчикам.

// ViewModel не вызывает сервисы напрямую — она подписывается на
// IObservable потоки этого коллектора и пассивно получает данные.
//
// Использует System.Reactive (ReactiveX) для организации потоков данных.

using System.Reactive.Linq;
using System.Reactive.Subjects;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Collectors;


/// Центральная точка сбора метрик.
/// Запускает один таймер и раздаёт данные всем подписчикам через IObservable.

public sealed class MetricsCollector : IDisposable
{

    // Зависимости — приходят через DI, не создаются внутри
   
    private readonly ICpuMetricsService _cpuService;
    private readonly IMemoryMetricsService _memoryService;
    private readonly IProcessMetricsService _processService;


    // Subjects — внутренние источники данных
    //
    // Subject это одновременно Observable (можно подписаться)
    // и Observer (можно пушить данные).
    // Снаружи отдаём только IObservable — подписчики не могут пушить сами.



    /// Внутренний источник снапшотов CPU.
    /// Только коллектор пишет в него через OnNext().
    private readonly Subject<CpuSnapshot> _cpuSubject = new();


    /// Внутренний источник снапшотов RAM.
    private readonly Subject<RamSnapshot> _memorySubject = new();


    /// Внутренний источник списков процессов.
    private readonly Subject<IReadOnlyList<ProcessEntry>> _processSubject = new();


    // Публичные потоки — только для чтения снаружи

    /// Поток снапшотов CPU.
    /// ViewModel подписывается сюда и получает новый снапшот каждые N секунд.
    public IObservable<CpuSnapshot> CpuSnapshots => _cpuSubject.AsObservable();

    /// Поток снапшотов RAM.
    public IObservable<RamSnapshot> MemorySnapshots => _memorySubject.AsObservable();

    /// Поток списков процессов.
    public IObservable<IReadOnlyList<ProcessEntry>> ProcessSnapshots => _processSubject.AsObservable();


    // Таймер и управление жизненным циклом

    /// Токен отмены — используется чтобы остановить фоновый поток
    /// при вызове Dispose().

    private readonly CancellationTokenSource _cts = new();


    /// Интервал между опросами сервисов в миллисекундах.
    /// 1000ms = обновление раз в секунду как в Task Manager.

    private readonly int _intervalMs;


    /// Фоновый поток на котором работает таймер.
    /// Хранится чтобы дождаться его завершения в Dispose().
   
    private readonly Task _pollingTask;


    // Конструктор


    /// Принимает все три сервиса через DI.
    /// Сразу запускает фоновый поток опроса.

    /// <param name="cpuService">Сервис метрик CPU.</param>
    /// <param name="memoryService">Сервис метрик RAM.</param>
    /// <param name="processService">Сервис метрик процессов.</param>
    /// <param name="intervalMs">Интервал опроса в миллисекундах. По умолчанию 1000.</param>
    public MetricsCollector(
        ICpuMetricsService cpuService,
        IMemoryMetricsService memoryService,
        IProcessMetricsService processService,
        int intervalMs = 1000)
    {
        _cpuService = cpuService;
        _memoryService = memoryService;
        _processService = processService;
        _intervalMs = intervalMs;

        // Запускаем фоновый поток сразу при создании коллектора.
        // Task.Run переносит работу на ThreadPool — UI поток не блокируется.
        _pollingTask = Task.Run(RunPollingLoopAsync);
    }


    // Фоновый цикл опроса

    /// Основной цикл — работает на фоновом потоке всё время жизни коллектора.
    /// Каждые _intervalMs миллисекунд опрашивает все сервисы и пушит данные.

    private async Task RunPollingLoopAsync()
    {
        // Крутимся пока не придёт сигнал отмены через _cts.
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                // Опрашиваем все три сервиса последовательно.
                // Параллельно не делаем — сервисы могут не быть thread-safe
                // и выигрыш в скорости минимален (все вызовы быстрые).
                var cpuSnapshot = _cpuService.GetSnapshot();
                var memorySnapshot = _memoryService.GetSnapshot();
                var processSnapshot = _processService.GetSnapshot();

                // Пушим данные в Subject — все подписчики получат их синхронно.
                // OnNext() вызывает всех подписчиков в том же потоке.
                _cpuSubject.OnNext(cpuSnapshot);
                _memorySubject.OnNext(memorySnapshot);
                _processSubject.OnNext(processSnapshot);
            }
            catch (Exception ex)
            {
                // Если один цикл упал — логируем и продолжаем.
                // Не останавливаем весь поток из-за одной ошибки.
                // В реальном приложении здесь был бы ILogger.
                Console.WriteLine($"[MetricsCollector] Ошибка в цикле опроса: {ex.Message}");
            }

            // Ждём интервал перед следующим опросом.
            // Task.Delay не блокирует поток — он освобождает его на время ожидания.
            // CancellationToken позволяет прервать ожидание при Dispose().
            await Task.Delay(_intervalMs, _cts.Token);
        }
    }


    // Освобождение ресурсов

    public void Dispose()
    {
        // Посылаем сигнал отмены фоновому потоку.
        // Это прервёт Task.Delay и завершит цикл while на следующей итерации.
        _cts.Cancel();

        // Ждём завершения фонового потока.
        // Без этого поток может продолжать работать после Dispose().
        // Timeout 3 секунды — не хотим зависать при закрытии приложения.
        _pollingTask.Wait(TimeSpan.FromSeconds(3));

        // Завершаем все Subject — подписчики получат OnCompleted()
        // и будут знать что данных больше не будет.
        _cpuSubject.OnCompleted();
        _memorySubject.OnCompleted();
        _processSubject.OnCompleted();

        // Освобождаем Subject и CancellationTokenSource.
        _cpuSubject.Dispose();
        _memorySubject.Dispose();
        _processSubject.Dispose();
        _cts.Dispose();
    }
}