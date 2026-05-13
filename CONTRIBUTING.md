# Contribution Guide

Документ описывает правила разработки, кодстайл и процесс внесения изменений в проект Sentinel.

## Общие правила

- Пишите код так, чтобы ответственность класса была понятна из его названия.
- Не смешивайте UI, бизнес-логику, работу с базой данных и внешние API в одном классе.
- Новую функциональность размещайте в соответствующем слое архитектуры.
- Секреты, токены Telegram, локальные базы данных и пользовательские настройки не добавляйте в Git.
- Перед отправкой изменений убедитесь, что проект собирается без ошибок.

## Ветки и коммиты

Для задач используйте отдельные ветки:

```text
feature/system-monitoring
feature/telegram-bot
fix/sqlite-logging
docs/update-architecture
```

Сообщения коммитов пишите кратко и по смыслу:

```text
Add CPU monitoring service
Fix Telegram notification threshold
Update architecture documentation
```

## Кодстайл C#

### Именование

- Классы, интерфейсы, методы и свойства: `PascalCase`.
- Локальные переменные и параметры методов: `camelCase`.
- Приватные поля: `_camelCase`.
- Интерфейсы начинаются с `I`: `ISystemMonitorService`.
- Асинхронные методы заканчиваются на `Async`: `SendNotificationAsync`.

Пример:

```csharp
public interface ISystemMonitorService
{
    Task<SystemSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}

public sealed class SystemMonitorService : ISystemMonitorService
{
    private readonly IMetricRepository _metricRepository;

    public SystemMonitorService(IMetricRepository metricRepository)
    {
        _metricRepository = metricRepository;
    }
}
```

### Форматирование

- Используйте 4 пробела для отступов.
- Одна публичная сущность на файл.
- Название файла должно совпадать с названием класса.
- Используйте фигурные скобки даже для однострочных `if`, `for`, `while`.
- Сортируйте `using` и удаляйте неиспользуемые директивы.

### Nullable Reference Types

Рекомендуется включить nullable reference types:

```xml
<Nullable>enable</Nullable>
```

Если значение может отсутствовать, это должно быть явно видно в типе:

```csharp
public string? ErrorMessage { get; init; }
```

### Асинхронность

- Для операций с базой данных, файлами, Telegram API и долгими системными операциями используйте `async/await`.
- Передавайте `CancellationToken` в сервисы, которые работают в фоне.
- Не используйте `.Result` и `.Wait()` в асинхронном коде.

### Обработка ошибок

- Не подавляйте исключения без логирования.
- Для ожидаемых ошибок используйте понятные сообщения в журнале событий.
- Ошибки внешних API и базы данных должны обрабатываться на уровне сервисов или application layer.

## Структура кода

Рекомендуемое распределение:

```text
src/
├── Sentinel.Presentation/
│   ├── Views/
│   ├── ViewModels/
│   └── Resources/
├── Sentinel.Application/
│   ├── UseCases/
│   ├── Abstractions/
│   └── Options/
├── Sentinel.Services/
│   ├── Monitoring/
│   ├── Notifications/
│   └── Telegram/
├── Sentinel.Infrastructure/
│   ├── Persistence/
│   ├── SystemMetrics/
│   └── External/
└── Sentinel.Domain/
    ├── Entities/
    ├── Enums/
    └── ValueObjects/
```

## Правила для WPF

- XAML должен отвечать за разметку и визуальное представление.
- Логика поведения должна находиться во ViewModel, а не в code-behind.
- Используйте привязки данных и команды вместо прямого обращения к элементам UI.
- ViewModel не должна напрямую работать с SQLite, Telegram API или `System.Diagnostics`.

## Тестирование

Минимально тестируйте:

- расчет уровней предупреждений;
- проверку порогов CPU, RAM и диска;
- форматирование уведомлений;
- application-сценарии без привязки к WPF.

Перед отправкой изменений выполните:

```bash
dotnet build
dotnet test
```

## Документация

При добавлении значимой функциональности обновляйте:

- `README.md`, если меняется способ запуска или стек;
- `docs/architecture/architecture.md`, если меняется архитектура;
- комментарии в коде, если логика неочевидна.

