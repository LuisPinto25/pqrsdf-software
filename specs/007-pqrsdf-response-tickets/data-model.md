# Data Model: PQRSDF Ticket Management and Official Response

**Feature**: `007-pqrsdf-response-tickets`  
**Date**: 2026-09-19  
**Status**: Completed  

---

## 1. Domain Entities & Value Objects

### Aggregate Root: `PqrsdfTicket`

Aggregate root encapsulating citizen request state, assignment details, and official institutional resolution.

| Field | Type | Nullable | Description & Domain Rules |
|-------|------|:--------:|----------------------------|
| `Id` | `Guid` | No | Unique identifier (Primary Key). |
| `RadicadoNumber` | `RadicadoNumber` (VO) | No | Official filing number (format: `PQRSDF-YYYY-NNNNNN`). |
| `Type` | `PqrsdfType` (Enum) | No | Petition (1), Complaint (2), Claim (3), Suggestion (4), Denunciation (5), Compliment (6). |
| `DestinationAreaId` | `Guid` | No | Foreign Key to `DestinationAreas`. |
| `IsAnonymous` | `bool` | No | Indicates anonymous submission. Only allowed for Suggestion & Denunciation. |
| `Applicant` | `Applicant` (VO) | Yes | Contains DocumentType, DocumentNumber, FullName, Email, Phone. Mandatory if `!IsAnonymous`. |
| `Subject` | `string` | No | Summary subject (5-150 characters). |
| `Description` | `string` | No | Full citizen statement (10-4,000 characters). |
| `DueDate` | `DueDate` (VO) | No | Legal deadline calculated by statutory business days. |
| `Status` | `TicketStatus` (Enum) | No | Registered (1), Assigned (2), InReview (3), Answered (4), Closed (5). |
| `ResponseText` | `string` | Yes | Institutional resolution text (10-4,000 characters). Set on closure. |
| `ResponseDateUtc` | `DateTime` | Yes | Timestamp of formal closure and response emission. |
| `AssignedToUserId` | `Guid` | Yes | Foreign Key to `Users`. Official responsible for answering the ticket. |
| `AssignedAtUtc` | `DateTime` | Yes | Timestamp when the ticket was assigned to the official. |
| `AssignmentNote` | `string` | Yes | Administrative instruction from the assigning Administrator (up to 500 chars). |
| `CreatedAtUtc` | `DateTime` | No | Timestamp of filing creation in UTC. |
| `UpdatedAtUtc` | `DateTime` | Yes | Timestamp of last modification in UTC. |

#### Domain Invariant Methods on `PqrsdfTicket`

```csharp
// Closes ticket with citizen response, validates text length, and sets Status to Closed
public Result CloseWithResponse(string? responseText, DateTime responseDateUtc)
{
    if (string.IsNullOrWhiteSpace(responseText) || responseText.Trim().Length < 10 || responseText.Trim().Length > 4000)
    {
        return Result.Failure(Error.Validation(
            "PqrsdfTicket.InvalidResponseText",
            "El texto de la respuesta institucional debe tener entre 10 y 4000 caracteres."));
    }

    if (Status == TicketStatus.Closed)
    {
        return Result.Failure(Error.Conflict(
            "PqrsdfTicket.AlreadyClosed",
            "La solicitud ya se encuentra en estado cerrado."));
    }

    ResponseText = responseText.Trim();
    ResponseDateUtc = responseDateUtc;
    Status = TicketStatus.Closed;
    UpdatedAtUtc = responseDateUtc;

    return Result.Success();
}

// Updates operational in-progress status (cannot alter closed tickets)
public Result ChangeOperationalStatus(TicketStatus newStatus, DateTime utcNow)
{
    if (Status == TicketStatus.Closed)
    {
        return Result.Failure(Error.Conflict(
            "PqrsdfTicket.AlreadyClosed",
            "No se puede cambiar el estado de una solicitud cerrada."));
    }

    Status = newStatus;
    UpdatedAtUtc = utcNow;

    return Result.Success();
}
```

---

### Audit Entity: `TicketStatusHistory`

Immutable chronological log tracking every status transition and administrative justification.

| Field | Type | Nullable | Description & Domain Rules |
|-------|------|:--------:|----------------------------|
| `Id` | `Guid` | No | Unique identifier (Primary Key). |
| `TicketId` | `Guid` | No | Foreign Key to `PqrsdfTickets`. Non-cascading. |
| `PreviousStatus` | `TicketStatus` (Enum) | No | Status of the ticket prior to the update. |
| `NewStatus` | `TicketStatus` (Enum) | No | Target status applied. |
| `ChangedByUserId` | `Guid` | No | Foreign Key to `Users`. User who executed the action. |
| `Justification` | `string` | No | Mandatory justification note (10-500 characters). |
| `ChangedAtUtc` | `DateTime` | No | Exact UTC timestamp of the status change. |
| `CreatedAtUtc` | `DateTime` | No | Record creation timestamp in UTC. |

#### Navigation Properties

- `Ticket`: `PqrsdfTicket`
- `ChangedByUser`: `User`

---

## 2. State Transition Diagram

```mermaid
stateDiagram-v2
    [*] --> Registered: Citizen submits filing

    Registered --> InReview: Admin assigns to Official
    
    state InReview {
        [*] --> Analyzing
        Analyzing --> InReview: Official updates status (Mandatory Justification)
    }

    InReview --> Closed: Official submits Final Response (10-4000 chars)
    
    Closed --> [*]: SLA calculation freezes permanently & Workload capacity freed
```

---

## 3. Database Schema (EF Core Configuration)

### Table: `TicketStatusHistories`

```sql
CREATE TABLE [TicketStatusHistories] (
    [Id] UNIQUEIDENTIFIER NOT NULL,
    [TicketId] UNIQUEIDENTIFIER NOT NULL,
    [PreviousStatus] INT NOT NULL,
    [NewStatus] INT NOT NULL,
    [ChangedByUserId] UNIQUEIDENTIFIER NOT NULL,
    [Justification] NVARCHAR(500) NOT NULL,
    [ChangedAtUtc] DATETIME2 NOT NULL,
    [CreatedAtUtc] DATETIME2 NOT NULL,
    CONSTRAINT [PK_TicketStatusHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TicketStatusHistories_PqrsdfTickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [PqrsdfTickets] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TicketStatusHistories_Users_ChangedByUserId] FOREIGN KEY ([ChangedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_TicketStatusHistories_TicketId_ChangedAtUtc] 
ON [TicketStatusHistories] ([TicketId], [ChangedAtUtc] DESC);
```
