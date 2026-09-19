# Technical Research: PQRSDF Ticket Management and Official Response

**Feature**: `007-pqrsdf-response-tickets`  
**Date**: 2026-09-19  
**Status**: Completed  

---

## 1. Context & Architectural Objectives

The goal of this feature is to enable staff officials (`Funcionario`) and administrators (`Administrador`) to open the operational detail of an assigned ticket from their personal dashboard inbox (`/dashboard`), record operational status changes with mandatory justification, and submit formal institutional responses that transition tickets to `Closed` (`Cerrado`), stopping SLA time counters and freeing official capacity.

---

## 2. Research Decisions & Technical Architecture

### Decision 1: Dedicated Use Cases and API Endpoints for Status Change vs. Final Response

- **Decision**: Implement two explicit, decoupled CQRS Commands under `Application/Features/Pqrsdf/UseCases/`:
  1. `ChangeTicketStatus`: Command and Handler to update operational status with a mandatory justification note (10 to 500 characters).
  2. `RespondTicket`: Command and Handler to submit the formal institutional response (10 to 4,000 characters) and close the case.
  In the API layer, expose these via:
  - `POST /api/v1/tickets/{radicado}/status`
  - `POST /api/v1/tickets/{radicado}/response`
  - `GET /api/v1/tickets/{radicado}/management-detail` (returns full operational detail including applicant identity and status audit history for authenticated staff).
- **Rationale**:
  - Adheres strictly to the Single Responsibility Principle (SOLID) and Constitution Principle II (CQRS organizativo por caso de uso).
  - Isolates validation rules: status changes require concise justification (10-500 chars), whereas formal citizen responses require extensive narrative (10-4,000 chars).
  - Eliminates ambiguity in Swagger documentation and contract generation.
- **Alternatives Considered**:
  - *Single generic `PATCH /api/v1/tickets/{radicado}` endpoint*: Rejected because it encourages complex conditional branching and loose contract enforcement.

---

### Decision 2: Immutable Audit Trail via `TicketStatusHistory`

- **Decision**: Create a dedicated aggregate entity `TicketStatusHistory` in the Domain layer (`Domain/Entities/TicketStatusHistory.cs`) and map it in Infrastructure using EF Core (`TicketStatusHistories` table).
  Key attributes:
  - `Id`: Guid (PK)
  - `TicketId`: Guid (FK to `PqrsdfTickets`)
  - `PreviousStatus`: `TicketStatus`
  - `NewStatus`: `TicketStatus`
  - `ChangedByUserId`: Guid (FK to `Users`)
  - `Justification`: string (10 to 500 characters)
  - `ChangedAtUtc`: DateTime (UTC)
- **Rationale**:
  - Colombian public administration standards require chronological, tamper-proof tracking of every administrative status change.
  - Keeps `PqrsdfTicket` clean while enabling rich historical queries for internal management without performance penalties.
  - Per Clarification Q3, this history is visible strictly to authenticated internal staff (`Funcionario`, `Administrador`) and excluded from public citizen tracking.
- **Alternatives Considered**:
  - *Storing only a `LastJustification` string in `PqrsdfTicket`*: Rejected because it overwrites prior audit history and fails legal traceability requirements.

---

### Decision 3: Strict Two-Tier Authorization and Invariant Protection

- **Decision**:
  1. **API / Transport Layer**: Endpoints protected by `[Authorize(Roles = "Funcionario,Administrador")]`.
  2. **Application Layer**: Command handlers verify that `ticket.AssignedToUserId == CurrentUserId` or `CurrentUserRole == "Administrador"`. If neither holds, return `Error.Forbidden("Ticket.Forbidden", "No tiene autorización para gestionar esta solicitud.")`.
  3. **Domain Layer**: The `PqrsdfTicket` aggregate root enforces invariants:
     - Tickets already in `Closed` status reject any further status change or response submission with `Error.Conflict`.
     - `CloseWithResponse` validates response text length (10-4,000 chars) and sets `Status = TicketStatus.Closed`, `ResponseText`, and `ResponseDateUtc`.
- **Rationale**:
  - Multi-layered defense-in-depth prevents unauthorized tampering even if client-side validation or UI permissions are bypassed.
  - Domain models remain self-validating and impossible to put into an invalid state.
- **Alternatives Considered**:
  - *Role-only authorization without ticket ownership verification*: Rejected because any official could modify tickets assigned to colleagues, violating accountability.

---

### Decision 4: SLA Halting & Workload Auto-Recalculation

- **Decision**:
  - Halting SLA: `GetTicketByRadicadoQueryHandler` already suppresses remaining/overdue days calculation when `ticket.Status is (TicketStatus.Closed or TicketStatus.Answered)`. Transitioning to `Closed` natively freezes the SLA at the moment of closure.
  - Workload calculation: The existing `GetOfficialInboxQueryHandler` and `GetAssignableOfficialsQueryHandler` calculate active workload using `Status == TicketStatus.InReview`. When a ticket is closed, it naturally drops out of active workload counts, updating the official's capacity (e.g. from 5/5 to 4/5) instantly.
- **Rationale**:
  - Zero background jobs or asynchronous message queues needed; completely aligned with Constitution Principle II (Base de datos única y sin sobreingeniería).
- **Alternatives Considered**:
  - *Maintaining a denormalized `ActiveTicketsCount` column on `Users`*: Rejected to prevent concurrency write bottlenecks and race conditions.

---

### Decision 5: Frontend Screaming Architecture and Component Reuse

- **Decision**:
  - Build `ManageTicketModal.tsx` in `src/app/dashboard/components/` and link it to the "Gestionar" action in `OfficialInboxTable.tsx`.
  - Provide two tabbed/segmented actions inside the management modal:
    1. **"Registrar Respuesta Final"**: Rich textarea with character counter (10-4,000), preview, and prominent "Enviar Respuesta y Cerrar Radicado" action.
    2. **"Actualizar Estado / Justificación"**: Form with status selector (`En Trámite`), mandatory justification textarea (10-500 chars), and submission button.
    3. **"Historial de Gestión"**: Visual audit list of previous status changes with author name, date, and justification.
  - Update `OfficialInboxTable.tsx` to automatically refresh the inbox data upon successful submission and update the workload badge.
- **Rationale**:
  - Aligns with Constitution Principle IV (Screaming Architecture under `src/app/dashboard/`).
  - Provides a frictionless, high-speed UX for officials processing multiple petitions daily.
- **Alternatives Considered**:
  - *Navigating to a separate page `/dashboard/tickets/[radicado]`*: Rejected because drawer/modal keeps the official's context intact without full page reloads.
