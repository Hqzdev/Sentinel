using CommunityToolkit.Mvvm.ComponentModel;

namespace SystemMonitor.ViewModels;

/// <summary>
/// Базовый класс для всех ViewModel в приложении.
/// ObservableObject из CommunityToolkit.Mvvm дает механизм уведомлений об изменении свойств, чтобы Binding в Avalonia мог обновлять интерфейс.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}
