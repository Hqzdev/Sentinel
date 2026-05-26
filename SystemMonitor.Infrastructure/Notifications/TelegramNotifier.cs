using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;
using SystemMonitor.Infrastructure.Configuration;

namespace SystemMonitor.Infrastructure.Notifications;

/// <summary>
/// Отправляет алерты в Telegram через Telegram.Bot API.
/// BotToken берётся из .env через EnvConfig (никогда не хранится в settings).
/// ChatId — из AppSettings (сохраняется после верификации кода).
/// </summary>
public sealed class TelegramNotifier : INotifier
{
    private readonly ISettingsRepository _settingsRepo;
    private readonly EnvConfig _env;
    private TelegramBotClient? _client;

    public TelegramNotifier(ISettingsRepository settingsRepo, EnvConfig env)
    {
        _settingsRepo = settingsRepo;
        _env = env;

        // Создаём клиент сразу если токен уже есть в .env
        var token = _env.BotToken;
        if (!string.IsNullOrWhiteSpace(token))
            _client = new TelegramBotClient(token);
    }

    public async Task NotifyAsync(AlertMessage message, CancellationToken cancellationToken = default)
    {
        var token = _env.BotToken;
        if (string.IsNullOrWhiteSpace(token)) return;

        var settings = await _settingsRepo.LoadAsync(cancellationToken);
        // "skipped" — пользователь пропустил Telegram при первом запуске
        if (string.IsNullOrWhiteSpace(settings.ChatId) || settings.ChatId == "skipped") return;

        // Клиент уже создан в конструкторе; токен из .env не меняется в runtime
        _client ??= new TelegramBotClient(token);

        await _client.SendMessage(
            chatId: settings.ChatId,
            text: message.Format(),
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }
}
