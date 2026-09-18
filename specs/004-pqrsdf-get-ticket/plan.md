# Implementation Plan: Public PQRSDF Ticket Consultation

**Branch**: `004-pqrsdf-get-ticket` | **Date**: 2026-09-18 | **Spec**: [specs/004-pqrsdf-get-ticket/spec.md](spec.md)

**Input**: Feature specification from `/specs/004-pqrsdf-get-ticket/spec.md`

---

## Summary

Implement the public citizen consultation module allowing unauthenticated citizens to track their PQRSDF requests using only their unique radicado tracking number (`YYYY-NNNNNNNN`). The backend exposes a read-only endpoint (`GET /api/v1/pqrsdf/{radicado}`) protected by IP-based rate limiting (30 requests/minute), returning ticket operational metadata, current lifecycle state, timeline milestones, remaining Colombian business days (or elapsed days in mora if overdue), and the official final resolution text if closed/answered, while strictly excluding all applicant personal data (0% PII exposure). The frontend is implemented at `/pqrsdf/search` adhering to Next.js App Router Screaming Architecture, with interactive search, instant input validation, and deep-linking support via URL query parameters (`?radicado=...`).

---

## Technical Context

**Language/Version**: 
- Backend: C# 14 / .NET 10 (ASP.NET Core 10) with `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- Frontend: TypeScript 5.7+ (strict mode enabled) with Next.js 15+ App Router.

**Primary Dependencies**:
- Backend: `Microsoft.EntityFrameworkCore.SqlServer` (10.x), `MediatR` (12.x), `Microsoft.AspNetCore.RateLimiting` (built-in .NET 10).
- Frontend: `next-intl` (3.x), `lucide-react`, Tailwind CSS (3.x).

**Storage**: Microsoft SQL Server (single unified relational database for reads and writes; no secondary databases or message brokers).

**Testing**: 
- Backend: xUnit, FluentAssertions, Moq, NetArchTest.
- Frontend: Vitest, React Testing Library, JSDOM.

**Target Platform**: Linux / Cross-platform server environment (.NET 10 Runtime) and modern desktop/mobile web browsers.

**Project Type**: Decoupled Client-Server Web Application (Clean Architecture ASP.NET Core Web API + Next.js App Router).

**Performance Goals**:
- Public ticket consultation latency < 2 seconds under normal network load.
- IP rate limiting enforced at 30 requests/minute per IP address returning HTTP 429 upon threshold breach.
- Instant client-side validation for radicado format (`YYYY-NNNNNNNN`).

**Constraints**:
- Absolute Zero PII exposure: Applicant Name, ID Type, ID Number, Email, and Phone must never be exposed or transmitted in public queries.
- Clean Architecture concentric layers with unidirectional dependencies (`Domain` <- `Application` <- `Infrastructure` / `Api`).
- CQRS query isolation under `Application/Features/Pqrsdf/UseCases/GetTicketByRadicado/`.
- Frontend Screaming Architecture co-located in `src/app/pqrsdf/search/` (no generic root technical folders).
- 100% of source code, models, database schemas, and commits strictly in English.
- 100% of user-facing UI labels, form fields, validation feedback, and status texts strictly in Spanish.

**Scale/Scope**: Public tracking view accessible to all citizens without authentication.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Gate | Status | Justification / Verification |
| :--- | :---: | :--- |
| **I. Stack Tecnológico (ASP.NET Core 10 + Next.js)** | ✅ PASS | Backend uses ASP.NET Core 10; frontend uses Next.js App Router with TypeScript strict mode. |
| **II. Clean Architecture (4 capas concéntricas)** | ✅ PASS | Four decoupled projects (`Domain`, `Application`, `Infrastructure`, `Api`). Pure POCO domain. Dependency Inversion enforced. |
| **II. CQRS Organizativo Co-ubicado** | ✅ PASS | Query isolated under `Application/Features/Pqrsdf/UseCases/GetTicketByRadicado/` with co-located Query, Handler, and exclusive DTOs. |
| **II. Base de Datos Única (Sin Sobreingeniería)** | ✅ PASS | Single SQL Server database for reads and writes; no dual databases, read replicas, or event brokers. |
| **III. DDD & Tipos Fuertes** | ✅ PASS | Non-anemic `PqrsdfTicket` aggregate root, strongly typed Value Objects (`RadicadoNumber`, `DueDate`), enums for lifecycle states. |
| **IV. Frontend Screaming Architecture (App Router)** | ✅ PASS | Consultation feature co-located under `src/app/pqrsdf/search/` per Constitution v1.3.0 Principle IV. Shared utilities in `src/shared/`. |
| **V. Gobernanza de Errores (Result Pattern & Global Exception)** | ✅ PASS | Query returns explicit `Result<PublicTicketStatusDto, Error>`; API translates to HTTP 200/400/404 via `ApiControllerBase.HandleResult()`; exceptions intercepted by `GlobalExceptionHandler`. |
| **VI. SOLID & Prevención de Code Smells** | ✅ PASS | Query Handler has a single responsibility; business day math reused cleanly via `DueDateCalculator` and `IColombianHolidayService`. |
| **VII. Convención de Idioma** | ✅ PASS | Code, classes, and database schemas in English; all citizen-facing messages, timeline titles, and validation feedback in Spanish. |

---

## Project Structure

### Documentation (this feature)

```text
specs/004-pqrsdf-get-ticket/
├── checklists/
│   └── requirements.md                # Spec quality checklist (16/16 passing)
├── contracts/
│   └── get-ticket-contract.json       # OpenAPI / JSON Schema for GET /api/v1/pqrsdf/{radicado}
├── data-model.md                      # Conceptual Read Model, Invariants, DB Schema mappings
├── plan.md                            # This implementation plan
├── quickstart.md                      # Verification and quickstart guide
├── research.md                        # Technical decisions (CQRS, SLA math, rate limiting, routing)
└── spec.md                            # Feature specification with 5 clarified questions
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   └── PqrsdfTicket.cs                   # Evolved with ResponseText, ResponseDateUtc, CloseWithResponse
│   │   ├── Repositories/
│   │   │   └── IPqrsdfTicketRepository.cs        # Already contains GetByRadicadoAsync
│   │   └── Services/
│   │       └── DueDateCalculator.cs              # Extended with CalculateRemainingBusinessDays & CalculateOverdueBusinessDays
│   ├── Application/
│   │   └── Features/
│   │       └── Pqrsdf/
│   │           └── UseCases/
│   │               └── GetTicketByRadicado/
│   │                   ├── GetTicketByRadicadoQuery.cs
│   │                   ├── GetTicketByRadicadoQueryHandler.cs
│   │                   ├── PublicTicketStatusDto.cs
│   │                   ├── TicketTimelineMilestoneDto.cs
│   │                   └── TicketResolutionDto.cs
│   ├── Infrastructure/
│   │   └── Persistence/
│   │       └── Configurations/
│   │           └── PqrsdfTicketConfiguration.cs  # Added mappings for ResponseText and ResponseDateUtc
│   └── Api/
│       ├── Controllers/
│       │   └── V1/
│       │       └── PqrsdfController.cs           # Added [HttpGet("{radicado}")] with Rate Limiting
│       └── Program.cs                            # Configured AddRateLimiter with IP-based FixedWindowLimiter
└── tests/
    ├── Application.UnitTests/
    │   └── Features/
    │       └── Pqrsdf/
    │           └── UseCases/
    │               └── GetTicketByRadicadoQueryHandlerTests.cs
    └── Domain.UnitTests/
        └── Services/
            └── DueDateCalculatorRemainingDaysTests.cs

frontend/
├── src/
│   ├── app/
│   │   └── pqrsdf/
│   │       ├── search/
│   │       │   ├── page.tsx                      # Public consultation page (/pqrsdf/search)
│   │       │   ├── components/
│   │       │   │   ├── TicketSearchBox.tsx       # Radicado input with regex validation
│   │       │   │   ├── TicketStatusHeader.tsx    # Summary header, status/overdue badges
│   │       │   │   ├── TicketTimeline.tsx        # Milestone timeline (Registrado -> Cerrado)
│   │       │   │   ├── TicketDetailCard.tsx      # Subject and Description display
│   │       │   │   └── TicketResolutionCard.tsx  # Final response card
│   │       │   └── hooks/
│   │       │       └── useTicketSearch.ts        # Search state, query logic, URLParams sync
│   └── shared/
│       └── api/
│           └── client.ts                         # Added getTicketByRadicado(radicado)
└── tests/
    └── app/
        └── pqrsdf/
            └── search/
                └── TicketSearch.test.tsx         # Component and interaction tests
```

---

## Phase 0: Outline & Research

- Completed in [research.md](research.md).
- Key technical decisions consolidated:
  1. CQRS query pattern under `Application/Features/Pqrsdf/UseCases/GetTicketByRadicado/`.
  2. Domain aggregate evolution for official final response storage.
  3. Exact Colombian business days calculation for remaining and overdue days via `DueDateCalculator`.
  4. Native ASP.NET Core 10 Rate Limiting (30 requests/minute/IP).
  5. Next.js App Router Screaming Architecture at `/pqrsdf/search` with URL search param synchronization.

---

## Phase 1: Design & Contracts

- **Data Model**: Completed in [data-model.md](data-model.md).
- **Interface Contracts**: Completed in [contracts/get-ticket-contract.json](contracts/get-ticket-contract.json).
- **Validation Guide**: Completed in [quickstart.md](quickstart.md).
- **Post-Design Constitution Check**: All 9 gates verified and passing. Ready for Phase 2 task decomposition (`/speckit-tasks`).
