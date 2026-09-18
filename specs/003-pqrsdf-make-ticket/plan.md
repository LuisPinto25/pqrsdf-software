# Implementation Plan: Public PQRSDF Ticket Registration

**Branch**: `003-pqrsdf-make-ticket` | **Date**: 2026-09-18 | **Spec**: [specs/003-pqrsdf-make-ticket/spec.md](spec.md)

**Input**: Feature specification from `/specs/003-pqrsdf-make-ticket/spec.md`

---

## Summary

Implement the public citizen filing module allowing unauthenticated citizens to register PQRSDF tickets (Petición, Queja, Reclamo, Sugerencia, Denuncia, Felicitación) through a public web interface. The backend calculates statutory deadlines in Colombian business days (15 days standard, 30 days for denuncias) excluding weekends and holidays per Law 51 of 1983 (Ley Emiliani) and guarantees unique sequential radicado numbers (`YYYY-NNNNNNNN`) with annual resets under high concurrency. The frontend is built under `src/app/pqrsdf/` adhering to Next.js App Router Screaming Architecture, with dynamic destination area retrieval, plain text inputs with character counters, anonymous filing support exclusively for Denuncias and Sugerencias, and an immediate on-screen confirmation receipt.

---

## Technical Context

**Language/Version**: 
- Backend: C# 14 / .NET 10 (ASP.NET Core 10) with `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- Frontend: TypeScript 5.7+ (strict mode enabled) with Next.js 15+ App Router.

**Primary Dependencies**:
- Backend: `Microsoft.EntityFrameworkCore.SqlServer` (10.x), `MediatR` (12.x), `FluentValidation` (11.x).
- Frontend: `next-intl` (3.x), `react-hook-form` (7.x), `@hookform/resolvers`, `zod` (3.x), `lucide-react`, Tailwind CSS (3.x).

**Storage**: Microsoft SQL Server (single unified relational database for reads and writes; no secondary databases or message brokers).

**Testing**: 
- Backend: xUnit, FluentAssertions, Moq, NetArchTest.
- Frontend: Vitest, React Testing Library, JSDOM.

**Target Platform**: Linux / Cross-platform server environment (.NET 10 Runtime) and modern desktop/mobile web browsers.

**Project Type**: Decoupled Client-Server Web Application (Clean Architecture ASP.NET Core Web API + Next.js App Router).

**Performance Goals**:
- Public ticket filing submission and radicado generation latency < 500ms under normal load.
- Immediate UI button lock to prevent duplicate clicks upon submission.
- Zero duplicate radicado numbers generated under 50 simultaneous concurrent submissions.

**Constraints**:
- Clean Architecture concentric layers with unidirectional dependencies (`Domain` <- `Application` <- `Infrastructure` / `Api`).
- CQRS use case grouping under `Application/Features/Pqrsdf/UseCases/[UseCaseName]/`.
- Frontend Screaming Architecture co-located in `src/app/pqrsdf/` (no generic root technical folders).
- 100% of source code, models, database schemas, and commits strictly in English.
- 100% of user-facing UI labels, form fields, validation feedback, and confirmation screens strictly in Spanish.

**Scale/Scope**: Core citizen-facing entry point of the PQRSDF software.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Gate | Status | Justification / Verification |
| :--- | :---: | :--- |
| **I. Stack Tecnológico (ASP.NET Core 10 + Next.js)** | ✅ PASS | Backend uses ASP.NET Core 10; frontend uses Next.js App Router with TypeScript strict mode. |
| **II. Clean Architecture (4 capas concéntricas)** | ✅ PASS | Four decoupled projects (`Domain`, `Application`, `Infrastructure`, `Api`). Pure POCO domain. Dependency Inversion enforced. |
| **II. CQRS Organizativo Co-ubicado** | ✅ PASS | Use cases organized strictly under `Application/Features/Pqrsdf/UseCases/MakePqrsdf/` and `GetActiveDestinationAreas/` with co-located Command/Query, Handler, and DTOs. |
| **II. Base de Datos Única (Sin Sobreingeniería)** | ✅ PASS | Single SQL Server database for reads and writes; no dual databases, read replicas, or event brokers. |
| **III. DDD & Tipos Fuertes** | ✅ PASS | Non-anemic `PqrsdfTicket` aggregate root, strongly typed Value Objects (`RadicadoNumber`, `Applicant`, `DueDate` as C# `record`), enums for categories and statuses. |
| **IV. Frontend Screaming Architecture (App Router)** | ✅ PASS | All filing components, hooks, and views co-located in `src/app/pqrsdf/` per Constitution v1.3.0 Principle IV. Shared utilities in `src/shared/`. |
| **V. Gobernanza de Errores (Result Pattern & Global Exception)** | ✅ PASS | Use cases return explicit `Result<T, Error>`; API translates to HTTP 201/400/422 via `ApiControllerBase.HandleResult()`; exceptions intercepted by `GlobalExceptionHandler`. |
| **VI. SOLID & Prevención de Code Smells** | ✅ PASS | Handlers have single responsibility; algorithmic holiday calculation cleanly segregated in `ColombianHolidayService`. |
| **VII. Convención de Idioma** | ✅ PASS | Code, classes, and database schemas in English; all citizen-facing messages and validation feedback in Spanish. |

---

## Project Structure

### Documentation (this feature)

```text
specs/003-pqrsdf-make-ticket/
├── checklists/
│   └── requirements.md                # Spec quality checklist
├── contracts/
│   ├── make-pqrsdf-contract.json      # OpenAPI / JSON Schema for POST /api/v1/pqrsdf
│   └── destination-areas-contract.json# OpenAPI / JSON Schema for GET /api/v1/pqrsdf/areas
├── data-model.md                      # Entities, Value Objects, DB Schema mappings
├── plan.md                            # This implementation plan
├── quickstart.md                      # Verification and quickstart guide
├── research.md                        # Technical decisions (holiday algorithm, radicado counter)
└── spec.md                            # Feature specification
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── PqrsdfTicket.cs
│   │   │   └── DestinationArea.cs
│   │   ├── Enums/
│   │   │   ├── PqrsdfType.cs
│   │   │   ├── IdentificationType.cs
│   │   │   └── TicketStatus.cs
│   │   ├── ValueObjects/
│   │   │   ├── RadicadoNumber.cs
│   │   │   ├── Applicant.cs
│   │   │   └── DueDate.cs
│   │   ├── Services/
│   │   │   ├── IColombianHolidayService.cs
│   │   │   └── IRadicadoSequenceGenerator.cs
│   │   └── Repositories/
│   │       ├── IPqrsdfTicketRepository.cs
│   │       └── IDestinationAreaRepository.cs
│   ├── Application/
│   │   ├── Features/
│   │   │   └── Pqrsdf/
│   │   │       └── UseCases/
│   │   │           ├── MakePqrsdf/
│   │   │           │   ├── MakePqrsdfCommand.cs
│   │   │           │   ├── MakePqrsdfCommandHandler.cs
│   │   │           │   ├── MakePqrsdfRequest.cs
│   │   │           │   ├── MakePqrsdfResponse.cs
│   │   │           │   └── MakePqrsdfCommandValidator.cs
│   │   │           └── GetActiveDestinationAreas/
│   │   │               ├── GetActiveDestinationAreasQuery.cs
│   │   │               ├── GetActiveDestinationAreasQueryHandler.cs
│   │   │               └── DestinationAreaDto.cs
│   │   └── Shared/
│   │       └── Dtos/
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Configurations/
│   │   │   │   ├── PqrsdfTicketConfiguration.cs
│   │   │   │   ├── DestinationAreaConfiguration.cs
│   │   │   │   └── RadicadoSequenceConfiguration.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── PqrsdfTicketRepository.cs
│   │   │   │   └── DestinationAreaRepository.cs
│   │   │   └── Services/
│   │   │       ├── RadicadoSequenceGenerator.cs
│   │   │       └── PqrsdfDbContextSeed.cs
│   │   └── Services/
│   │       └── ColombianHolidayService.cs
│   └── Api/
│       └── Controllers/
│           └── V1/
│               └── PqrsdfController.cs
└── tests/
    ├── Application.UnitTests/
    │   ├── Features/
    │   │   └── Pqrsdf/
    │   │       ├── MakePqrsdfCommandHandlerTests.cs
    │   │       └── GetActiveDestinationAreasQueryHandlerTests.cs
    │   └── Domain/
    │       ├── RadicadoNumberTests.cs
    │       ├── ColombianHolidayServiceTests.cs
    │       └── DueDateCalculationTests.cs
    └── Architecture.Tests/
        └── PqrsdfArchitectureTests.cs

frontend/
├── messages/
│   └── es.json                        # Spanish messages catalog (form labels, errors, confirmation)
├── src/
│   ├── app/
│   │   └── pqrsdf/                    # Screaming Architecture feature root
│   │       ├── components/
│   │       │   ├── PqrsdfForm.tsx     # Citizen public filing form
│   │       │   ├── ConfirmationReceipt.tsx # Success confirmation view with clipboard action
│   │       │   └── CharacterCounter.tsx # Visual countdown indicator for plain text fields
│   │       ├── hooks/
│   │       │   ├── usePqrsdfForm.ts   # Form submission & validation hook
│   │       │   └── useDestinationAreas.ts # Dynamic area retrieval hook
│   │       ├── types/
│   │       │   └── pqrsdf.ts          # Feature-specific client types
│   │       └── page.tsx               # Public page orchestrating form and confirmation view
│   └── shared/
│       └── api/
│           └── generated/
│               └── schema.d.ts        # Updated OpenAPI types
└── tests/
    └── pqrsdf/
        ├── PqrsdfForm.test.tsx        # Component form rendering & validation tests
        └── ConfirmationReceipt.test.tsx # Confirmation receipt view tests
```

**Structure Decision**: Monorepo layout containing `backend/` and `frontend/`. Backend strictly implements Clean Architecture with co-located use cases in `Application/Features/Pqrsdf/UseCases/`. Frontend implements Screaming Architecture with all feature presentation, logic, and tests co-located under `src/app/pqrsdf/` and `frontend/tests/pqrsdf/`.

---

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
| :--- | :--- | :--- |
| *None* | Architecture strictly complies with all 7 Constitutional principles. | No violations or deviations introduced. |
