# DB_parse

REST API сервис для получения XML документа закупки с сайта `zakupki.gov.ru`.

## Что делает сервис

Сервис принимает идентификатор закупки, определяет подходящий тип документа и возвращает XML.

Поддерживаются следующие типы документов:

- `notice`
- `contract`
- `contract223`

Сервис последовательно проверяет URL:

- `https://zakupki.gov.ru/epz/order/notice/printForm/viewXml.html?regNumber={id}`
- `https://zakupki.gov.ru/epz/contract/printForm/viewXml.html?contractReestrNumber={id}`
- `https://zakupki.gov.ru/epz/contractfz223/printForm/viewXml.html?contractNumber={id}`

## Требования

- `.NET SDK 10.0`

## Запуск

Из корня репозитория:

```powershell
dotnet run --project .\DB_parse\DB_parse.csproj
```

После запуска сервис по умолчанию доступен на:

```text
http://localhost:5000
```

Если нужно указать свой порт:

```powershell
dotnet run --project .\DB_parse\DB_parse.csproj --urls=http://localhost:5099
```

## API

### Получить XML по идентификатору закупки

```http
GET /api/xml/{registryNumber}
```

Примеры:

```powershell
curl http://localhost:5000/api/xml/0338200008525000109
curl http://localhost:5000/api/xml/3861701026824000058
curl http://localhost:5000/api/xml/80273021553250000050000
```

Успешный ответ:

- статус `200 OK`
- content-type `application/xml`

Возможные ошибки:

- `400 Bad Request` - если идентификатор пустой
- `404 Not Found` - если документ не найден ни по одному из поддерживаемых типов
- `502 Bad Gateway` - если внешний сервис недоступен или вернул ошибку

## Проверка

Сборка проекта:

```powershell
dotnet build .\DB_parse\DB_parse.csproj
```

Если сборка не выполняется из-за блокировки `DB_parse.exe`, сначала остановите запущенный экземпляр приложения через `Ctrl+C`.

## Структура проекта

- `DB_parse/Program.cs` - настройка web-приложения и HTTP endpoint
- `DB_parse/Services/ZakupkiXmlService.cs` - логика запроса XML с `zakupki.gov.ru`
