// сбор метрик памяти

namespace SystemMonitor.Core.Interfaces;

public interface IMemoryMetricsService
{
    // метод который возвращает снимок оперативной памяти в конкретный момент времени
    // он асинхронный так как может требовать время для получения данных от системы и мы не хотим блокировать UI или другие операции пока ждем эти данные
    RamSnapshot GetSnapshot();
}

