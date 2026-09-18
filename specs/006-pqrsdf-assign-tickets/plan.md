# Implementation Plan: PQRSDF Ticket Assignment to Staff

**Branch**: `006-pqrsdf-assign-tickets` | **Date**: 2026-09-18 | **Spec**: [specs/006-pqrsdf-assign-tickets/spec.md](spec.md)

**Input**: Feature specification from `/specs/006-pqrsdf-assign-tickets/spec.md`

---

## Summary

Implement the ticket assignment and staff workload management module for internal operations. The backend exposes endpoints for administrators to inspect unassigned PQRSDF requests (`Status = Registered`) ordered by statutory deadline with filters, retrieve eligible active officials with real-time workload counts, assign requests to officials (transitioning the status to `InReview` and enforcing an invariant limit of 5 active tickets per official), and reassign tickets with mandatory justification. All assignment operations record an immutable audit trail in `TicketAssignmentHistories`. On the frontend, Next.js App Router Screaming Architecture is implemented with an administrative workspace under `src/app/dashboard/assignments/` (featuring "Sin Asignar" and "En Trámite" tabs, a slide-over preview drawer, and capacity-aware assignment modals) and a dedicated workload inbox directly on the staff `/dashboard` for officials with SLA urgency badges (Red $\le 3$ days, Yellow $4-7$ days, Green $\ge 8$ days) and administrative instructions.

---

## Technical Context

**Language/Version**: 
- Backend: C# 14 / .NET 10 (ASP.NET Core 10) with `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- Frontend: TypeScript 5.7+ (strict mode enabled) with Next.js 15+ App Router.

**Primary Dependencies**:
- Backend: `MediatR` (14.x), `Microsoft.EntityFrameworkCore.SqlServer` (10.x), `Microsoft.AspNetCore.Authentication.JwtBearer` (10.x).
- Frontend: `next-intl` (3.x), `lucide-react`, Tailwind CSS (3.x).

**Storage**: Microsoft SQL Server (single unified relational database; no secondary databases or message brokers per Constitution Principle II).

**Testing**: 
- Backend: xUnit, FluentAssertions, Moq.
- Frontend: Vitest, React Testing Library, JSDOM.

**Target Platform**: Linux / Cross-platform server environment (.NET 10 Runtime) and modern desktop/mobile web browsers.

**Project Type**: Decoupled Client-Server Web Application (Clean Architecture ASP.NET Core Web API + Next.js App Router).

**Performance Goals**:
- Assignment and reassignment transaction execution latency < 500ms.
- Unassigned queue retrieval latency < 1 second with up to 50 concurrent requests.
- Instant client-side validation on justification note (>= 10 chars) and real-time workload badge computation.

**Constraints**:
- Clean Architecture concentric layers with unidirectional dependencies (`Domain` <- `Application` <- `Infrastructure` / `Api`).
- CQRS use cases co-located under `Application/Features/Pqrsdf/UseCases/`.
- Frontend Screaming Architecture co-located in `src/app/dashboard/assignments/` and `src/app/dashboard/components/`.
- 100% of source code, models, database schemas, and commits strictly in English.
- 100% of user-facing UI labels, table headers, validation feedback, and alerts strictly in Spanish.

**Scale/Scope**: Internal administrative assignment queue and staff inbox module.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Gate | Status | Justification / Verification |
| :--- | :---: | :--- |
| **I. Stack Tecnológico (ASP.NET Core 10 + Next.js)** | ✅ PASS | Backend uses ASP.NET Core 10; frontend uses Next.js 15 App Router with TypeScript in strict mode. |
| **II. Clean Architecture (4 capas concéntricas)** | ✅ PASS | Decoupled projects (`Domain`, `Application`, `Infrastructure`, `Api`). Pure POCO domain (`PqrsdfTicket`, `TicketAssignmentHistory`). DIP enforced via repositories. |
| **II. CQRS Organizativo Co-ubicado** | ✅ PASS | Use cases strictly isolated under `Application/Features/Pqrsdf/UseCases/` (`GetUnassignedTickets`, `GetAssignedTickets`, `GetAssignableOfficials`, `AssignTicket`, `ReassignTicket`, `GetOfficialInbox`) with co-located Commands/Queries, Handlers, and DTOs. |
| **II. Base de Datos Única (Sin Sobreingeniería)** | ✅ PASS | Single SQL Server database (`PqrsdfTickets`, `TicketAssignmentHistories`, `Users`); no dual databases or event brokers. |
| **III. DDD & Tipos Fuertes** | ✅ PASS | Rich domain methods (`AssignToOfficial`, `ReassignToOfficial`) encapsulating invariant enforcement (5-ticket limit, status transition to `InReview`). Strongly typed Value Objects (`RadicadoNumber`, `DueDate`) and Enums (`TicketStatus`, `AssignmentType`). |
| **IV. Frontend Screaming Architecture (App Router)** | ✅ PASS | Admin assignment views co-located in `src/app/dashboard/assignments/` and official inbox in `src/app/dashboard/components/` per Constitution v1.3.0 Principle IV; shared visual tokens in `src/shared/`. |
| **V. Gobernanza de Errores (Result Pattern & Global Exception)** | ✅ PASS | All handlers return explicit `Result<T, Error>`; API translates to HTTP 200/400/403/409 via `ApiControllerBase.HandleResult()`; unhandled exceptions caught by `GlobalExceptionHandler`. |
| **VI. SOLID & Prevención de Code Smells** | ✅ PASS | Single Responsibility per Command/Query handler; DIP for repositories and calendar services. |
| **VII. Convención de Idioma** | ✅ PASS | Code, classes, methods, and database schemas strictly in English; all staff-facing UI text, error messages, and tooltips strictly in Spanish. |

---

## Project Structure

### Documentation (this feature)

```text
specs/006-pqrsdf-assign-tickets/
├── checklists/
│   └── requirements.md                         # Spec quality checklist (16/16 passing)
├── contracts/
│   ├── assign-ticket-contract.json             # API contract for POST /api/v1/assignments/{radicado}/assign
│   ├── get-assignable-officials-contract.json  # API contract for GET /api/v1/assignments/officials
│   ├── get-assigned-tickets-contract.json      # API contract for GET /api/v1/assignments/assigned
│   ├── get-official-inbox-contract.json        # API contract for GET /api/v1/assignments/my-inbox
│   ├── get-unassigned-tickets-contract.json    # API contract for GET /api/v1/assignments/unassigned
│   └── reassign-ticket-contract.json           # API contract for POST /api/v1/assignments/{radicado}/reassign
├── data-model.md                               # Domain models, invariants, DB schemas, state transitions
├── plan.md                                     # This implementation plan
├── quickstart.md                               # End-to-end validation scenarios and test commands
├── research.md                                 # Phase 0 architectural & technical decisions
└── spec.md                                     # Feature specification with 8 clarified questions
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── PqrsdfTicket.cs                 # Extended with AssignedToUserId, AssignedAtUtc, AssignmentNote & domain methods
│   │   │   └── TicketAssignmentHistory.cs      # New immutable audit entity
│   │   ├── Enums/
│   │   │   └── AssignmentType.cs               # InitialAssignment (1), Reassignment (2)
│   │   └── Repositories/
│   │       ├── IPqrsdfTicketRepository.cs      # Updated with query methods for unassigned & assigned tickets
│   │       └── ITicketAssignmentHistoryRepository.cs # Contract for recording and querying audit logs
│   ├── Application/
│   │   └── Features/
│   │       └── Pqrsdf/
│   │           └── UseCases/
│   │               ├── GetUnassignedTickets/   # Query, Handler, Response DTOs
│   │               ├── GetAssignedTickets/     # Query, Handler, Response DTOs
│   │               ├── GetAssignableOfficials/ # Query, Handler, Response DTOs (workload counting)
│   │               ├── GetOfficialInbox/       # Query, Handler, Response DTOs (for staff dashboard)
│   │               ├── AssignTicket/           # Command, Handler, Response DTOs (validates capacity < 5)
│   │               └── ReassignTicket/         # Command, Handler, Response DTOs (mandatory justification)
│   ├── Infrastructure/
│   │   └── Persistence/
│   │       ├── Configurations/
│   │       │   ├── PqrsdfTicketConfiguration.cs        # Maps assignment columns and composite indexes
│   │       │   └── TicketAssignmentHistoryConfiguration.cs # Table configuration & foreign keys
│   │       ├── Repositories/
│   │       │   ├── PqrsdfTicketRepository.cs           # EF Core implementations
│   │       │   └── TicketAssignmentHistoryRepository.cs
│   │       └── PqrsdfDbContext.cs              # Registers DbSet<TicketAssignmentHistory>
│   └── Api/
│       └── Controllers/
│           └── V1/
│               └── AssignmentsController.cs    # REST endpoints with [Authorize(Roles = "Administrador")] & [Authorize]
frontend/
└── src/
    ├── app/
    │   └── dashboard/
    │       ├── assignments/                    # Screaming Architecture module for Admin assignment
    │       │   ├── page.tsx                    # Protected admin assignment page
    │       │   └── components/
    │       │       ├── AssignmentTabs.tsx      # Navigation: "Sin Asignar" vs "En Trámite"
    │       │       ├── UnassignedQueueTable.tsx # Table sorted by due date with filters & SLA badges
    │       │       ├── InReviewQueueTable.tsx  # Table of in-review tickets with radicado search
    │       │       ├── TicketDetailDrawer.tsx  # Slide-over preview panel with citizen full text
    │       │       ├── AssignOfficialModal.tsx # Capacity-aware official selection modal
    │       │       └── ReassignOfficialModal.tsx # Reassignment modal with justification validation
    │       └── components/
    │           ├── DashboardContent.tsx        # Integrates admin card to /assignments & official inbox
    │           └── OfficialInboxTable.tsx      # Active assigned requests table (up to 5 items)
    └── shared/
        └── components/
            ├── UrgencyBadge.tsx                # Red (<=3d), Yellow (4-7d), Green (>=8d) SLA badge
            └── WorkloadBadge.tsx               # Official capacity badge (e.g., "3/5 solicitudes")
```

---

## Complexity Tracking

> **No Constitution violations detected.** Clean Architecture, single database persistence, CQRS use case co-location, and strict English/Spanish separation fully upheld.

| Violation | Why Needed | Simpler Alternative Rejected Because |
| :--- | :--- | :--- |
| *None* | N/A | N/A |
