namespace SystemMonitor.Core.Models;

/// <summary>
/// Сообщение об алерте, которое передаётся в INotifier.
/// </summary>
public sealed record AlertMessage(
    string Metric,
    float Value,
    float Threshold,
    DateTime Time)
{
    public string Format() =>
        $"⚠️ <b>{Metric}</b>\n" +
        $"Текущее значение: <b>{Value:F1}%</b>\n" +
        $"Порог: {Threshold}%\n" +
        $"Время: {Time:HH:mm:ss}";
}
