// Реализация IMemoryMetricsService через Windows API
// Используем GlobalMemoryStatusEx из kernel32.dll напрямую. не испольум perofmacne counter потому что у нас быстрее и не надо админ права


using System.Runtime.InteropServices;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Services;

// читаемт метрики памяти через вин апи и не держит состояния
public sealed class MemoryMetricsService : IMemoryMetricsService
{
    
    /// Структура которую заполняет GlobalMemoryStatusEx.
    /// Должна точно соответствовать нативной MEMORYSTATUSEX структуре.
    /// StructLayout гарантирует что поля идут в памяти в том же порядке
    /// что ожидает Windows.

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        // размер в байтах так как требует винда
        // uint надо потму что трубет виндовс так как структура должжна быть байт в байт 
        public uint DwLength;

   
        /// Текущая загрузка памяти в процентах (0-100).
        /// Не используем — считаем сами из TotalPhys и AvailPhys для точности.
        public uint DwMemoryLoad;

 
        /// Общий объём физической RAM в байтах.
        public ulong UllTotalPhys;

        /// Доступная физическая RAM в байтах.
        public ulong UllAvailPhys;

        // Остальные поля структуры (своп, виртуальная память) —
        // объявляем чтобы структура имела правильный размер,
        // но не используем в нашей логике.
        public ulong UllTotalPageFile;
        public ulong UllAvailPageFile;
        public ulong UllTotalVirtual;
        public ulong UllAvailVirtual;
        public ulong UllAvailExtendedVirtual;
    }

    /// Импорт нативной функции из kernel32.dll.
    /// Заполняет переданную структуру MemoryStatusEx актуальными данными.
    /// Возвращает true при успехе, false при ошибке.
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

    // тут начинается интерфейс
    public RamSnapshot GetSnapshot()
    {
        // создаем пустую строку в памяти
        var memStatus = new MemoryStatusEx();

        // Windows требует чтобы DwLength был заполнен до вызова —
        // иначе функция вернёт false и данные будут плохими.
        // и переводим в uint так как винда требует
        memStatus.DwLength = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));
        // тут защита стоит винда записывает данные в структуру и если все плохо то возвращает 0 вместо падения ui 
        if (!GlobalMemoryStatusEx(ref memStatus))
        {
            // Если WinAPI вернул ошибку — возвращаем пустой снапшот.
            // Не бросаем исключение: сервис не должен ронять приложение
            // из-за временной ошибки чтения метрик.
            
            return new RamSnapshot
            {
                Timestamp = DateTime.UtcNow,
                TotalBytes = 0,
                UsedBytes = 0
            };
        }
        // тут берем данные из структуры которую заполнил виндовс
        // делаем конверт из ulong в long
        var totalBytes = (long)memStatus.UllTotalPhys;
        var availableBytes = (long)memStatus.UllAvailPhys;
        // создаем снапшот с реальными данными 
        return new RamSnapshot
        {
            Timestamp = DateTime.UtcNow,
            TotalBytes = totalBytes,

            // UsedBytes = Total - Available
            // Windows не отдаёт Used напрямую — только Total и Available.
            UsedBytes = totalBytes - availableBytes
        };
    }
}
