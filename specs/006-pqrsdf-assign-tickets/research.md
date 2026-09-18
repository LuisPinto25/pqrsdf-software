# Phase 0: Research & Architectural Decisions

**Feature**: PQRSDF Ticket Assignment to Staff (`006-pqrsdf-assign-tickets`)  
**Status**: Completed  
**Date**: 2026-09-18  

---

## 1. CQRS Command and Query Separation for Ticket Assignment

### Decision
Structure all assignment business cases in the Application layer strictly separated into Commands (mutations) and Queries (read-only operations) under `Application/Features/Pqrsdf/UseCases/`, in strict alignment with Constitution Principle II:
- `GetUnassignedTickets/` (`GetUnassignedTicketsQuery`, Handler, and response DTOs)
- `GetAssignedTickets/` (`GetAssignedTicketsQuery`, Handler, and response DTOs for Admin "En Trámite" tab)
- `GetAssignableOfficials/` (`GetAssignableOfficialsQuery`, Handler, and response DTOs with workload counts)
- `GetOfficialInbox/` (`GetOfficialInboxQuery`, Handler, and response DTOs for the Funcionario's `/dashboard`)
- `AssignTicket/` (`AssignTicketCommand`, Handler, and response DTOs)
- `ReassignTicket/` (`ReassignTicketCommand`, Handler, and response DTOs)

### Rationale
- Comports strictly with Constitution Principle II (CQRS strictly organizational, co-located within Application layer, without dual databases or event brokers).
- Keeps each use case isolated, easy to test with isolated unit tests, and prevents "God Handlers".
- Shared DTOs (such as `RadicadoDto` or shared summary representations) reside in `Application/Shared/Dtos/`, while exclusive DTOs remain co-located.

### Alternatives Considered
- *Single Controller / Service with multiple methods*: Violates Single Responsibility Principle and CQRS organizational mandates established in Constitution Principle II.
- *Async message broker for assignment*: Expressly forbidden by Constitution Principle II ("Queda terminantemente prohibido incorporar arquitecturas con bases de datos separadas o buses de eventos").

---

## 2. Workload Limit Enforcement (Max 5 Active Tickets) & Concurrency Protection

### Decision
Enforce the invariant that no official can have more than 5 concurrently active assigned tickets in state `InReview` through a double layer of defense:
1. **Application & Domain Layer Invariant**: The domain entity `PqrsdfTicket.Assign(...)` and `PqrsdfTicket.Reassign(...)` methods require passing the current active count of the target official, validating `currentActiveCount < 5`.
2. **Concurrency & Race Condition Control**:
   - The repository query to count active tickets executes with transaction-level consistency (`RepeatableRead` / row locking or an atomic check in the assignment transaction).
   - In EF Core, if two administrators assign different tickets to the same official whose workload is at 4/5 at the exact same instant, the transaction verifies the count within an atomic database transaction. If the count reaches 5, the second transaction is aborted with a domain failure: `Error.Conflict("Official.WorkloadLimitExceeded", "El funcionario seleccionado ha alcanzado el límite máximo de 5 solicitudes asignadas activas.")`.

### Rationale
- Prevents staff overload and preserves statutory quality of response.
- Guarantees that business invariants are never violated even under concurrent operations.
- Avoids primitive obsession by representing assignment status transitions inside domain aggregate methods.

### Alternatives Considered
- *UI-only validation*: Vulnerable to race conditions and direct API bypassing. Rejected because domain integrity must be enforced at the server/database boundary.
- *Distributed Redis Locks*: Over-engineering violating Constitution Principle II (single unified database). A standard relational database transaction handles concurrency with zero extra infrastructure.

---

## 3. Audit Trail Architecture (`TicketAssignmentHistory`)

### Decision
Create an explicit, immutable domain entity `TicketAssignmentHistory` persisted in the unified database (`PqrsdfDbContext`), recording:
- `Id` (Guid, PK)
- `TicketId` (Guid, FK to `PqrsdfTickets`)
- `PreviousAssignedUserId` (Guid?, nullable; null for initial assignment)
- `NewAssignedUserId` (Guid, FK to `Users`)
- `AssignedByUserId` (Guid, FK to `Users`, the Administrator executing the change)
- `AssignedAtUtc` (DateTime in UTC)
- `JustificationNote` (string?, mandatory >= 10 characters on reassignment, optional up to 500 characters on initial assignment)
- `AssignmentType` (enum: `InitialAssignment = 1`, `Reassignment = 2`)

### Rationale
- Complete legal compliance and statutory accountability for public sector workflows in Colombia.
- Enables auditing who assigned which ticket, to whom, and why.
- Separates current ticket state (`PqrsdfTicket.AssignedToUserId`) from complete historical movements.

### Alternatives Considered
- *JSON column in PqrsdfTicket table*: Harder to index, query for reports, and violates strong typing in EF Core.
- *Separate audit microservice*: Explicitly prohibited by the single-database rule in the Constitution.

---

## 4. Colombian Business Days SLA Calculation & Urgency Badges

### Decision
Reuse the existing `IColombianCalendarService` (established in Feature 003) to compute the remaining Colombian business days from `DateTime.UtcNow` to `PqrsdfTicket.DueDate`.
Map the calculated remaining business days to the 3 agreed visual tiers:
- **Red (Crítico / Vencido)**: Remaining Colombian business days $\le 3$ (or overdue if $\le 0$).
- **Yellow (Atención)**: Remaining Colombian business days between $4$ and $7$.
- **Green (A tiempo)**: Remaining Colombian business days $\ge 8$.

### Rationale
- Guarantees 100% statutory alignment with Law 1755 of 2015 and Law 51 of 1983 (Ley Emiliani).
- Consistent visual indicators across both the Administrator's queue and the Official's inbox.
- Calculated dynamically on retrieval so deadlines advance without cron batch jobs mutating rows daily.

### Alternatives Considered
- *Storing remaining days as a column*: Stales every 24 hours and requires night-time scheduled jobs. Dynamic calculation based on UTC today and stored `DueDate` is cleaner, stateless, and always exact.

---

## 5. Frontend Screaming Architecture in Next.js App Router

### Decision
Implement the UI following Constitution Principle IV (Frontend Screaming Architecture y Next.js App Router Feature-Driven):
- **Admin Assignment Workspace**: Located at `src/app/dashboard/assignments/page.tsx`:
  - Co-located components under `src/app/dashboard/assignments/components/`:
    - `AssignmentTabs.tsx`: Navigation between "Sin Asignar" and "En Trámite" tabs.
    - `UnassignedQueueTable.tsx`: Table sorted by nearest legal due date with filters (Type, Area) and urgency badges.
    - `InReviewQueueTable.tsx`: Table of assigned tickets with radicado search.
    - `TicketDetailDrawer.tsx`: Slide-over panel rendering full citizen description and assignment form.
    - `AssignOfficialModal.tsx` / `ReassignOfficialModal.tsx`: Staff selector with workload ratios (e.g. `3/5`) and validation.
- **Funcionario Workload Inbox**: Co-located in `src/app/dashboard/components/OfficialInboxTable.tsx`:
  - Directly rendered when `user.role === 'Funcionario'` on `/dashboard`.
  - Displays up to 5 active assigned requests (`InReview`) with urgency badge, admin instruction notes, and access to view/draft response.
- **Shared Transversal Components**: `src/shared/components/` (e.g. `UrgencyBadge.tsx`, `WorkloadBadge.tsx`, drawer shell).

### Rationale
- Fulfills Constitution Principle IV: no generic root technical folders, domain screams from directory structure, and Next.js nested layouts and server/client boundaries are respected.
- All user-facing text, error messages, and form labels strictly rendered in Spanish per Constitution Principle VII.
