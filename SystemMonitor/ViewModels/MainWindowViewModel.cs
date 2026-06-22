using System.Collections.Generic;

namespace SystemMonitor.ViewModels;

/// <summary>
/// ViewModel главного окна Sentinel.
/// В MVVM этот класс является посредником между XAML-интерфейсом и данными: MainWindow.axaml берет отсюда заголовки, проценты, списки процессов и журнал событий через Binding.
/// Текущая версия хранит демонстрационные значения, поэтому экран показывает заранее подготовленное состояние системы, а не результат живого MonitoringService.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Название приложения, которое выводится в верхней части главного окна.
    /// Значение берется прямо из ViewModel, а XAML получает его через Binding AppTitle.
    /// </summary>
    public string AppTitle { get; } = "Sentinel";

    /// <summary>
    /// Текст состояния приложения в зеленом индикаторе.
    /// Сейчас это демонстрационная подпись, показывающая, что интерфейс работает в тестовом режиме.
    /// </summary>
    public string StatusText { get; } = "Тестовый режим";

    /// <summary>
    /// Демонстрационная загрузка процессора в процентах.
    /// Это число используется ProgressBar и текстовым представлением CpuUsageText.
    /// </summary>
    public int CpuUsage { get; } = 68;

    /// <summary>
    /// Демонстрационное использование оперативной памяти в процентах.
    /// В реальной интеграции это значение должно приходить из MetricsSnapshot.RamPct.
    /// </summary>
    public int RamUsage { get; } = 74;

    /// <summary>
    /// Демонстрационная нагрузка или заполненность диска в процентах.
    /// В текущем UI используется для карточки Disk и ProgressBar.
    /// </summary>
    public int DiskUsage { get; } = 82;

    /// <summary>
    /// Текст времени работы системы или приложения.
    /// Сейчас значение задано вручную; при подключении живого мониторинга его можно формировать из MetricsSnapshot.Uptime.
    /// </summary>
    public string Uptime { get; } = "12 ч 34 мин";

    /// <summary>
    /// Форматирует числовую загрузку CPU в строку с символом процента.
    /// XAML использует это свойство для крупного текста в карточке CPU.
    /// </summary>
    public string CpuUsageText => $"{CpuUsage}%";

    /// <summary>
    /// Форматирует числовую загрузку RAM в строку с символом процента.
    /// Это отделяет представление текста от базового числового значения RamUsage.
    /// </summary>
    public string RamUsageText => $"{RamUsage}%";

    /// <summary>
    /// Форматирует значение диска в строку с символом процента.
    /// Так интерфейс может одновременно использовать DiskUsage как число и DiskUsageText как готовую подпись.
    /// </summary>
    public string DiskUsageText => $"{DiskUsage}%";

    /// <summary>
    /// Подпись статуса мониторинга под карточкой Uptime.
    /// Сейчас текст статический и нужен для демонстрации состояния интерфейса.
    /// </summary>
    public string MonitorStatus { get; } = "Мониторинг активен";

    /// <summary>
    /// Текст последнего обновления данных.
    /// В текущем варианте дата задана вручную; при живом мониторинге должна строиться из MetricsSnapshot.Timestamp.
    /// </summary>
    public string LastUpdate { get; } = "Последнее обновление: 13.05.2026 20:30";

    /// <summary>
    /// Точки демонстрационного графика CPU.
    /// ItemsControl в MainWindow.axaml проходит по этому списку и рисует столбцы нагрузки по времени.
    /// </summary>
    public IReadOnlyList<ChartPoint> CpuChartPoints { get; } =
    [
        new("20:00", 32),
        new("20:05", 41),
        new("20:10", 48),
        new("20:15", 44),
        new("20:20", 57),
        new("20:25", 63),
        new("20:30", 68)
    ];

    /// <summary>
    /// Демонстрационный список самых нагруженных процессов.
    /// В реальной версии аналогичные данные должен отдавать IProcessMetricsProvider как список ProcessSnapshot.
    /// </summary>
    public IReadOnlyList<ProcessMetric> TopProcesses { get; } =
    [
        new("Visual Studio", "18%", "1.2 GB"),
        new("Chrome", "14%", "2.1 GB"),
        new("Finder", "5%", "320 MB"),
        new("System", "3%", "180 MB")
    ];

    /// <summary>
    /// Демонстрационный журнал событий и предупреждений.
    /// Сейчас записи задаются вручную, чтобы показать, как в интерфейсе выглядит история предупреждений.
    /// </summary>
    public IReadOnlyList<AlertLogEntry> AlertLog { get; } =
    [
        new("20:25", "CPU", "Загрузка процессора выше 65%", "Warning"),
        new("20:18", "RAM", "Использование памяти выше 70%", "Warning"),
        new("20:05", "Disk", "Диск заполнен на 82%", "Info")
    ];
}

/// <summary>
/// Одна строка таблицы процессов в главном окне.
/// Name показывает имя процесса, Cpu — его загрузку процессора, Memory — используемую память в удобном для UI текстовом виде.
/// </summary>
public sealed record ProcessMetric(string Name, string Cpu, string Memory);

/// <summary>
/// Одна запись журнала событий.
/// Time хранит время, Type показывает категорию события, Message содержит текст предупреждения, Level может использоваться для визуального уровня важности.
/// </summary>
public sealed record AlertLogEntry(string Time, string Type, string Message, string Level);

/// <summary>
/// Одна точка графика нагрузки.
/// Time используется как подпись по оси времени, Value задает высоту столбца и процент нагрузки CPU.
/// </summary>
public sealed record ChartPoint(string Time, int Value);
