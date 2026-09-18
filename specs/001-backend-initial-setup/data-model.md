# Data Model & Domain Primitives: Backend Initial Architecture Setup

**Feature**: `001-backend-initial-setup`  
**Date**: 2026-09-17  
**Status**: Completed  

## 1. Domain Building Blocks & Core Primitives

All domain models reside in `Pqrsdf.Domain` as pure POCOs without framework dependencies, organized under `Shared/` in purposeful subdirectories.

### 1.1 Base Entity (`Pqrsdf.Domain/Shared/Entities/Entity.cs`)
Represents an identifiable domain object with identity-based equality.
- **Attributes**:
  - `Id`: `TId` (Primary key identifier, protected set)
  - `CreatedAtUtc`: `DateTime`
  - `UpdatedAtUtc`: `DateTime?`
- **Invariants & Behavior**:
  - Entities encapsulate domain events via `IReadOnlyCollection<IDomainEvent> DomainEvents`.
  - Equality is determined strictly by runtime type and entity `Id`.

### 1.2 Value Objects (`Pqrsdf.Domain/Shared/ValueObjects/ValueObject.cs`)
Represents an immutable concept defined purely by its structural attributes (no identity).
- **Implementation Mechanism**:
  - Implemented strictly using C# **`record`** types (e.g., `public abstract record ValueObject;` or concrete immutable records).
  - Takes full advantage of C#'s compiler-generated value semantics: native value equality (`Equals`), `GetHashCode`, and relational equality operators (`==`, `!=`) are handled automatically by the runtime and compiler without requiring manual overrides or `GetEqualityComponents` boilerplate.
  - Immutability is strictly enforced via positional or `init`-only properties.
  - Invariants are validated in the constructor or factory method; invalid states throw validation domain errors or fail to instantiate.

### 1.3 Aggregate Root Marker (`Pqrsdf.Domain/Shared/Entities/IAggregateRoot.cs`)
- Marker interface denoting the root of an aggregate boundary. Repositories only persist and load aggregate roots.

### 1.4 Domain Events (`Pqrsdf.Domain/Shared/Events/IDomainEvent.cs`)
- Marker interface for events published when state mutations occur within aggregate boundaries.

### 1.5 Result Pattern & Error Types (`Pqrsdf.Domain/Shared/Results/`)
Enforces the mandatory Result Pattern across the entire application without using exceptions for business flow control.

#### `Error` (C# `record`) — `Pqrsdf.Domain/Shared/Results/Error.cs`
- **Attributes**:
  - `Code`: `string` (Machine-readable identifier in English, e.g., `System.Unavailable`, `Validation.InvalidInput`)
  - `Message`: `string` (Human-readable description in Spanish, e.g., `"El servicio se encuentra temporalmente degradado"`)
  - `Type`: `ErrorType` enum (`Failure`, `Validation`, `NotFound`, `Conflict`, `Unauthorized`)
- **Predefined Constants**:
  - `Error.None`: Represents no error.
  - `Error.NullValue`: Code `"Error.NullValue"`, Message `"El valor provisto no puede ser nulo."`

#### `Result<T>` — `Pqrsdf.Domain/Shared/Results/Result.cs`
- **Attributes**:
  - `IsSuccess`: `bool` (True if operation succeeded)
  - `IsFailure`: `bool` (!IsSuccess)
  - `Value`: `T` (Payload when successful; throws InvalidOperationException if accessed on failure)
  - `Error`: `Error` (Failure details when unsuccessful)
- **Factory Methods**:
  - `Result<T>.Success(T value)`
  - `Result<T>.Failure(Error error)`

---

## 2. Operational Entities & DTOs

### 2.1 SystemStatus (Operational Domain/Infrastructure Record)
Represents the operational snapshot of the backend service.
- **Attributes**:
  - `Status`: `string` (`"Healthy"` | `"Degraded"` | `"Unhealthy"`)
  - `Service`: `string` (e.g., `"PQRSDF API"`)
  - `Version`: `string` (e.g., `"1.0.0"`)
  - `Environment`: `string` (e.g., `"Development"`, `"Production"`)
  - `TimestampUtc`: `DateTime`
  - `DatabaseConnected`: `bool`
  - `Message`: `string` (Human-readable status summary in Spanish, e.g., `"Sistema operando normalmente"`)

### 2.2 SystemStatusDto (Application DTO `record`)
Located in `Application/Features/System/UseCases/GetSystemStatus/SystemStatusDto.cs`:
- **Implementation**: Modeled as a C# **`record`** (`public sealed record SystemStatusDto(...)`) for immutability, concise syntax, and native value equality.
- **Properties**:
  - `Status`: `string`
  - `Service`: `string`
  - `Version`: `string`
  - `Environment`: `string`
  - `Timestamp`: `DateTime`
  - `DatabaseConnected`: `bool`
  - `Message`: `string` (Spanish text)

---

## 3. Database Schema (Initial EF Core Baseline)

The initial architecture setup validates the single unified database connection to Microsoft SQL Server.

### Table: `__EFMigrationsHistory`
- Managed automatically by EF Core for migration version tracking.

*(Subsequent business features such as PQRSDF registration and user identities will define domain entity tables mapped via Fluent API configurations)*.
