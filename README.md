# Страховое агентство - Курсовая работа

Веб-приложение на C# для управления страховым агентством.

## Структура проекта

Проект выполнен по принципам Domain-Driven Design (DDD) и Clean Architecture:

```
InsuranceAgency/
├── src/
│   ├── InsuranceAgency.Domain/          # Доменный слой (сущности, бизнес-правила)
│   ├── InsuranceAgency.Application/     # Слой приложения (сервисы, DTO, интерфейсы)
│   ├── InsuranceAgency.Infrastructure/  # Слой инфраструктуры (БД, внешние сервисы)
│   ├── InsuranceAgency.Web/             # Web API (контроллеры, middleware)
│   ├── InsuranceAgency.Worker/         # Фоновые задачи (обработка проблемных договоров)
│   └── InsuranceAgency.Tests/          # Тесты (unit и integration)
```

## Технологии

- **.NET 8.0**
- **ASP.NET Core Web API**
- **Entity Framework Core** (SQL Server)
- **AutoMapper**
- **Swagger/OpenAPI**
- **xUnit** (тестирование)
- **Moq** (моки для тестов)

## Функциональность

### Основные возможности:

1. **Управление клиентами**

   - Создание и просмотр клиентов
   - Хранение контактной информации

2. **Управление договорами**

   - Создание договоров страхования
   - Регистрация договоров
   - Отслеживание статусов договоров
   - Поиск проблемных договоров (просроченные, неоплаченные, требующие продления)

3. **Платежи**

   - Инициация платежей по договорам
   - Отслеживание статусов платежей
   - **Платежная система реализована как заглушка (MockPaymentGateway)** - не выполняет реальных платежей

4. **Фоновые задачи (Worker)**
   - Автоматическое обнаружение проблемных договоров
   - Отправка уведомлений клиентам
   - Обновление статусов договоров

## Настройка и запуск

### Требования

- .NET 8.0 SDK
- SQL Server (LocalDB или полноценный SQL Server)
- Visual Studio 2022 или VS Code

### Установка

1. Клонируйте репозиторий:

```bash
git clone <repository-url>
cd Kursovik_TP
```

2. Восстановите зависимости:

```bash
dotnet restore
```

3. Примените миграции базы данных:

```bash
cd src/InsuranceAgency.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../InsuranceAgency.Web
dotnet ef database update --startup-project ../InsuranceAgency.Web
```

4. Запустите Web API:

```bash
cd корневая папка
dotnet run --project src/InsuranceAgency.Web/InsuranceAgency.Web.csproj
```

5. Откройте Swagger UI либо страницы приложения:

```
http://localhost:.../swagger
hhtp://localhost:.../home
```

### Запуск Worker (фоновые задачи)

```bash
cd src/InsuranceAgency.Worker
dotnet run
```

### Запуск тестов

```bash
dotnet test src/InsuranceAgency.Tests/InsuranceAgency.Tests.csproj
```

## База данных

Проект использует SQL Server LocalDB по умолчанию. Строка подключения настраивается в `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InsuranceAgencyDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

## API Endpoints

### Клиенты

- `GET /api/clients` - Получить всех клиентов
- `GET /api/clients/{id}` - Получить клиента по ID
- `POST /api/clients` - Создать нового клиента

### Договоры

- `GET /api/contracts` - Получить все договоры
- `GET /api/contracts/{id}` - Получить договор по ID
- `POST /api/contracts` - Создать новый договор
- `POST /api/contracts/{id}/register` - Зарегистрировать договор

### Платежи

- `POST /api/payments` - Инициировать платеж

### Услуги

- `GET /api/services` - Получить все страховые услуги
- `GET /api/services/{id}` - Получить услугу по ID

## Платежная система

**Важно:** Платежная система реализована как **заглушка (MockPaymentGateway)**. Она не выполняет реальных платежей и всегда возвращает успешный результат с мнимым transactionId. Это сделано для целей курсовой работы и тестирования.

## Архитектура

Проект следует принципам:

- **Separation of Concerns** - разделение на слои
- **Dependency Inversion** - зависимости направлены внутрь
- **Repository Pattern** - абстракция доступа к данным
- **Service Layer** - бизнес-логика в сервисах
- **DTO Pattern** - передача данных через DTO
- **Value Objects** - Money, DateRange
- **Domain Events** - события домена

## Тестирование

Проект включает:

- **Unit тесты** - тестирование сервисов и доменной логики
- **Integration тесты** - тестирование API и работы с БД

## Автор

Курсовая работа выполнена в рамках семестра по дисциплине "Технологии программирования".

## Лицензия

Учебный проект.
