# Research & Technical Decisions: Backend Initial Architecture Setup

**Feature**: `001-backend-initial-setup`  
**Date**: 2026-09-17  
**Status**: Completed  

## 1. Solution Layout & Clean Architecture (.NET 10)

### Decision
Structure the backend as a single .NET Solution (`Pqrsdf.sln`) housed under `backend/` with 4 strictly decoupled projects:
1. `Pqrsdf.Domain`: POCO core. Entities, Value Objects, Domain Events, Repositories/Contracts. Zero external dependencies.
2. `Pqrsdf.Application`: CQRS use cases, commands, queries, handlers, DTOs, domain validators. References `Pqrsdf.Domain`.
3. `Pqrsdf.Infrastructure`: EF Core 10 persistence, SQL Server DbContext, concrete repository implementations, adapters. References `Pqrsdf.Application` and `Pqrsdf.Domain`.
4. `Pqrsdf.Api`: ASP.NET Core 10 Web API, Controllers (`ControllerBase`), Global Exception Handler, Swagger/OpenAPI, DI composition root. References `Pqrsdf.Application` and `Pqrsdf.Infrastructure`.

### Uniform Folder Standardization (`Shared/` across all layers)
- **Elimination of generic `Common` folders**: Generic `Common` directories are prohibited across the entire solution.
- Standardize on `Shared/` with purpose-driven subdirectories:
  - `Pqrsdf.Domain/Shared/Entities/` (Base Entity, Aggregate Root markers)
  - `Pqrsdf.Domain/Shared/ValueObjects/` (Base Value Object record / domain markers)
  - `Pqrsdf.Domain/Shared/Results/` (Result pattern, Error records, ErrorType enum)
  - `Pqrsdf.Domain/Shared/Events/` (IDomainEvent interface)
  - `Pqrsdf.Application/Shared/Dtos/` (Cross-use-case reusable DTOs)
  - `Pqrsdf.Api/Shared/Controllers/` (Base API controllers)
  - `Pqrsdf.Api/Shared/Middleware/` (Global exception handling and diagnostic middleware)

### Rationale
- Aligns with Constitution Principle II and eliminates ambiguous dumping-ground folders.
- Cleanly groups cross-cutting artifacts into semantic, purpose-aligned locations.

---

## 2. Value Objects and DTOs using C# `record` Types

### Decision
- **Value Objects as C# `record` types**: Instead of traditional classes overriding `Equals`, `GetHashCode`, `==`, and `!=` based on custom `GetEqualityComponents()` reflection/lists, Value Objects in `Domain` are implemented strictly as C# `record` types (e.g., `public abstract record ValueObject;` or concrete positional/init records).
- **DTOs as C# `record` types**: All Data Transfer Objects in `Application` (both use-case specific and shared) are implemented as `record` types.

### Rationale
- In C#, `record` types provide native, compiler-generated value equality semantics out-of-the-box (`Equals`, `GetHashCode`, `==`, `!=`), guaranteed immutability via `init` properties, and clean non-destructive mutation (`with` expressions).
- Drastically reduces boilerplate code, improves readability, and eliminates runtime reflection overhead.

### Alternatives Considered
- **Class-based Value Objects with `GetEqualityComponents()`**: Evaluated and discarded. In modern C# (.NET 10), `record` is the idiomatic, built-in language construct for value semantics.

---

## 3. CQRS Implementation & Use Case Organization

### Decision
- Organize use cases under `Pqrsdf.Application/Features/[FeatureName]/UseCases/[UseCaseName]/`.
- Each use case folder co-locates:
  - Command or Query record (`[UseCaseName]Command.cs` or `[UseCaseName]Query.cs`)
  - Execution Handler (`[UseCaseName]CommandHandler.cs` or `[UseCaseName]QueryHandler.cs`)
  - Exclusive request/response DTO records (`[UseCaseName]Response.cs`, etc.)
- Reusable cross-feature DTOs are placed in `Pqrsdf.Application/Shared/Dtos/`.
- CQRS mediation: MediatR 12.x or lightweight interface-based dispatcher (`ICommandHandler<TCommand, TResult>`, `IQueryHandler<TQuery, TResult>`).
- Provide reference use case: `Pqrsdf.Application/Features/System/UseCases/GetSystemStatus/` containing `GetSystemStatusQuery.cs`, `GetSystemStatusQueryHandler.cs`, and `SystemStatusDto.cs`.

### Rationale
- Complies with Constitution Principle II and Directrices Técnicas (Mandatory folder structure).
- Ensures CQRS is strictly an organizational pattern within `Application` without infrastructure overhead.

---

## 4. Error Governance & The Result Pattern

### Decision
- Implement a custom, zero-dependency `Result<T>` and `Result` structure in `Pqrsdf.Domain/Shared/Results/`:
  - `Result<T>` encapsulates `IsSuccess`, `Value`, `Error` (with `ErrorCode`, `ErrorMessage` in Spanish).
  - Strongly typed `Error` record (`Code`, `Message`, `Type`).
- All CQRS handlers return `Task<Result<TResponse>>`.
- Base controller `ApiControllerBase` in `Pqrsdf.Api/Shared/Controllers/` provides helper method `HandleResult<T>(Result<T> result)` mapping results to appropriate HTTP status codes.

### Rationale
- Strictly obeys Constitution Principle V: "Tanto en Backend como en Frontend se prohíbe el uso de excepciones para el control de flujo normal o errores esperados del negocio".

---

## 5. Centralized Global Exception Handling

### Decision
- Register ASP.NET Core 10's native `IExceptionHandler` (`GlobalExceptionHandler`) in `Pqrsdf.Api/Shared/Middleware/`.
- When an unhandled exception occurs (e.g., database timeout, null pointer, system crash):
  - Log exception with structured details and unique `TraceId`.
  - Return an RFC 7807 `ProblemDetails` response with HTTP 500 status.
  - Never disclose internal stack traces or connection strings; provide client message: *"Ha ocurrido un error inesperado en el servidor. Por favor intente más tarde."*

---

## 6. Persistence with Microsoft SQL Server & EF Core 10

### Decision
- Use `Microsoft.EntityFrameworkCore.SqlServer` version 10.x.
- `PqrsdfDbContext` resides in `Pqrsdf.Infrastructure/Persistence/`.
- Entity configurations are specified via `IEntityTypeConfiguration<T>` classes implementing Fluent API.
- Migrations managed via `dotnet-ef` targeting SQL Server.

---

## 7. Language & Localization Standards

### Decision
- Codebase, class names, method signatures, properties, database tables, and internal logs are 100% in English.
- Citizen/user feedback, error descriptions, and validation messages returned in API envelopes are 100% in Spanish.
