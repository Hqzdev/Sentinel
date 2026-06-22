using SystemMonitor.Core.Models;

namespace SystemMonitor.Core.Interfaces;

/// <summary>
/// Интерфейс хранилища настроек приложения.
/// Core знает, что настройки можно загрузить и сохранить, но не привязан к конкретному формату хранения: JSON, база данных или реестр могут быть заменены реализацией в Infrastructure.
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Загружает настройки приложения.
    /// Если физическое хранилище отсутствует, реализация может вернуть AppSettings.Default, чтобы приложение продолжило работу с безопасными значениями.
    /// </summary>
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет настройки приложения.
    /// Метод принимает готовый объект AppSettings из ViewModel или сервиса и передает его конкретному хранилищу.
    /// </summary>
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
