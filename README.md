# JK Three Tier

A hands-on **C# / .NET learning sandbox** for exploring modern backend architecture: modular services, platform abstractions, gRPC, distributed actors, background work, observability, and Kubernetes deployment.

The repo is organized as a **three-tier application** (Vue frontend → ASP.NET Core APIs → PostgreSQL) with optional **API gateway**, **Docker Compose**, and **K3s/Kubernetes** manifests.

---

## Goal

Learn and practice **advanced C# and .NET backend techniques** in a realistic, multi-service codebase—not a single demo API.

---

## Technologies

### Language & runtime
- **C# 12+** with nullable reference types, records, `required`/`init` properties, pattern matching
- **.NET 9** (`net9.0`) across backend projects
- **Async/await** and `BackgroundService` for long-running workers

### Web & APIs
- **ASP.NET Core** minimal hosting (`WebApplication` / `IHostApplicationBuilder`)
- **REST** controllers with **API versioning** (`Asp.Versioning.Mvc`)
- **Swagger / OpenAPI** (`Swashbuckle.AspNetCore`, gRPC Swagger integration)
- **CORS** platform middleware
- **Health checks** (`/health`, `/health/live`, `/health/ready`)

### Inter-service communication
- **gRPC** (`Grpc.AspNetCore`, `Grpc.Net.Client`)
- **Protocol Buffers** (`Google.Protobuf`, `Grpc.Tools` code generation)
- **gRPC JSON transcoding** (REST-like HTTP over gRPC for Swagger)
- Typed **gRPC client wrappers** per module (`JK.*.Client.Grpc`)
- **Centralizer** pattern: BFF-style aggregation via gRPC clients

### Data & persistence
- **PostgreSQL** (`Npgsql`, `Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Entity Framework Core 9** (DbContext, repositories, Unit of Work)
- **FluentMigrator** (schema-per-module migrations, embedded SQL scripts)
- **Repository + Unit of Work** patterns (`JK.Platform.Persistence.EfCore`)
- Per-module database projects (`JK.*.Database`)

### Distributed systems & background processing
- **Microsoft Orleans 10** (grains, reminders, ADO.NET clustering & persistence in Messaging)
- **Cron-based scheduling** (`Cronos`) for recurring API message tasks
- **Long-running task framework** (`JK.Platform.LongRunningTasks`): parent/child jobs, locking, retries, progress tracking, parallel workers
- **Hosted services** (`BackgroundService`) for task polling

### Configuration & DI
- **Convention-based DI** via `[Injectable]` attribute and assembly scanning
- **Module installers** (`IModuleInstaller`) discovered at runtime (`DomainDiscovery`)
- **Platform configurators** (`IBuilderConfigurator`, `IApplicationConfigurator`) for composable startup
- **Custom `IConfiguration` provider** pulling settings from Configuration gRPC service (`JK.Configuration.Provider`)
- **Options pattern** (`IOptions`, `BindConfiguration`)

### Cross-cutting concerns
- **Serilog** structured logging (compact JSON, enrichers, exception details, file + console sinks)
- **Correlation IDs** (`AsyncLocal`, middleware)
- **AutoMapper** for DTO mapping
- **FluentValidation** for request validation

### Observability
- **OpenTelemetry** tracing & metrics (ASP.NET Core, HttpClient, gRPC client instrumentation)
- **OTLP export** to collectors
- Kubernetes-oriented stack: **Grafana Alloy**, **Loki**, **Tempo** (values/manifests under `k8s/`)

### Tooling & DevOps
- **System.CommandLine** migration CLI (`JK.Migrations.Cli`)
- **Docker** per-service Dockerfiles under `backend/Api/*`
- **Docker Compose** (`docker/`) for local Postgres + gateway stack
- **Nginx** reverse proxy gateway (`gateway/`)
- **Kubernetes / Kustomize** manifests (`k8s/`)
- **Traefik** ingress configuration

### Frontend
- **Vue 3** (`<script setup>` SFCs)
- **Vite 7** dev server and build

---

## Use cases (what this project teaches)

- **Modular monolith → microservices**: each domain module (`Configuration`, `Order`, `Offer`, `Messaging`, `Centralizer`) ships as reusable class libraries hosted by thin `JK.*.CZ` API projects
- **Platform-driven startup**: reflection-based discovery of installers, configurators, and injectable services—minimal `Program.cs` in hosts
- **REST + gRPC dual exposure** of the same domain logic with shared contracts
- **Multimarket / multiservice configuration** store with scoped keys (`MarketCode`, `ServiceCode`) and remote configuration provider for consuming services
- **Cross-service reads** via gRPC (Centralizer aggregating Order data)
- **Schema-owned migrations** per module with a shared FluentMigrator runner
- **Orleans grains & reminders** for durable scheduled messaging tasks
- **Chunked sync jobs** with parent/child long-running tasks, optimistic locking, and OpenTelemetry metrics
- **Correlation propagation** across HTTP/gRPC boundaries
- **K8s deployment** patterns: separate DBs per service, secrets, probes, dual HTTP/gRPC ports, migration jobs
- **Observability pipeline** wiring (logs, traces, metrics) for production-style debugging

---

## Architecture (high level)

```
frontend (Vue) ──► gateway (nginx) ──► API hosts (JK.*.CZ)
                                         │
                    ┌────────────────────┼────────────────────┐
                    ▼                    ▼                    ▼
              Configuration           Order / Offer         Messaging
              (settings)            (CRUD + tasks)        (Orleans + cron)
                    │                    │                    │
                    └────────────────────┴────────────────────┘
                                         ▼
                                   PostgreSQL
```

### API hosts (`backend/Api/`)
| Host | Module | Notable features |
|------|--------|------------------|
| `JK.Configuration.CZ` | Configuration | REST + gRPC CRUD, FluentMigrator |
| `JK.Order.CZ` | Order | REST + gRPC CRUD |
| `JK.Offer.CZ` | Offer | Long-running sync tasks, OpenTelemetry metrics |
| `JK.Messaging.CZ` | Messaging | Orleans silo, recurring API message scheduler |
| `JK.Centralizer.CZ` | Centralizer | REST BFF over Order gRPC |

### Platform packages (`backend/Platform/`)
Shared infrastructure: Core abstractions, ASP.NET Core hosting extensions, HTTP/CORS, REST server, Swagger, gRPC server/client, EF Core persistence helpers, Serilog, OpenTelemetry, database migrations, long-running tasks.

### Modules (`backend/Modules/`)
Each module typically includes: main library, `.Contracts`, `.Proto`, `.Client.Grpc`, and `.Database` projects.

---

## Repository layout

| Path | Purpose |
|------|---------|
| `backend/` | .NET solution, platform, modules, API hosts, migration CLI |
| `frontend/` | Vue 3 demo UI |
| `gateway/` | Nginx reverse proxy |
| `docker/` | Docker Compose for local development |
| `k8s/` | Kubernetes manifests and observability values |
| `database/` | Legacy SQL init script (superseded by FluentMigrator for most services) |
| `ai/` | Architecture standards and implementation plans (for AI-assisted development) |
| `script/` | K3s helper scripts |

---

## Quick start (local)

1. **PostgreSQL** — run via `docker/docker-compose.yml` or your own instance.
2. **Migrations** — use `JK.Migrations.Cli` or enable `Database:RunMigrationsOnStartup` in host `appsettings.json`.
3. **API host** — e.g. `dotnet run --project backend/Api/JK.Configuration.CZ`.
4. **Frontend** — `cd frontend && npm install && npm run dev`.
5. **Swagger** — typically at `/swagger` on each API host.

See module-specific docs in `backend/Modules/Configuration/README.md` and runbooks in `ai/`.

---

## Learning focus areas

If your goal is **advanced C#**, prioritize exploring:

1. `WebApplicationBuilderExtension` — composable platform bootstrap
2. `DomainDiscovery` + `IModuleInstaller` — convention-over-configuration
3. `[Injectable]` + `ServiceDiscovery` — attribute-driven DI registration
4. `CorrelationContextAccessor` — `AsyncLocal<T>` for request context
5. Orleans grains in `JK.Messaging` — distributed state & reminders
6. `LongRunningTaskWorker` — concurrent background processing with DB locking
7. `ConfigurationServerProvider` — custom `IConfigurationProvider` over gRPC
8. OpenTelemetry setup in `JK.Platform.Core.Observability`
9. FluentMigrator embedded SQL migrations per module

---

## Future learning points

Natural next steps to extend this codebase and deepen advanced C# / backend skills. Grouped by area; each item fits the existing modular platform without a full rewrite.

### Application architecture & patterns
- **MediatR** — CQRS-style commands/queries, pipeline behaviors (validation, logging, transactions), thinner controllers
- **Vertical slice architecture** — organize by feature (`CreateOrder`, `SyncOffers`) instead of layer folders
- **Domain events & outbox pattern** — reliable cross-module notifications without tight coupling
- **Result / railway-oriented error handling** — explicit success/failure instead of exceptions for business rules
- **Specification pattern** — composable EF Core query filters in repositories
- **Feature flags** — Microsoft.FeatureManagement or LaunchDarkly-style toggles per market/service

### Messaging & integration
- **MassTransit** (or **NServiceBus**, **Rebus**) — message buses, sagas, retries, dead-letter queues
- **RabbitMQ / Azure Service Bus** — async integration between Order, Offer, Messaging modules
- **Apache Kafka** — event streaming for audit logs or offer-change feeds
- **SignalR** — real-time UI updates (task progress, messaging status) from the Vue frontend
- **Hangfire** or **Quartz.NET** — alternative schedulers to compare with Orleans reminders and `Cronos`

### Data & persistence
- **MongoDB** (`MongoDB.Driver` or EF Core provider) — document store for unstructured offer payloads, message bodies, or audit trails alongside PostgreSQL
- **Redis** — distributed cache, session store, rate limiting, Orleans grain state (optional)
- **Dapper** — raw SQL performance path next to EF Core where needed
- **Read replicas & CQRS read models** — separate read DB or materialized views for Centralizer dashboards
- **Event sourcing** (Marten, EventStoreDB) — experiment on one bounded context (e.g. Order history)

### Security & identity
- **JWT + OpenID Connect** — `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Keycloak** or **Duende IdentityServer** — central auth for all `JK.*.CZ` hosts
- **Policy-based authorization** — market/service-scoped claims aligned with `MarketCode` / `ServiceCode`
- **mTLS** between gRPC services in K8s
- **Secrets management** — Vault, Azure Key Vault, or External Secrets Operator (replace plaintext `appsettings` / `db-secret` patterns)

### Resilience & performance
- **Polly** — retries, circuit breakers, timeouts on gRPC/HTTP clients (Centralizer → Order)
- **Rate limiting** — `Microsoft.AspNetCore.RateLimiting` on public REST endpoints
- **BenchmarkDotNet** — profile hot paths (long-running task worker, configuration provider)
- **Native AOT / trimming** — explore constraints for CLI tools or edge deployments
- **Channels & `IAsyncEnumerable`** — high-throughput streaming inside workers

### Testing
- **xUnit + FluentAssertions** — unit tests for services, validators, handlers
- **WebApplicationFactory** — integration tests per API host with in-memory or Testcontainers DB
- **Testcontainers** — spin up real PostgreSQL, MongoDB, RabbitMQ in tests
- **Docker deployment tests** — build images, run Compose/K8s stack in CI, smoke-test `/health` and Swagger
- **Contract testing (Pact)** — gRPC/REST contracts between Configuration, Order, Centralizer
- **ArchUnitNET** — enforce module boundaries (no direct DB access across modules)
- **Bogus / AutoFixture** — realistic test data for multimarket configuration

### DevOps, containers & cloud
- **Fix & modernize Docker Compose** — multi-service compose matching current `JK.*.CZ` hosts (replace legacy monolith layout)
- **Docker Compose profiles** — `dev`, `test`, `observability` stacks
- **Multi-stage Dockerfile optimization** — smaller images, `dotnet publish` trimming, non-root users
- **GitHub Actions / Azure DevOps pipelines** — build, test, push images, deploy to K3s
- **Helm charts** — parameterize `k8s/` manifests (replicas, secrets, ingress)
- **.NET Aspire** — local orchestration dashboard for all services + Postgres + OTLP
- **Terraform / Pulumi** — infrastructure as code for cloud Postgres, AKS, networking
- **Canary / blue-green deployments** — Traefik weighted routes or Argo Rollouts

### Observability & operations
- **Prometheus + Grafana dashboards** — metrics from OpenTelemetry meters (long-running tasks, gRPC latency)
- **Structured log correlation** — trace_id / span_id in Serilog output end-to-end
- **Health Checks UI** — aggregate `/health` from all modules in one dashboard
- **SLOs & alerting** — Alertmanager rules on error rate and task backlog
- **Chaos engineering** — kill pods, partition network, verify Orleans recovery and task retries

### Frontend & API experience
- **TypeScript migration** — typed API clients generated from OpenAPI
- **Pinia** — state management for multi-module admin UI
- **YARP** — replace or complement nginx gateway with .NET reverse proxy, transforms, auth
- **GraphQL (Hot Chocolate)** — optional BFF layer in Centralizer over gRPC backends
- **API rate-limit & versioning policies** — exercise `Asp.Versioning` with v2 breaking changes

### Advanced C# language & runtime
- **Source generators** — compile-time DI registration or `[Injectable]` code generation
- **Interceptors (.NET 8+)** — AOP for logging/validation without MediatR pipelines
- **`required` members & collection expressions** — tighten DTOs and options classes
- **`FrozenDictionary` / `SearchValues`** — micro-optimizations in hot configuration lookups
- **Half-decoupled hosting** — further reduce reflection in `DomainDiscovery` with source-generated assembly lists

### Suggested learning order

1. **MediatR** in one module (Order) — immediate payoff, minimal infrastructure
2. **Testcontainers + WebApplicationFactory** — safety net before bigger changes
3. **Docker deployment tests in CI** — validate images and health endpoints on every PR
4. **JWT auth** platform package — shared across all hosts
5. **MongoDB** in Offer or Messaging — practice polyglot persistence
6. **MassTransit** — event-driven link between Order and Messaging
7. **Helm / Aspire** — polish local and K8s developer experience

---

## License / status

Educational / work-in-progress codebase. Some deployment paths (Docker Compose monolith, frontend demo endpoint) are out of sync with the current modular APIs—see issues noted in project reviews.
