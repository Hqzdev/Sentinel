# Архитектура проекта Sentinel

## Цель документа

Документ описывает архитектуру desktop-приложения Sentinel: основные слои, зоны ответственности, поток данных, выбранный стек и причины использования этих технологий.

## Архитектурный стиль

В проекте используется слоистая монолитная архитектура.

```text
Presentation Layer (WPF UI)
        ↓
Application Layer
        ↓
Services Layer
        ↓
Infrastructure Layer
        ↓
SQLite Database
```

Такой подход выбран потому, что проект является desktop-приложением, запускается локально на одном компьютере и не требует микросервисной архитектуры. Слоистый монолит проще разрабатывать, тестировать и сопровождать, при этом он сохраняет четкое разделение ответственности.

## Почему используется Layered Architecture

Слоистая архитектура подходит для Sentinel по нескольким причинам:

- UI не зависит от конкретной реализации Telegram API или SQLite.
- Бизнес-логика отделена от графического интерфейса.
- Сервисы мониторинга можно тестировать отдельно от WPF.
- Инфраструктурный код можно заменить без полной переработки приложения.
- Проект проще расширять: можно добавить веб-панель, экспорт отчетов или удаленный мониторинг.

## Слои приложения

### Presentation Layer

Слой пользовательского интерфейса.

Основные задачи:

- отображение текущих метрик CPU, RAM, диска и uptime;
- отображение графиков нагрузки;
- отображение журнала событий;
- работа с действиями пользователя;
- привязка данных через ViewModel.

Технологии:

- WPF;
- XAML;
- MVVM-подход.

Presentation Layer не должен напрямую обращаться к SQLite, Telegram API или `System.Diagnostics`. Для этого используются application-сценарии и сервисы.

### Application Layer

Слой сценариев приложения и бизнес-логики.

Основные задачи:

- координация мониторинга системы;
- обработка полученных метрик;
- проверка пороговых значений;
- управление состоянием приложения;
- подготовка данных для UI;
- вызов сервисов уведомлений и сохранения истории.

Примеры компонентов:

- `StartMonitoringUseCase`;
- `GetCurrentSystemStatusUseCase`;
- `CheckMetricThresholdsUseCase`;
- `CreateSystemReportUseCase`.

Application Layer должен зависеть от абстракций, а не от конкретных инфраструктурных классов.

### Services Layer

Слой прикладных сервисов.

Основные задачи:

- периодический мониторинг системы;
- формирование уведомлений;
- работа с Telegram-ботом;
- управление настройками;
- логирование событий;
- подготовка отчетов.

Примеры сервисов:

- `SystemMonitorService`;
- `TelegramNotificationService`;
- `AlertService`;
- `MetricHistoryService`;
- `ReportService`.

### Infrastructure Layer

Слой инфраструктуры и внешних зависимостей.

Основные задачи:

- получение системных метрик через `System.Diagnostics`;
- работа с SQLite;
- выполнение запросов к Telegram Bot API;
- работа с файловой системой;
- сохранение локальных настроек.

Примеры компонентов:

- `WindowsSystemMetricsProvider`;
- `SqliteMetricRepository`;
- `SqliteAlertRepository`;
- `TelegramBotClientAdapter`;
- `AppSettingsProvider`.

Infrastructure Layer содержит технические детали. Остальные слои должны обращаться к нему через интерфейсы.

### Domain Layer

Domain Layer содержит общие модели предметной области.

Примеры сущностей:

- `SystemMetric`;
- `SystemSnapshot`;
- `Alert`;
- `MetricType`;
- `WarningLevel`;
- `ProcessInfo`;
- `MonitoringSettings`.

Domain Layer не должен зависеть от WPF, SQLite, Telegram API или других внешних библиотек.

## Поток данных

1. Infrastructure Layer получает текущие системные показатели.
2. Services Layer формирует снимок состояния системы.
3. Application Layer проверяет метрики на превышение порогов.
4. При необходимости создается предупреждение.
5. Services Layer отправляет уведомление через Telegram.
6. Infrastructure Layer сохраняет метрики и предупреждения в SQLite.
7. Presentation Layer получает обновленные данные и отображает их пользователю.

## Основные модели данных

### SystemMetric

Описывает одну метрику системы.

```text
Id
MetricType
Value
CreatedAt
WarningLevel
```

### SystemSnapshot

Описывает текущее состояние компьютера.

```text
CpuUsagePercent
RamUsagePercent
DiskUsagePercent
Uptime
TopProcesses
CreatedAt
```

### Alert

Описывает предупреждение, созданное при превышении порога.

```text
Id
MetricType
Message
Value
Threshold
WarningLevel
CreatedAt
IsSent
```

### MonitoringSettings

Описывает пользовательские настройки мониторинга.

```text
CpuWarningPercent
RamWarningPercent
DiskWarningPercent
PollingIntervalSeconds
TelegramBotToken
TelegramChatId
```

## База данных

Для хранения данных используется SQLite.

Причины выбора SQLite:

- не требует отдельного сервера;
- подходит для локального desktop-приложения;
- проста в установке и переносе;
- достаточно производительна для хранения истории метрик;
- хорошо интегрируется с .NET.

Рекомендуемые таблицы:

```text
Metrics
Alerts
ApplicationLogs
Settings
```

### Metrics

Хранит историю системных показателей.

```text
Id INTEGER PRIMARY KEY
MetricType TEXT NOT NULL
Value REAL NOT NULL
WarningLevel TEXT NOT NULL
CreatedAt TEXT NOT NULL
```

### Alerts

Хранит предупреждения.

```text
Id INTEGER PRIMARY KEY
MetricType TEXT NOT NULL
Message TEXT NOT NULL
Value REAL NOT NULL
Threshold REAL NOT NULL
WarningLevel TEXT NOT NULL
IsSent INTEGER NOT NULL
CreatedAt TEXT NOT NULL
```

### Settings

Хранит локальные настройки приложения.

```text
Key TEXT PRIMARY KEY
Value TEXT NOT NULL
```

## Telegram-интеграция

Telegram-бот используется для уведомлений и получения текущего статуса компьютера.

Основные сценарии:

- отправка предупреждения при высокой нагрузке;
- отправка краткого отчета;
- обработка команды `/status`;
- обработка команды `/report`;
- обработка команды `/help`.

Telegram-токен и chat id считаются секретными данными и не должны храниться в репозитории.

## Причины выбора технологий

### C# и .NET 8

C# и .NET 8 подходят для Windows desktop-разработки, имеют хорошую поддержку асинхронности, работы с файлами, базами данных и системными API.

### WPF

WPF выбран для создания desktop-интерфейса под Windows. Он поддерживает XAML, привязки данных и MVVM, что удобно для отображения метрик, графиков и журналов.

### SQLite

SQLite используется как локальная встроенная база данных. Для проекта не требуется отдельный сервер БД, поэтому SQLite снижает сложность развертывания.

### Telegram.Bot API

Telegram.Bot API позволяет отправлять уведомления пользователю и обрабатывать команды бота без разработки отдельного мобильного приложения.

### System.Diagnostics

`System.Diagnostics` используется для получения информации о процессах и части системных показателей. Это стандартный инструмент .NET, который не требует сторонних зависимостей для базового мониторинга.

### Графики

На текущем этапе тестовый график построен стандартными элементами Avalonia без отдельной библиотеки. Это снижает риск конфликтов зависимостей на раннем этапе. Позже для полноценной визуализации истории CPU, RAM и диска можно подключить совместимую версию LiveCharts или другую библиотеку графиков.

## Правила зависимостей

- Presentation может зависеть от Application.
- Application может зависеть от Domain и абстракций сервисов.
- Services могут зависеть от Domain и интерфейсов инфраструктуры.
- Infrastructure реализует интерфейсы и содержит технические детали.
- Domain не зависит от других слоев.

Запрещено:

- вызывать SQLite напрямую из ViewModel;
- отправлять Telegram-сообщения из code-behind;
- размещать бизнес-логику проверки порогов в XAML или окнах;
- хранить токены Telegram в исходном коде.

## Расширение проекта

Архитектура позволяет добавить:

- мониторинг нескольких устройств;
- веб-панель;
- экспорт отчетов в CSV или PDF;
- авторизацию;
- облачную синхронизацию;
- дополнительные типы уведомлений.
