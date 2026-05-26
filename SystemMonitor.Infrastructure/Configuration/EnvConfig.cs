namespace SystemMonitor.Infrastructure.Configuration;

/// <summary>
/// Читает .env файл из папки рядом с exe и предоставляет переменные приложению.
/// Значения из настоящих переменных окружения имеют приоритет над .env.
/// </summary>
public sealed class EnvConfig
{
    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

    public EnvConfig()
    {
        LoadEnvFile();
    }

    /// <summary>Токен Telegram-бота приложения из SENTINEL_BOT_TOKEN.</summary>
    public string BotToken => Get("SENTINEL_BOT_TOKEN");

    public string Get(string key)
    {
        // Приоритет: настоящие env-переменные → .env файл
        var envVal = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(envVal)) return envVal;

        return _values.TryGetValue(key, out var val) ? val : string.Empty;
    }

    // ── Private ────────────────────────────────────────────────────────────

    private void LoadEnvFile()
    {
        // Ищем .env рядом с exe, или в текущей директории
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
        };

        foreach (var path in candidates)
        {
            if (!File.Exists(path)) continue;

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.StartsWith('#') || line.Length == 0) continue;

                var idx = line.IndexOf('=');
                if (idx < 1) continue;

                var key = line[..idx].Trim();
                var value = line[(idx + 1)..].Trim().Trim('"').Trim('\'');
                _values[key] = value;
            }
            break;
        }
    }
}
