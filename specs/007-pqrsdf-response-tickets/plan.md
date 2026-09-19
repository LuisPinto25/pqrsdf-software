# Implementation Plan: PQRSDF Ticket Management and Official Response

**Branch**: `007-pqrsdf-response-tickets` | **Date**: 2026-09-19 | **Spec**: [specs/007-pqrsdf-response-tickets/spec.md](spec.md)

**Input**: Feature specification from `/specs/007-pqrsdf-response-tickets/spec.md`

---

## Summary

Implement the official ticket management and institutional response module. The backend exposes endpoints for authenticated officials (`Funcionario`) and supervisors (`Administrador`) to inspect operational ticket details (including citizen contact info and internal audit history), update operational in-progress statuses with mandatory justification (`InReview` with stage notes), and register formal final institutional responses that transition tickets to `Closed` (`Cerrado`). Case closure freezes SLA business day calculations immediately, records an immutable audit trail in `TicketStatusHistories`, and decrements the assigned official's active workload count. On the frontend, Next.js App Router Screaming Architecture is implemented with an interactive management slide-over drawer co-located under `src/app/dashboard/components/` (featuring response registration with character counters, status updates with justification, and audit history inspection). Public tracking (`/pqrsdf/search`) immediately reflects the closed status and displays the formal resolution while strictly shielding internal notes and staff data.

---

## Technical Context

**Language/Version**: 
- Backend: C# 14 / .NET 10 (ASP.NET Core 10) with `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- Frontend: TypeScript 5.7+ (strict mode enabled) with Next.js 15+ App Router.

**Primary Dependencies**:
- Backend: `MediatR` (14.x), `Microsoft.EntityFrameworkCore.SqlServer` (10.x), `Microsoft.AspNetCore.Authentication.JwtBearer` (10.x).
- Frontend: `lucide-react`, Tailwind CSS (3.x).

**Storage**: Microsoft SQL Server (single unified relational database; no secondary databases or message brokers per Constitution Principle II).

**Testing**: 
- Backend: xUnit, FluentAssertions, Moq.
- Frontend: Vitest, React Testing Library, JSDOM.

**Target Platform**: Linux / Cross-platform server environment (.NET 10 Runtime) and modern desktop/mobile web browsers.

**Project Type**: Decoupled Client-Server Web Application (Clean Architecture ASP.NET Core Web API + Next.js App Router).

**Performance Goals**:
- Response submission and case closure execution latency < 500ms.
- Status update and justification audit logging execution latency < 300ms.
- Real-time client-side character counting and validation feedback without UI stutter.

**Constraints**:
- Clean Architecture concentric layers with unidirectional dependencies (`Domain` <- `Application` <- `Infrastructure` / `Api`).
- CQRS use cases co-located under `Application/Features/Pqrsdf/UseCases/`.
- Frontend Screaming Architecture co-located in `src/app/dashboard/components/` and `src/app/dashboard/` per Constitution v1.3.0 Principle IV.
- 100% of source code, models, database schemas, and commits strictly in English.
- 100% of user-facing UI labels, table headers, validation feedback, and alerts strictly in Spanish.

**Scale/Scope**: Internal staff ticket management and citizen resolution display.

---

## Constitution Check

*GATE: Evaluated before Phase 0 research and re-verified post Phase 1 design.*

| Principle / Gate | Status | Justification / Verification |
| :--- | :---: | :--- |
| **I. Stack Tecnológico (ASP.NET Core 10 + Next.js)** | ✅ PASS | Backend implemented in ASP.NET Core 10; frontend in Next.js 15 App Router with TypeScript strict mode. |
| **II. Clean Architecture (4 capas concéntricas)** | ✅ PASS | Strict separation: `Domain` (pure POCO `TicketStatusHistory`, aggregate methods on `PqrsdfTicket`), `Application` (CQRS use cases `RespondTicket`, `ChangeTicketStatus`, `GetTicketManagementDetail`), `Infrastructure` (EF Core mapping & migration), `Api` (Controller). |
| **II. CQRS Organizativo Co-ubicado** | ✅ PASS | Commands and Queries co-located in `Application/Features/Pqrsdf/UseCases/` with dedicated Handlers and DTOs per use case. Shared DTOs placed in `Application/Shared/Dtos/`. |
| **II. Base de Datos Única (Sin Sobreingeniería)** | ✅ PASS | Single SQL Server database (`PqrsdfTickets`, `TicketStatusHistories`, `TicketAssignmentHistories`, `Users`); no dual databases or event streaming brokers. |
| **III. DDD & Tipos Fuertes** | ✅ PASS | Aggregate methods (`CloseWithResponse`, `ChangeOperationalStatus`) enforce domain invariants. Value Objects (`RadicadoNumber`, `DueDate`) and strongly typed Enums (`TicketStatus`). |
| **IV. Frontend Screaming Architecture (App Router)** | ✅ PASS | Feature components co-located under `src/app/dashboard/components/` (e.g. `ManageTicketDrawer.tsx`, `TicketStatusHistoryList.tsx`) and public resolution under `src/app/pqrsdf/search/components/`; shared tokens in `src/shared/`. |
| **V. Gobernanza de Errores (Result Pattern & Global Exception)** | ✅ PASS | All handlers return explicit `Result<T, Error>` or `Result`; translated by `ApiControllerBase.HandleResult()`; unexpected failures handled by `GlobalExceptionHandler`. |
| **VI. SOLID & Prevención de Code Smells** | ✅ PASS | Single Responsibility per Command/Query handler; DIP for repositories; no god objects or primitive obsession. |
| **VII. Convención de Idioma** | ✅ PASS | Code, classes, methods, and database schemas strictly in English; all staff-facing UI text, buttons, alerts, and feedback strictly in Spanish. |

---

## Project Structure

### Documentation (this feature)

```text
specs/007-pqrsdf-response-tickets/
├── checklists/
│   └── requirements.md                         # Spec quality checklist (16/16 passing)
├── contracts/
│   ├── change-ticket-status-contract.json      # API contract for POST /api/v1/tickets/{radicado}/status
│   ├── get-ticket-detail-contract.json         # API contract for GET /api/v1/tickets/{radicado}/management-detail
│   └── respond-ticket-contract.json            # API contract for POST /api/v1/tickets/{radicado}/response
├── data-model.md                               # Domain models, invariants, DB schemas, state transitions
├── plan.md                                     # This implementation plan
├── quickstart.md                               # End-to-end validation scenarios and test commands
├── research.md                                 # Phase 0 architectural & technical decisions
└── spec.md                                     # Feature specification with 3 clarified questions
```

### Source Code Layout

```text
backend/
├── src/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── PqrsdfTicket.cs                 # Domain methods CloseWithResponse & ChangeOperationalStatus
│   │   │   └── TicketStatusHistory.cs          # Immutable audit entity for status transitions & justifications
│   │   └── Repositories/
│   │       └── ITicketStatusHistoryRepository.cs
│   ├── Application/
│   │   └── Features/Pqrsdf/UseCases/
│   │       ├── ChangeTicketStatus/
│   │       │   ├── ChangeTicketStatusCommand.cs
│   │       │   ├── ChangeTicketStatusCommandHandler.cs
│   │       │   ├── ChangeTicketStatusRequestDto.cs
│   │       │   └── ChangeTicketStatusResponseDto.cs
│   │       ├── RespondTicket/
│   │       │   ├── RespondTicketCommand.cs
│   │       │   ├── RespondTicketCommandHandler.cs
│   │       │   ├── RespondTicketRequestDto.cs
│   │       │   └── RespondTicketResponseDto.cs
│   │       └── GetTicketManagementDetail/
│   │           ├── GetTicketManagementDetailQuery.cs
│   │           ├── GetTicketManagementDetailQueryHandler.cs
│   │           └── TicketManagementDetailDto.cs
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs         # DbSet<TicketStatusHistory>
│   │   │   ├── Configurations/
│   │   │   │   └── TicketStatusHistoryConfiguration.cs
│   │   │   └── Repositories/
│   │   │       └── TicketStatusHistoryRepository.cs
│   │   └── Migrations/                         # AddTicketStatusHistory migration
│   └── Api/
│       └── Controllers/V1/
│           └── TicketsManagementController.cs  # Status, response, and management detail endpoints
└── tests/
    ├── Pqrsdf.Domain.UnitTests/
    │   └── PqrsdfTicketTests.cs
    └── Pqrsdf.Application.UnitTests/
        ├── ChangeTicketStatusCommandHandlerTests.cs
        └── RespondTicketCommandHandlerTests.cs

frontend/
├── src/
│   ├── app/
│   │   ├── dashboard/
│   │   │   ├── components/
│   │   │   │   ├── ManageTicketDrawer.tsx       # Interactive drawer for response & status change
│   │   │   │   ├── TicketResponseForm.tsx       # Form with character counter (10-4,000 chars)
│   │   │   │   ├── TicketStatusForm.tsx         # Form with mandatory justification (10-500 chars)
│   │   │   │   ├── TicketStatusHistoryList.tsx  # Audit history list for internal staff
│   │   │   │   └── OfficialInboxTable.tsx       # Wired to open ManageTicketDrawer on "Gestionar"
│   │   │   └── types/
│   │   │       └── ticket-management.types.ts
│   │   └── pqrsdf/search/components/
│   │       └── TicketResolutionCard.tsx         # Verified citizen display for closed tickets
│   └── shared/
│       └── api/
│           └── client.ts                        # respondTicket, changeTicketStatus, getTicketManagementDetail
└── tests/
    └── components/
        └── ManageTicketDrawer.test.tsx
```

---

## Phase 0: Research Summary

All architectural unknowns were investigated and documented in [`research.md`](research.md):
1. **Separation of Concerns**: Dedicated endpoints and CQRS commands for `RespondTicket` and `ChangeTicketStatus` to maintain SRP and distinct validation boundaries.
2. **Audit Persistence**: `TicketStatusHistory` entity to preserve full chronological traceability for all status transitions and justifications.
3. **Defense-in-Depth Authorization**: Route level `[Authorize(Roles = "Funcionario,Administrador")]`, command handler ownership verification (`AssignedToUserId == CurrentUserId || Admin`), and domain aggregate invariant enforcement.
4. **SLA & Workload Lifecycle**: Automatic SLA freezing upon `Closed` status and immediate release of official workload capacity.

---

## Phase 1: Design & Contracts Summary

All design artifacts have been produced:
1. **Data Model**: Documented in [`data-model.md`](data-model.md) with aggregate root methods, EF Core schema, and state transitions.
2. **API Contracts**: Generated in [`contracts/`](contracts/):
   - [`respond-ticket-contract.json`](contracts/respond-ticket-contract.json)
   - [`change-ticket-status-contract.json`](contracts/change-ticket-status-contract.json)
   - [`get-ticket-detail-contract.json`](contracts/get-ticket-detail-contract.json)
3. **Validation Guide**: Documented in [`quickstart.md`](quickstart.md) with unit test commands and end-to-end manual flows.
