using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Interfaces;

/// <summary>
/// Абстракция отправки уведомлений.
/// Реализуется в Infrastructure (Telegram, Discord и т.д.).
/// </summary>
public interface INotifier
{
    Task NotifyAsync(AlertMessage message, CancellationToken cancellationToken = default);
}
