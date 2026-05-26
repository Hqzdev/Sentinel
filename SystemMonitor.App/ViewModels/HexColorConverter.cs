using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace SystemMonitor.App.ViewModels;

/// <summary>
/// Конвертирует hex-строку ("#3b82f6") в Color для биндинга в AXAML.
/// Используется как StaticResource.
/// </summary>
public sealed class HexColorConverter : IValueConverter
{
    public static readonly HexColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hex && Color.TryParse(hex, out var color))
            return color;
        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
