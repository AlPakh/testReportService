Сервис принимает запросы на построение отчётов по конверсии просмотров в оплаты через шину сообщений, обрабатывает их пакетно и отдаёт результат по http api

## Стек

- C# .NET 8
- ASP.NET Core Web API
- PostgreSQL 16
- RabbitMQ 3
- Entity Framework Core
- Docker
- MSTest

## Что реализовано

- Приём запросов на отчёт через RabbitMQ
- Асинхронная обработка запросов
- Пакетная обработка нескольких заявок за один проход
- Хранение заявок и результатов в PostgreSQL
- Получение статуса и результата отчёта через HTTP API
- Кэширование статуса отчёта в памяти приложения
- Unit-тесты для бизнес-логики

  gRPC не использовался, вместо него HTTP API

## Как запустить проверку

1. Нужны Docker Desktop,свободные порты 5432, 5672, 15672, 8080

2. Файл .env:

```
POSTGRES_DB=db_conversion_service

POSTGRES_USER=conversion_user

POSTGRES_PASSWORD=Pass_TEST12

POSTGRES_PORT=5432

RABBITMQ_DEFAULT_USER=report_user

RABBITMQ_DEFAULT_PASS=Pass_TEST12

RABBITMQ_PORT=5672

RABBITMQ_MANAGEMENT_PORT=15672
```

3. Открыть Swagger на localhost:8080

    Эндпоинты:

    POST /dev/report-requests - отправляет тестовый запрос на построение отчёта в очередь RabbitMQ
      ```
      {
        "requestId": "00000000-0000-0000-0000-000000000000",
        "externalMessageId": "",
        "productId": "PROD-001",
        "checkoutId": "CHK-STD",
        "periodFrom": "2026-03-01",
        "periodTo": "2026-03-03"
      }
      ```

    POST /dev/process-pending - ручной запуск пакетной обработки ожидающих заявок

    GET /api/reports/{requestId} - возвращает текущее состояние отчёта по requestId
