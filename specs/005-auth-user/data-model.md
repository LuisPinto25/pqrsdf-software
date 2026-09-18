# Data Model & Schema Specification: User Authentication and Role-Based Access Control

**Feature**: [User Authentication and Role-Based Access Control](spec.md)  
**Date**: 2026-09-18  
**Status**: Completed  

---

## 1. Conceptual Domain Entities & Value Objects

### 1.1 Aggregate Root: `User` (Domain)

Represents an internal operator, official, or system administrator who logs into the PQRSDF management application.

| Property | Type | Nullable | Invariants / Constraints | Description |
| :--- | :--- | :---: | :--- | :--- |
| `Id` | `Guid` | No | Non-empty `Guid` | Unique aggregate root identifier. |
| `Email` | `Email` (VO) | No | Valid RFC email, max 254 chars | Unique login email identifier. |
| `PasswordHash` | `PasswordHash` (VO) | No | Non-empty BCrypt hash string | Cryptographic password hash. |
| `FullName` | `string` | No | 2-150 characters, trimmed | Staff member's official display name. |
| `Role` | `UserRole` (Enum) | No | Must be `Funcionario` or `Administrador` | Operational authorization level. |
| `IsActive` | `bool` | No | Defaults to `true` | Account active/suspended flag. |
| `CreatedAtUtc` | `DateTime` | No | UTC timestamp | Account creation date and time. |
| `LastLoginAtUtc` | `DateTime?` | Yes | UTC timestamp | Timestamp of the most recent successful login. |

#### Domain Invariants & Methods
- `User.Create(...)`: Factory method enforcing non-empty fields, valid email format, valid role, and initializing `IsActive = true`.
- `user.RecordLogin(DateTime utcNow)`: Updates `LastLoginAtUtc` upon successful authentication.
- `user.Deactivate()`: Sets `IsActive = false` (blocks future logins).
- `user.Activate()`: Sets `IsActive = true`.
- `user.ChangePassword(PasswordHash newHash)`: Updates password hash and enforces that new hash differs.

---

### 1.2 Value Objects

#### `Email` (Domain/ValueObjects/Email.cs)
- Encapsulates an institutional email address.
- Invariants:
  - Cannot be null, empty, or pure whitespace.
  - Maximum length: 254 characters.
  - Normalized to lowercase for case-insensitive uniqueness and comparison.
  - Matches standard email regex validation.

#### `PasswordHash` (Domain/ValueObjects/PasswordHash.cs)
- Encapsulates the BCrypt cryptographic hash string.
- Invariants:
  - Cannot be null or whitespace.
  - Minimum length: 20 characters (standard BCrypt hashes are 60 characters).
  - Implements value equality to prevent exposure of raw strings.

---

### 1.3 Enums

#### `UserRole` (Domain/Enums/UserRole.cs)

```csharp
namespace Pqrsdf.Domain.Enums;

public enum UserRole
{
    Funcionario = 1,
    Administrador = 2
}
```

---

## 2. Application DTOs

### 2.1 Login Input: `LoginUserRequestDto` (Application)
```json
{
  "email": "funcionario@pqrsdf.gov.co",
  "password": "Funcionario123*"
}
```

### 2.2 Login Output: `LoginUserResponseDto` (Application)
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 28800,
  "user": {
    "id": "e49d34f1-9bd7-40a5-926b-9548be740f02",
    "email": "funcionario@pqrsdf.gov.co",
    "fullName": "Funcionario de PQRSDF",
    "role": "Funcionario"
  }
}
```

### 2.3 Shared User Profile: `UserProfileDto` (Application/Shared/Dtos)
- `id`: `Guid`
- `email`: `string`
- `fullName`: `string`
- `role`: `string` ("Funcionario" or "Administrador")

---

## 3. Relational Persistence Schema (Microsoft SQL Server)

### 3.1 Table Definition: `Users`

```sql
CREATE TABLE [dbo].[Users] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [Email]           NVARCHAR(254)    NOT NULL,
    [PasswordHash]    NVARCHAR(255)    NOT NULL,
    [FullName]        NVARCHAR(150)    NOT NULL,
    [Role]            INT              NOT NULL,
    [IsActive]        BIT              NOT NULL DEFAULT 1,
    [CreatedAtUtc]    DATETIME2(7)     NOT NULL,
    [LastLoginAtUtc]  DATETIME2(7)     NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] 
    ON [dbo].[Users] ([Email] ASC);
```

### 3.2 Seed Data Specifications

The database migration seeds two permanent development/test accounts:

| Id | Email | Role | FullName | Raw Password (Dev) |
| :--- | :--- | :--- | :--- | :--- |
| `B81B94C8-E0C7-4187-8A33-89AC54395E01` | `admin@pqrsdf.gov.co` | `Administrador` (2) | Administrador del Sistema | `Admin123*` |
| `E49D34F1-9BD7-40A5-926B-9548BE740F02` | `funcionario@pqrsdf.gov.co` | `Funcionario` (1) | Funcionario de PQRSDF | `Funcionario123*` |

*Note: In the EF Core migration script, password hashes are pre-generated using BCrypt with work factor 11.*
