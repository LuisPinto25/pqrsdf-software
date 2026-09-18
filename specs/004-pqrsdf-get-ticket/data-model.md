# Data Model & Schema Specification: Public PQRSDF Ticket Consultation

**Feature**: [Public PQRSDF Ticket Consultation](../spec.md)  
**Date**: 2026-09-18  
**Status**: Completed

---

## 1. Conceptual Domain & Read Models

### 1.1 Aggregate Root: `PqrsdfTicket` (Domain)

Represents the core ticket aggregate. For this feature, it supports reading operational status, timeline milestones, and final resolution.

| Property | Type | Nullable | Description |
| :--- | :--- | :---: | :--- |
| `Id` | `Guid` | No | Unique aggregate identifier. |
| `RadicadoNumber` | `RadicadoNumber` (VO) | No | Official tracking identifier in format `YYYY-NNNNNNNN`. |
| `Type` | `PqrsdfType` (Enum) | No | Category (Petition, Complaint, Claim, Suggestion, Denunciation, Compliment). |
| `DestinationAreaId` | `Guid` | No | Foreign key to `DestinationArea`. |
| `IsAnonymous` | `bool` | No | Flag indicating anonymous submission. |
| `Applicant` | `Applicant` (VO) | Yes | Citizen contact details (STRICTLY PRIVATE, NEVER EXPOSED IN PUBLIC API). |
| `Subject` | `string` | No | Summary text of the request (5-150 chars). |
| `Description` | `string` | No | Detailed narrative of the request (10-4000 chars). |
| `DueDate` | `DueDate` (VO) | No | Calculated legal response deadline (date + business days count). |
| `Status` | `TicketStatus` (Enum) | No | Current lifecycle status (`Registered`, `Assigned`, `InReview`, `Answered`, `Closed`). |
| `ResponseText` | `string?` | Yes | Official answer text provided by the institution (10-4000 chars). |
| `ResponseDateUtc` | `DateTime?` | Yes | Timestamp in UTC when the final response was registered. |
| `CreatedAtUtc` | `DateTime` | No | UTC timestamp of filing. |
| `UpdatedAtUtc` | `DateTime?` | Yes | UTC timestamp of last update. |

---

### 1.2 Public Read Model: `PublicTicketStatusDto` (Application)

The data contract specifically built for public consultation. Under zero circumstances does this DTO contain applicant personal data.

```text
PublicTicketStatusDto
├── radicadoNumber: string (e.g. "2026-00000001")
├── requestType: string (e.g. "Petition", "Denunciation")
├── destinationAreaName: string (e.g. "Atención al Ciudadano")
├── subject: string
├── description: string
├── status: string (e.g. "Registered", "Assigned", "InReview", "Answered", "Closed")
├── filingDate: string (ISO 8601 date, e.g. "2026-09-18")
├── dueDate: string (ISO 8601 date, e.g. "2026-10-09")
├── remainingBusinessDays: int? (e.g. 15; null if closed or overdue)
├── isOverdue: bool
├── overdueBusinessDays: int? (e.g. 3 when isOverdue is true; null otherwise)
├── timeline: List<TicketTimelineMilestoneDto>
│   ├── status: string
│   ├── title: string (in Spanish, e.g. "Registrado", "En trámite")
│   ├── date: string? (ISO 8601 timestamp, e.g. "2026-09-18T14:30:00Z")
│   ├── isCompleted: bool
│   └── isCurrent: bool
└── resolution: TicketResolutionDto? (null if not answered/closed)
    ├── responseText: string
    └── responseDate: string (ISO 8601 timestamp)
```

---

## 2. Database Schema Mapping (SQL Server)

The feature utilizes the existing `PqrsdfTickets` and `DestinationAreas` tables in the single unified SQL Server database.

```sql
-- Existing table with evolved nullable response columns
ALTER TABLE [PqrsdfTickets]
ADD [ResponseText] NVARCHAR(4000) NULL,
    [ResponseDateUtc] DATETIME2 NULL;
```

### Table Indexing
- `[RadicadoNumber]` has a unique clustered/non-clustered index: `IX_PqrsdfTickets_RadicadoNumber` ensuring sub-millisecond lookups:
  ```sql
  CREATE UNIQUE NONCLUSTERED INDEX [IX_PqrsdfTickets_RadicadoNumber]
  ON [PqrsdfTickets] ([RadicadoNumber]);
  ```

---

## 3. Privacy & Security Invariants

1. **Zero PII Exposure Guarantee**:
   - The query handler `GetTicketByRadicadoQueryHandler` maps fields exclusively into `PublicTicketStatusDto`.
   - Columns `Applicant_FullName`, `Applicant_IdType`, `Applicant_IdNumber`, `Applicant_Email`, and `Applicant_Phone` are never selected or projected into the public response.
2. **Rate Limiting**:
   - Fixed window: 30 requests per 1 minute per IP address.
3. **Immutability of Consultation**:
   - The public consultation is an idempotent read-only query that produces no mutations in the database.
