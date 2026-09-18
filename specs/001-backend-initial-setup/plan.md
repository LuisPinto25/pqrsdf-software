# Implementation Plan: Backend Initial Architecture Setup

**Branch**: `001-backend-initial-setup` | **Date**: 2026-09-17 | **Spec**: [specs/001-backend-initial-setup/spec.md](spec.md)

**Input**: Feature specification from `specs/001-backend-initial-setup/spec.md`

## Summary

Establish the foundational backend architecture for the PQRSDF management platform using ASP.NET Core 10 following Clean Architecture (Domain, Application, Infrastructure, API), CQRS organizational grouping, Domain-Driven Design building blocks leveraging C# `record` types for Value Objects and DTOs, the Result Pattern (`Result<T, Error>`), centralized ProblemDetails exception handling, purpose-driven `Shared/` subdirectories across all layers, and Microsoft SQL Server persistence via Entity Framework Core. This baseline delivers an end-to-end operational verification pipeline, automated health checks, OpenAPI contracts, and a live reference use case (`GetSystemStatus`) demonstrating the mandatory folder structure with unit and architectural tests.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (ASP.NET Core 10) with `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.

**Primary Dependencies**:
- `Microsoft.AspNetCore.OpenApi` / `Swashbuckle.AspNetCore` (OpenAPI documentation)
- `Microsoft.EntityFrameworkCore.SqlServer` (10.x, single unified database)
- `Microsoft.EntityFrameworkCore.Design` (10.x, database migrations)
- `MediatR` (12.x, lightweight internal CQRS handler dispatch)
- `Microsoft.Extensions.Diagnostics.HealthChecks` (System health probes)

**Storage**: Microsoft SQL Server (single unified relational database for reads and writes; no secondary databases or replication message queues).

**Testing**: xUnit, FluentAssertions, Moq, and NetArchTest (verifying layer dependencies and architecture rules).

**Target Platform**: Linux / Cross-platform server environment (.NET 10 Runtime).

**Project Type**: RESTful Web API service with Clean Architecture class libraries.

**Performance Goals**:
- Health check and system status response latency < 200ms (SC-004).
- 0 unhandled exception stack traces leaked to clients (SC-003).

**Constraints**:
- Strict unidirectional dependencies (`Domain` <- `Application` <- `Infrastructure` / `Api`).
- Mandatory use case folder structure: `Application/Features/[FeatureName]/UseCases/[UseCaseName]/`.
- Shared DTOs strictly located in `Application/Shared/Dtos/`.
- Uniform folder naming convention: No generic `Common` folders; use `Shared/` with purpose-driven subdirectories (`Entities`, `ValueObjects`, `Results`, `Events`, `Controllers`, `Middleware`).
- Value Objects and DTOs implemented as C# `record` types for native compiler-generated value equality.
- 100% of codebase, symbols, and database artifacts in English.
- 100% of user-facing messages and validation errors in Spanish.

**Scale/Scope**: Initial architecture baseline serving as the foundational scaffold for all future PQRSDF domain features.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Gate | Status | Justification / Verification |
| :--- | :---: | :--- |
| **I. Stack Tecnológico (ASP.NET Core 10)** | ✅ PASS | Solution targets .NET 10 (`net10.0`) exclusively, decoupled RESTful contracts. |
| **II. Clean Architecture (4 capas concéntricas)** | ✅ PASS | Four decoupled projects (`Domain`, `Application`, `Infrastructure`, `Api`). Pure POCO domain. Dependency Inversion enforced. |
| **II. CQRS Organizativo Co-ubicado** | ✅ PASS | Use cases organized strictly under `Application/Features/[FeatureName]/UseCases/[UseCaseName]/` with co-located Query/Command, Handler, and DTOs. Reference `GetSystemStatus` provided. |
| **II. Base de Datos Única (Sin Sobreingeniería)** | ✅ PASS | Single SQL Server database for reads and writes; no dual databases, read replicas, or event brokers. |
| **III. DDD & Tipos Fuertes** | ✅ PASS | Rich domain models (`Entity<TId>`, `ValueObject` as C# `record`, `IAggregateRoot`), guaranteed invariants, no primitive obsession. DTOs modeled as `record`. |
| **V. Gobernanza de Errores (Result Pattern & Global Exception)** | ✅ PASS | Custom `Result<T>` and `Error` (record) primitives for business flow; `GlobalExceptionHandler` (`IExceptionHandler`) emitting Spanish RFC 7807 ProblemDetails. |
| **VI. SOLID & Prevención de Code Smells** | ✅ PASS | Single responsibility per handler, no God objects, NetArchTest architectural unit tests. |
| **VII. Convención de Idioma** | ✅ PASS | Code, classes, and database in English; user-facing messages and validation feedback strictly in Spanish. |

## Project Structure

### Documentation (this feature)

```text
specs/001-backend-initial-setup/
├── checklists/
│   └── requirements.md    # Spec quality checklist
├── contracts/
│   ├── system-status-contract.json    # JSON Schema for SystemStatus response
│   └── problem-details-contract.json  # JSON Schema for RFC 7807 error format
├── data-model.md          # Domain primitives and operational models
├── plan.md                # This file (/speckit-plan command output)
├── quickstart.md          # Quickstart and validation guide
├── research.md            # Technical decisions and rationale
└── spec.md                # Feature specification
```

### Source Code (repository root)

```text
backend/
├── Pqrsdf.sln
├── src/
│   ├── Domain/
│   │   ├── Pqrsdf.Domain.csproj
│   │   ├── Shared/
│   │   │   ├── Entities/
│   │   │   │   ├── Entity.cs
│   │   │   │   └── IAggregateRoot.cs
│   │   │   ├── ValueObjects/
│   │   │   │   └── ValueObject.cs (record-based base/marker for domain value objects)
│   │   │   ├── Results/
│   │   │   │   ├── Result.cs
│   │   │   │   ├── Error.cs (record)
│   │   │   │   └── ErrorType.cs
│   │   │   └── Events/
│   │   │       └── IDomainEvent.cs
│   │   └── Constants/
│   │       └── DomainMessages.cs (Spanish error message catalog)
│   ├── Application/
│   │   ├── Pqrsdf.Application.csproj
│   │   ├── DependencyInjection.cs
│   │   ├── Shared/
│   │   │   └── Dtos/ (Reusable DTO records)
│   │   └── Features/
│   │       └── System/
│   │           └── UseCases/
│   │               └── GetSystemStatus/
│   │                   ├── GetSystemStatusQuery.cs (record)
│   │                   ├── GetSystemStatusQueryHandler.cs
│   │                   └── SystemStatusDto.cs (record)
│   ├── Infrastructure/
│   │   ├── Pqrsdf.Infrastructure.csproj
│   │   ├── DependencyInjection.cs
│   │   └── Persistence/
│   │       ├── PqrsdfDbContext.cs
│   │       └── Configurations/
│   └── Api/
│       ├── Pqrsdf.Api.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Shared/
│       │   ├── Controllers/
│       │   │   └── ApiControllerBase.cs (Result to ActionResult translation)
│       │   └── Middleware/
│       │       └── GlobalExceptionHandler.cs
│       └── Controllers/
│           └── V1/
│               └── SystemController.cs
└── tests/
    ├── Domain.UnitTests/
    │   ├── Pqrsdf.Domain.UnitTests.csproj
    │   └── Shared/
    │       ├── Results/
    │       │   └── ResultTests.cs
    │       └── ValueObjects/
    │           └── ValueObjectTests.cs
    ├── Application.UnitTests/
    │   ├── Pqrsdf.Application.UnitTests.csproj
    │   └── Features/
    │       └── System/
    │           └── GetSystemStatusQueryHandlerTests.cs
    └── Architecture.Tests/
        ├── Pqrsdf.Architecture.Tests.csproj
        └── ArchitectureTests.cs (Layer dependency rules verification)
```

**Structure Decision**:
Multi-project Clean Architecture layout housed under `backend/`. This structure strictly satisfies Constitution Principle II and eliminates ambiguous `Common` folders by standardizing on `Shared/` with purpose-driven subdirectories across all layers (`Domain/Shared/Entities`, `Domain/Shared/ValueObjects`, `Domain/Shared/Results`, `Domain/Shared/Events`, `Application/Shared/Dtos`, `Api/Shared/Controllers`, `Api/Shared/Middleware`).

## Complexity Tracking

> *Constitution Check has 0 violations; no deviations require justification.*

| Violation | Why Needed | Simpler Alternative Rejected Because |
| :--- | :--- | :--- |
| *None* | *N/A* | *All decisions strictly adhere to Constitution v1.2.0* |
