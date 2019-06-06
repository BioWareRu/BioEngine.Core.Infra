# Модуль логгирования

Модуль реализует подключение библиотеки Serilog, её провайдеров и конфигурирование уровня логгирования в рантайме

## Установка

Подключить один из модулей-реализаций (Serilog, Loki)

```csharp
bioengine.AddModule<LokiLoggingModule, LokiLoggingConfig>((configuration, environment) =>
                    new LokiLoggingConfig(configuration["LOKI_URL"]))
``` 

## Использование

Модуль просто начнёт работать

## Смена уровня логгирования в рантайме

Модуль добавляет MVC-контроллер  `LogsController` позволящий GET-запросом изменить текущий уровень логов. Контроллер требует авторизации с политикой `logs`.

```
GET /logs/info
```

```
GET /logs/debug
```
