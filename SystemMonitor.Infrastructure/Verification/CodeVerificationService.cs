using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Infrastructure.Configuration;

namespace SystemMonitor.Infrastructure.Verification;

/// <summary>
/// Верификация через Telegram getUpdates:
/// 1. App показывает 8-символьный код пользователю
/// 2. Пользователь отправляет код нашему боту в Telegram
/// 3. Нажимает «Проверить» — app ищет код в последних обновлениях бота
/// 4. Если нашли — сохраняем chat_id и считаем авторизацию пройденной
/// </summary>
public sealed class CodeVerificationService : ICodeVerificationService, IDisposable
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private readonly EnvConfig _env;

    // Хранит offset для getUpdates чтобы не читать старые сообщения
    private int _lastUpdateId;

    public CodeVerificationService(EnvConfig env)
    {
        _env = env;
    }

    // ── ICodeVerificationService ───────────────────────────────────────────

    public string GenerateCode()
    {
        // 8 символов: заглавные буквы + цифры, без похожих I/1/O/0
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rng = new Random();
        return new string(Enumerable.Range(0, 8).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
    }

    public async Task<long?> CheckForCodeAsync(string code, CancellationToken ct = default)
    {
        var token = _env.BotToken;
        if (string.IsNullOrWhiteSpace(token)) return null;

        try
        {
            var url = $"https://api.telegram.org/bot{token}/getUpdates?limit=20&offset={_lastUpdateId}";
            var response = await Http.GetFromJsonAsync<TelegramUpdatesResponse>(url, ct);

            if (response?.Result is null) return null;

            foreach (var update in response.Result)
            {
                // Обновляем offset чтобы не получать одни и те же сообщения
                if (update.UpdateId >= _lastUpdateId)
                    _lastUpdateId = update.UpdateId + 1;

                var text = update.Message?.Text?.Trim() ?? "";

                // Ищем точное совпадение (без учёта регистра)
                if (string.Equals(text, code, StringComparison.OrdinalIgnoreCase))
                    return update.Message?.Chat.Id;

                // Также принимаем /start <код>
                if (text.StartsWith("/start ", StringComparison.OrdinalIgnoreCase) &&
                    text[7..].Trim().Equals(code, StringComparison.OrdinalIgnoreCase))
                    return update.Message?.Chat.Id;
            }

            return null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return null;
        }
    }

    public void Dispose() => Http.Dispose();

    // ── Telegram response models ───────────────────────────────────────────

    private sealed class TelegramUpdatesResponse
    {
        [JsonPropertyName("ok")]    public bool Ok     { get; set; }
        [JsonPropertyName("result")] public List<Update>? Result { get; set; }
    }

    private sealed class Update
    {
        [JsonPropertyName("update_id")] public int UpdateId { get; set; }
        [JsonPropertyName("message")]   public Message? Message { get; set; }
    }

    private sealed class Message
    {
        [JsonPropertyName("text")] public string? Text { get; set; }
        [JsonPropertyName("chat")] public Chat Chat { get; set; } = new();
    }

    private sealed class Chat
    {
        [JsonPropertyName("id")] public long Id { get; set; }
    }
}
