using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace SystemMonitor.App.ViewModels.Converters;

// конвертер нужен для xaml привязок
// xaml передает сюда число процента а обратно получает кисть с цветом
// так можно менять цвет текста без логики прямо в разметке
public sealed class PercentToColorConverter : IValueConverter
{
    // Цвет зависит от процента нагрузки.
    // Зелёный нормальная зона, жёлтый предупреждение, красный высокая нагрузка.
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // если в binding пришло не число то возвращаем нейтральный серый цвет
        // это защита чтобы интерфейс не падал из-за неправильного значения
        if (value is not double percent)
        {
            return Brushes.Gray;
        }

        // 80 и выше считаем сильной нагрузкой
        if (percent >= 80)
        {
            return Brushes.Firebrick;
        }

        // 55 и выше считаем средней нагрузкой
        if (percent >= 55)
        {
            return Brushes.DarkGoldenrod;
        }

        // всё что ниже это нормальная зона
        return Brushes.SeaGreen;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // обратное преобразование здесь не нужно
        // пользователь не вводит цвет руками чтобы получить процент
        throw new NotSupportedException();
    }
}
