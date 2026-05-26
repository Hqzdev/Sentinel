using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.App.ViewModels;

public sealed partial class LoginViewModel : ViewModelBase
{
    private readonly ICodeVerificationService _verifier;
    private readonly ISettingsRepository _settingsRepo;

    // ── Состояние формы ────────────────────────────────────────────────────
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _pairingCode = string.Empty;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _isVerifying;
    [ObservableProperty] private bool _isSuccess;

    // ── События для code-behind ────────────────────────────────────────────
    public event Action? LoginSucceeded;
    public event Action? ShakeRequested;

    public LoginViewModel(ICodeVerificationService verifier, ISettingsRepository settingsRepo)
    {
        _verifier = verifier;
        _settingsRepo = settingsRepo;
        PairingCode = _verifier.GenerateCode();
    }

    // ── Форматированный код (XXXX XXXX для читаемости) ─────────────────────
    public string FormattedCode => PairingCode.Length == 8
        ? $"{PairingCode[..4]} {PairingCode[4..]}"
        : PairingCode;

    // ── Команда: проверить код ─────────────────────────────────────────────
    [RelayCommand]
    private async Task VerifyAsync()
    {
        if (string.IsNullOrWhiteSpace(UserName))
        {
            ShowError("Введите ваше имя");
            ShakeRequested?.Invoke();
            return;
        }

        IsVerifying = true;
        HasError = false;
        StatusText = "Проверяем...";

        try
        {
            var chatId = await _verifier.CheckForCodeAsync(PairingCode);

            if (chatId is null)
            {
                ShowError("Код не найден. Отправьте его боту и попробуйте снова.");
                ShakeRequested?.Invoke();
                return;
            }

            // Успех — сохраняем настройки
            IsSuccess = true;
            StatusText = "Подключено ✓";

            var settings = await _settingsRepo.LoadAsync();
            var updated = settings with { ChatId = chatId.Value.ToString() };
            await _settingsRepo.SaveAsync(updated);

            await Task.Delay(700); // показываем успех перед переходом
            LoginSucceeded?.Invoke();
        }
        finally
        {
            IsVerifying = false;
        }
    }

    // ── Команда: пропустить ────────────────────────────────────────────────
    [RelayCommand]
    private async Task SkipAsync()
    {
        // Сохраняем маркер "пропущено" чтобы при следующем запуске не показывать снова
        var settings = AppSettings.Default with { ChatId = "skipped" };
        await _settingsRepo.SaveAsync(settings);
        LoginSucceeded?.Invoke();
    }

    // ── Команда: сгенерировать новый код ──────────────────────────────────
    [RelayCommand]
    private void RegenerateCode()
    {
        PairingCode = _verifier.GenerateCode();
        OnPropertyChanged(nameof(FormattedCode));
        HasError = false;
        StatusText = string.Empty;
    }

    private void ShowError(string message)
    {
        StatusText = message;
        HasError = true;
        IsSuccess = false;
    }
}
