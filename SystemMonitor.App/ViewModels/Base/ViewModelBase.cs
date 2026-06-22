using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SystemMonitor.App.ViewModels.Base;

// базовый класс для всех viewmodel
// нужен чтобы не писать PropertyChanged в каждом файле заново
// через него ui узнает что данные изменились и надо обновить экран
public abstract class ViewModelBase : INotifyPropertyChanged
{
    // событие из интерфейса INotifyPropertyChanged
    // Avalonia подписывается на него когда делает binding к свойствам
    public event PropertyChangedEventHandler? PropertyChanged;

    // Общий метод для обновления свойств ViewModel.
    // Если значение не поменялось то UI не перерисовываем лишний раз.
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        // сначала меняем поле внутри viewmodel
        // потом отправляем событие чтобы binding обновил значение на экране
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // Сообщает Avalonia что значение свойства изменилось.
    // После этого Binding сам обновляет нужный элемент интерфейса.
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
