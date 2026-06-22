// сбор метрик каждого процесса
// сложнее так как возвращает не один обьект а коллекциб

namespace SystemMonitor.Core.Interfaces;

public interface IProcessMetricsService: IDisposable
{
   // вызывает список всех запущенных процессов с их метриками
   // но есть нюанс при первом запуске cpu будет 0 но при след значение появится
   IreadOnlyList<ProcessEntry> GetSnapshot();
   double CpuThresholdPercent { get; set;}
}