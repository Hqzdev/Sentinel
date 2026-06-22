using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SystemMonitor.ViewModels;

namespace SystemMonitor;

/// <summary>
/// Автоматически подбирает View для переданной ViewModel.
/// Используется Avalonia DataTemplates: если экран получает объект ViewModelBase, локатор пытается найти класс View с похожим именем.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Создает визуальный Control для переданной ViewModel.
    /// Имя типа получается заменой суффикса ViewModel на View, поэтому MainWindowViewModel теоретически ищет MainWindowView.
    /// Если подходящий класс не найден, возвращается TextBlock с сообщением об ошибке, чтобы проблему было видно в интерфейсе.
    /// </summary>
    public Control? Build(object? param)
    {
        if (param is null)
            return null;
        
        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <summary>
    /// Проверяет, подходит ли объект под этот шаблон.
    /// Локатор работает только с объектами, которые наследуются от ViewModelBase, то есть относятся к MVVM-слою приложения.
    /// </summary>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
