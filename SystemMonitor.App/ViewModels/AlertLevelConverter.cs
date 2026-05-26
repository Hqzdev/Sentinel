using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace SystemMonitor.App.ViewModels;

/// <summary>
/// Конвертирует строку уровня алерта ("warning", "info", "error") в цвет точки.
/// Используется как StaticResource в AXAML.
/// </summary>
public sealed class AlertLevelConverter : IValueConverter
{
    public static readonly AlertLevelConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value as string) switch
        {
            "warning" => new SolidColorBrush(Color.FromRgb(251, 191, 36)),
            "error"   => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
            "info"    => new SolidColorBrush(Color.FromRgb(100, 116, 139)),
            _         => new SolidColorBrush(Color.FromRgb(100, 116, 139)),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
