namespace SystemMonitor.Core.Interfaces;

/// <summary>
/// Сервис верификации через Telegram: генерирует код и проверяет,
/// отправил ли пользователь его нашему боту.
/// </summary>
public interface ICodeVerificationService
{
    /// <summary>Генерирует новый 8-символьный код для текущей сессии.</summary>
    string GenerateCode();

    /// <summary>
    /// Проверяет последние обновления бота — ищет сообщение с указанным кодом.
    /// Возвращает chat_id пользователя если нашёл, иначе null.
    /// </summary>
    Task<long?> CheckForCodeAsync(string code, CancellationToken ct = default);
}
