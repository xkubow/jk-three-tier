# Configuration Module

Backend module for **multimarket, multiservice configuration** following the JK Backend Module Standard.

## Module layout (standard)

- **JK.Configuration** – main module (EF Core, repositories, services, REST + gRPC)
- **JK.Configuration.Client.Grpc** – typed gRPC client for other modules
- **JK.Configuration.Contracts** – DTOs and request/response models
- **JK.Configuration.Proto** – gRPC service and message definitions

Host API: **JK.Configuration.CZ** (`backend/Api/JK.Configuration.CZ`).

## Database

- **Database name:** `JK_Configurations` (per standard `JK_<DatabaseName>s`)
- **Main table:** `Configuration`

## Multimarket, multiservice model

Configuration is scoped by:

| Field         | Meaning |
|---------------|--------|
| **MarketCode**  | Market/region (e.g. `"CZ"`, `"SK"`, `"DE"`). `null` = applies to all markets. |
| **ServiceCode** | Service or product (e.g. `"Orders"`, `"Catalog"`, `"Auth"`). `null` = applies to all services. |
| **Key**         | Configuration key (required). |
| **Value**       | Configuration value (string; can store JSON for complex settings). |

Resolution order in applications is typically: **market + service** → **market only** → **service only** → **global** (both null).

### Example rows

| MarketCode | ServiceCode | Key           | Value    |
|------------|-------------|---------------|----------|
| null       | null        | DefaultLocale | en       |
| CZ         | null        | DefaultLocale | cs       |
| CZ         | Orders      | MaxCartSize   | 50       |
| null       | Auth        | TokenLifetime | 3600     |

### Entity fields

- `Id` (Guid), `MarketCode`, `ServiceCode`, `Key`, `Value`
- **Audit:** `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`
- **Soft delete:** `IsDeleted`, `DeletedAt`

Uniqueness of **(MarketCode, ServiceCode, Key)** is enforced in the service layer (no duplicate scope+key for non-deleted items).

## Checklist choices (standard)

- **Audit fields:** Yes – `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`
- **Soft delete:** Yes – `IsDeleted`, `DeletedAt`
- **Pagination:** Yes – `List` returns `PagedResponse<T>` with `Page`, `PageSize`, `TotalCount`
- **Filtering:** Yes – by `MarketCode`, `ServiceCode`, `SearchTerm`; `SortBy`, `SortDirection`
- **gRPC:** REST + gRPC (CRUD exposed on both)
- **EF Core migrations:** Generate from host API (see below)
- **Validation:** FluentValidation – Key required/max 500; Value required; optional max lengths for MarketCode/ServiceCode/CreatedBy/UpdatedBy
- **Authorization:** Anonymous (can be tightened per environment)

## CRUD

- **Create** – `POST /api/Configuration`
- **GetById** – `GET /api/Configuration/{id}`
- **List** – `GET /api/Configuration` (query: `MarketCode`, `ServiceCode`, `SearchTerm`, `Page`, `PageSize`, `SortBy`, `SortDirection`)
- **Update** – `PUT /api/Configuration/{id}`
- **Delete** – `DELETE /api/Configuration/{id}` (soft delete)

Same operations are exposed via gRPC (`ConfigurationGrpc` service).

## EF Core migrations

Install the EF Core tools if needed: `dotnet tool install --global dotnet-ef`

From the repository root, using the host API as startup project and the module’s `ConfigurationDbContext`:

```bash
dotnet ef migrations add InitialConfiguration --project backend/Modules/Configuration/JK.Configuration/JK.Configuration.csproj --startup-project backend/Api/JK.Configuration.CZ/JK.Configuration.CZ.csproj --context ConfigurationDbContext --output-dir Database/Migrations
```

Apply migrations (e.g. against local PostgreSQL):

```bash
dotnet ef database update --project backend/Modules/Configuration/JK.Configuration/JK.Configuration.csproj --startup-project backend/Api/JK.Configuration.CZ/JK.Configuration.CZ.csproj --context ConfigurationDbContext
```

Ensure `appsettings.json` (or environment) has a connection string pointing to the `JK_Configurations` database.
