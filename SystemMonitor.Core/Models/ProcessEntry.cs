// файл нужен для хранения записи о конкретном процессе 
// коллектор будет выозвращать ireadonly всю историю записей

namespace SystemMonitor.Core.Models

public sealed record ProcessEntry
{
    // создаем уникальный токен процесса  но он может быть переиспользован после заврешения процесса
    public int ProcessId { get; init;}
    // имя процесса без exe
    public string Name { get; init;} = string.Empty;
    // загрузка процессора этим процессом
    // double доя точности так как дробное число может быть
    public double CpuUsagePercent { get; init;}
    // потребление памятью этим процессом
    // берем long потому что может не влезть в int для больших процессов
    public long MemoryBytes { get; init;}
    // чем процесс занят
    // отвечает или завис
    // false - процесс завис 
    // используем в viewmodel для визуальной индикации еще
    public bool IsResponding { get; init;}
}