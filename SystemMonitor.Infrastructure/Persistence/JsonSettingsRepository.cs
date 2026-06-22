using System.Text.Json;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Infrastructure.Persistence;

/// <summary>
/// Реализация ISettingsRepository, которая хранит настройки в JSON-файле.
/// Путь строится от системной папки ApplicationData, поэтому настройки лежат вне исходного кода и сохраняются между запусками приложения.
/// SemaphoreSlim защищает файл от одновременного чтения и записи из разных асинхронных операций.
/// </summary>
public sealed class JsonSettingsRepository : ISettingsRepository
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Sentinel",
        "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Загружает AppSettings из JSON-файла.
    /// Если файла еще нет, возвращаются настройки по умолчанию, чтобы первый запуск приложения не требовал ручной подготовки конфигурации.
    /// Если JSON прочитан, он десериализуется в AppSettings с учетом регистра свойств.
    /// </summary>
    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(SettingsPath))
            return AppSettings.Default;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            await using var stream = File.OpenRead(SettingsPath);
            return await JsonSerializer.DeserializeAsync<AppSettings>(stream, JsonOptions, cancellationToken)
                   ?? AppSettings.Default;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Сохраняет AppSettings в JSON-файл.
    /// Перед записью создается папка Sentinel в ApplicationData, затем настройки сериализуются с отступами для удобного чтения человеком.
    /// </summary>
    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            await using var stream = File.Create(SettingsPath);
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }
}
