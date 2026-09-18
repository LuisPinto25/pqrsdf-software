# Data Model: Public PQRSDF Ticket Registration (003-pqrsdf-make-ticket)

This document specifies the domain entities, value objects, enums, database schema mappings, and invariants for the public PQRSDF filing module.

---

## 1. Domain Entities & Aggregates

### 1.1 `PqrsdfTicket` (Aggregate Root)

Represents a citizen petition, complaint, claim, suggestion, denunciation, or compliment filed with the institution.

| Property | Type | Description | Invariants & Constraints |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | Unique ticket identifier | Primary key. Generated on instantiation. |
| `RadicadoNumber` | `RadicadoNumber` (VO) | Official legal tracking number | Format: `YYYY-NNNNNNNN`. Unique across database. |
| `Type` | `PqrsdfType` (Enum) | Type of PQRSDF | One of: `Petition`, `Complaint`, `Claim`, `Suggestion`, `Denunciation`, `Compliment`. |
| `DestinationAreaId` | `Guid` | Targeted administrative department | Foreign key to `DestinationArea`. Must reference an active area. |
| `IsAnonymous` | `bool` | Flag indicating anonymous submission | Can ONLY be `true` if `Type` is `Denunciation` or `Suggestion`. |
| `Applicant` | `Applicant?` (VO) | Citizen contact & identity details | Nullable if `IsAnonymous == true`. Mandatory if `IsAnonymous == false`. |
| `Subject` | `string` | Short summary of the request | Strict plain text. Min length 5, max length 150 characters. |
| `Description` | `string` | Narrative content of the request | Strict plain text. Min length 10, max length 4000 characters. |
| `CreatedAtUtc` | `DateTimeOffset` | Submission timestamp in UTC | Recorded at time of receipt. |
| `DueDate` | `DueDate` (VO) | Maximum statutory response deadline | Computed business days (15 or 30 days) excluding weekends & Colombian holidays. |
| `Status` | `TicketStatus` (Enum) | Current lifecycle state | Initialized to `Registered`. |

**Domain Methods**:
- `static Result<PqrsdfTicket> Create(...)`: Factory method enforcing all domain invariants before instantiation (e.g. non-anonymous validation, subject/description constraints, due date assignment).

---

### 1.2 `DestinationArea` (Entity)

Represents an administrative unit or department eligible to process filings.

| Property | Type | Description | Invariants & Constraints |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | Unique area identifier | Primary key. |
| `Name` | `string` | Official department name | Max 100 characters. Unique. |
| `Code` | `string` | Short department code | Max 10 characters (e.g. `ATC`, `JUR`, `FIN`). Uppercase alphanumeric. |
| `IsActive` | `bool` | Eligibility flag | Only areas with `IsActive == true` can be selected in the filing form. |

---

### 1.3 `RadicadoSequence` (Infrastructure / Persistence Entity)

Maintains the atomic yearly monotonic sequence for radicado numbers.

| Property | Type | Description | Invariants & Constraints |
| :--- | :--- | :--- | :--- |
| `Year` | `int` | Calendar year (e.g., 2026) | Primary Key. |
| `CurrentValue` | `long` | Highest allocated sequential number | Incremented atomically from `1` up to `99999999`. |

---

## 2. Value Objects (C# `record` types)

### 2.1 `RadicadoNumber`
- **Fields**: `string Value { get; init; }`
- **Validation Rules**:
  - Must match regex `^\d{4}-\d{8}$`.
  - Prefix must match the filing year (`YYYY`).
  - Sequence portion must be an 8-digit zero-padded number (`00000001` to `99999999`).
- **Equality**: Structural equality by value (`record`).

### 2.2 `Applicant`
- **Fields**:
  - `FullName`: `string` (required, 3 to 150 characters).
  - `IdentificationType`: `IdentificationType` enum (`CC`, `CE`, `TI`, `PA`, `NIT`).
  - `IdentificationNumber`: `string` (required, 4 to 20 alphanumeric characters).
  - `Email`: `string` (required, valid RFC 5322 email address, max 254 characters).
  - `PhoneNumber`: `string?` (optional, 7 to 15 numeric digits).
- **Validation Rules**:
  - Email format must be strictly validated.
  - Identification number must contain no special symbols except hyphens for NIT.

### 2.3 `DueDate`
- **Fields**:
  - `Value`: `DateOnly` (the deadline date in Colombia standard calendar).
  - `BusinessDaysCount`: `int` (15 or 30).
- **Validation Rules**:
  - Value must be strictly strictly in the future compared to the filing date.
  - Must equal the starting filing date + statutory business days skipping Saturdays, Sundays, and Colombian holidays per Law 51 of 1983.

---

## 3. Enums

### 3.1 `PqrsdfType`
```csharp
public enum PqrsdfType
{
    Petition = 1,     // Petición (15 business days)
    Complaint = 2,    // Queja (15 business days)
    Claim = 3,        // Reclamo (15 business days)
    Suggestion = 4,   // Sugerencia (15 business days, anonymous optional)
    Denunciation = 5, // Denuncia (30 business days, anonymous optional)
    Compliment = 6    // Felicitación (15 business days)
}
```

### 3.2 `IdentificationType`
```csharp
public enum IdentificationType
{
    CC = 1,   // Cédula de Ciudadanía
    CE = 2,   // Cédula de Extranjería
    TI = 3,   // Tarjeta de Identidad
    PA = 4,   // Pasaporte
    NIT = 5   // Número de Identificación Tributaria
}
```

### 3.3 `TicketStatus`
```csharp
public enum TicketStatus
{
    Registered = 1,   // Registrado
    Assigned = 2,     // Asignado (future)
    InReview = 3,     // En trámite (future)
    Answered = 4,     // Respondido (future)
    Closed = 5        // Cerrado (future)
}
```

---

## 4. Database Schema Mapping (EF Core)

```text
================================================================================
TABLE: PqrsdfTickets
================================================================================
Id                 UNIQUEIDENTIFIER    PRIMARY KEY
RadicadoNumber     VARCHAR(13)         NOT NULL UNIQUE (INDEX: IX_PqrsdfTickets_RadicadoNumber)
Type               INT                 NOT NULL
DestinationAreaId  UNIQUEIDENTIFIER    NOT NULL REFERENCES DestinationAreas(Id)
IsAnonymous        BIT                 NOT NULL DEFAULT 0
Applicant_FullName VARCHAR(150)        NULL
Applicant_IdType   INT                 NULL
Applicant_IdNumber VARCHAR(20)         NULL
Applicant_Email    VARCHAR(254)        NULL
Applicant_Phone    VARCHAR(20)         NULL
Subject            NVARCHAR(150)       NOT NULL
Description        NVARCHAR(4000)      NOT NULL
CreatedAtUtc       DATETIMEOFFSET      NOT NULL
DueDate            DATE                NOT NULL
BusinessDaysCount  INT                 NOT NULL
Status             INT                 NOT NULL DEFAULT 1

================================================================================
TABLE: DestinationAreas
================================================================================
Id                 UNIQUEIDENTIFIER    PRIMARY KEY
Name               NVARCHAR(100)       NOT NULL UNIQUE
Code               VARCHAR(10)         NOT NULL UNIQUE
IsActive           BIT                 NOT NULL DEFAULT 1

================================================================================
TABLE: RadicadoSequences
================================================================================
Year               INT                 PRIMARY KEY
CurrentValue       BIGINT              NOT NULL DEFAULT 0
```
