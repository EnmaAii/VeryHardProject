# DB_parse

REST API service for fetching an XML procurement document from `zakupki.gov.ru`.

## Current scope

Implemented now:

- REST API endpoint that accepts only a procurement identifier
- automatic detection of document type
- XML response to the client

Planned next:

- save received XML into PostgreSQL

## Supported source URLs

The service tries these URLs in order:

- `https://zakupki.gov.ru/epz/order/notice/printForm/viewXml.html?regNumber={id}`
- `https://zakupki.gov.ru/epz/contract/printForm/viewXml.html?contractReestrNumber={id}`
- `https://zakupki.gov.ru/epz/contractfz223/printForm/viewXml.html?contractNumber={id}`

## Requirements

- `.NET SDK 10.0`
- network access to `https://zakupki.gov.ru`
- PostgreSQL for the next stage

## Run

From repository root:

```powershell
dotnet run --project .\DB_parse\DB_parse.csproj
```

Default address:

```text
http://localhost:5000
```

Custom port:

```powershell
dotnet run --project .\DB_parse\DB_parse.csproj --urls=http://localhost:5099
```

## API

Request:

```http
GET /api/xml/{registryNumber}
```

Examples:

```powershell
curl http://localhost:5000/api/xml/0338200008525000109
curl http://localhost:5000/api/xml/3861701026824000058
curl http://localhost:5000/api/xml/80273021553250000050000
```

Successful response:

- `200 OK`
- `application/xml`

Possible errors:

- `400 Bad Request`
- `404 Not Found`
- `502 Bad Gateway`

## PostgreSQL draft

This stage is not implemented yet. Draft files were added:

- `db/001_create_xml_documents.sql`
- `DB_parse/appsettings.json`
- `DB_parse/Services/ProcurementXmlStorageService.cs`

## Configuration

All current API settings and PostgreSQL connection settings are stored in:

- `DB_parse/appsettings.json`

This includes:

- local endpoint path
- upstream base URL
- upstream route templates and query parameter names
- PostgreSQL connection string
- PostgreSQL host and port

The PostgreSQL connection string is described through host and port, for example:

```text
Host=localhost;Port=5432;Database=db_parse;Username=postgres;Password=postgres
```

## Build

```powershell
dotnet build .\DB_parse\DB_parse.csproj
```

If build fails because `DB_parse.exe` is locked, stop the running app first with `Ctrl+C`.

## Project structure

- `DB_parse/Program.cs` - web application startup and endpoint
- `DB_parse/Services/ZakupkiXmlService.cs` - XML download logic
- `DB_parse/Services/ProcurementXmlStorageService.cs` - placeholder for PostgreSQL XML save
- `db/001_create_xml_documents.sql` - PostgreSQL table draft
