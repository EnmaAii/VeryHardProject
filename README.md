# DB_parse

REST API service for fetching and processing XML procurement documents from `zakupki.gov.ru`.

---

## Current scope

Implemented:

### 1. XML retrieval

* REST API endpoint that accepts a procurement identifier
* automatic detection of document type
* returns XML document to client

### 2. XML persistence

* received XML is stored in PostgreSQL

### 3. XML parsing (attachments)

* XML is parsed inside PostgreSQL
* attachment metadata is extracted:

  * `fileName`
  * `url`
  * `description`
* results are stored in a separate table
* parsing is executed automatically via `pg_cron`

---

## Supported source URLs

The service tries these URLs in order:

* `https://zakupki.gov.ru/epz/order/notice/printForm/viewXml.html?regNumber={id}`
* `https://zakupki.gov.ru/epz/contract/printForm/viewXml.html?contractReestrNumber={id}`
* `https://zakupki.gov.ru/epz/contractfz223/printForm/viewXml.html?contractNumber={id}`

---

## Requirements

* `.NET SDK 10.0`
* Docker (for PostgreSQL with pg_cron)
* network access to `https://zakupki.gov.ru`

---

# Full setup guide

## 1. Start PostgreSQL with pg_cron

### Dockerfile

Create file:

```bash
Dockerfile.postgres
```

```dockerfile
FROM postgres:16

RUN apt-get update && apt-get install -y postgresql-16-cron

RUN echo "shared_preload_libraries = 'pg_cron'" >> /usr/share/postgresql/postgresql.conf.sample
RUN echo "cron.database_name = 'db_parse'" >> /usr/share/postgresql/postgresql.conf.sample
```

---

### docker-compose

Create file:

```bash
docker-compose.yml
```

```yaml
version: '3.9'

services:
  postgres:
    build:
      context: .
      dockerfile: Dockerfile.postgres
    container_name: postgres_pgcron
    restart: always
    environment:
      POSTGRES_DB: db_parse
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

---

### Run database

```bash
docker-compose up --build
```

---

## 2. Initialize database schema

### Create base table (XML storage)

```bash
psql -h localhost -U postgres -d db_parse -f db/001_create_xml_documents.sql
```

Creates table:

* `procurement_xml_documents`

  * procurement_id
  * xml_document (XML)
  * created_at

---

### Create parsing logic + cron

```bash
psql -h localhost -U postgres -d db_parse -f db/002_parse_attachments.sql
```

This file contains:

* table `procurement_attachments`
* function `parse_procurement_attachments()`
* pg_cron job

---

## 3. Run API

```bash
dotnet run --project DB_parse/DB_parse.csproj
```

Default address:

```text
http://localhost:5000
```

---

## 4. Use API

```bash
curl http://localhost:5000/api/xml/0338200008525000109
```

What happens:

1. XML is fetched from zakupki.gov.ru
2. XML is returned to client
3. XML is saved into PostgreSQL

---

## 5. Automatic parsing

`pg_cron` runs:

```text
*/5 * * * *
```

every 5 minutes it executes:

```sql
SELECT parse_procurement_attachments();
```

---

## 6. Verify data

### XML storage

```sql
SELECT * FROM procurement_xml_documents;
```

---

### Parsed attachments

```sql
SELECT * FROM procurement_attachments;
```

---

### Cron jobs

```sql
SELECT * FROM cron.job;
```

---

### Cron execution logs

```sql
SELECT * 
FROM cron.job_run_details
ORDER BY start_time DESC;
```

---

# Database structure

## 1. procurement_xml_documents

Stores raw XML:

* `id`
* `procurement_id`
* `xml_document`
* `created_at`

---

## 2. procurement_attachments

Stores parsed attachment metadata:

* `id`
* `procurement_xml_id` (FK)
* `file_name`
* `url`
* `description`
* `created_at`

---

# Key components

## API layer

* `Program.cs` — endpoint definition
* `ZakupkiXmlService.cs` — XML download logic
* `ProcurementXmlStorageService.cs` — saves XML into DB

---

## Database layer

* `001_create_xml_documents.sql` — XML storage table
* `002_parse_attachments.sql`:

  * attachments table
  * parsing function (PL/pgSQL + XPath)
  * pg_cron scheduling

---

## Parsing logic

* XML is processed using PostgreSQL `xpath`
* namespaces handled via `local-name()`
* multiple attachments extracted per document

---

# Processing pipeline

```text
API request
    ↓
Fetch XML
    ↓
Save XML (PostgreSQL)
    ↓
pg_cron (every 5 min)
    ↓
parse_procurement_attachments()
    ↓
attachments saved to DB
```

---

# Notes

* PostgreSQL must be started with `pg_cron` enabled
* first run requires manual schema initialization
* XML parsing depends on actual structure of zakupki documents
* repeated cron runs are safe (can be configured with unique constraints)

---

# Project structure

* `DB_parse/Program.cs` — web application startup
* `DB_parse/Services/ZakupkiXmlService.cs` — XML retrieval
* `DB_parse/Services/ProcurementXmlStorageService.cs` — DB persistence
* `db/001_create_xml_documents.sql` — XML table
* `db/002_parse_attachments.sql` — parsing + cron

---

# Build

```bash
dotnet build DB_parse/DB_parse.csproj
```

---

# Summary

The system now:

✔ retrieves XML documents
✔ stores them in PostgreSQL
✔ parses attachments automatically
✔ schedules processing via pg_cron

---

Next steps (planned):

* download files by URL
* extract text from documents
* generate embeddings
* build AI agent
