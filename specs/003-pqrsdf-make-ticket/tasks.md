# Tasks: Public PQRSDF Ticket Registration (003-pqrsdf-make-ticket)

**Input**: Design documents from `specs/003-pqrsdf-make-ticket/` (`spec.md`, `plan.md`, `data-model.md`, `research.md`, `contracts/`, `quickstart.md`)  
**Prerequisites**: `plan.md` (complete), `spec.md` (complete), `data-model.md` (complete), `contracts/` (complete), `quickstart.md` (complete)  
**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no blocking dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`, `US4`)
- Includes exact file paths in all descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, dependency installation, and message catalogs

- [X] T001 Verify backend solution build and frontend package scripts in `backend/Pqrsdf.slnx` and `frontend/package.json`
- [X] T002 [P] Install `react-hook-form` and `@hookform/resolvers` `zod` in `frontend/package.json` for form validation
- [X] T003 [P] Add Spanish message keys for PQRSDF public filing and confirmation in `frontend/messages/es.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain primitives, enums, value objects, and persistence configurations that block user story implementation

- [X] T004 Create `PqrsdfType`, `IdentificationType`, and `TicketStatus` enums in `backend/src/Domain/Enums/PqrsdfEnums.cs`
- [X] T005 [P] Create `RadicadoNumber` value object validating `YYYY-NNNNNNNN` format in `backend/src/Domain/ValueObjects/RadicadoNumber.cs`
- [X] T006 [P] Create `Applicant` value object with document and RFC 5322 email constraints in `backend/src/Domain/ValueObjects/Applicant.cs`
- [X] T007 [P] Create `DueDate` value object encapsulating legal deadlines in `backend/src/Domain/ValueObjects/DueDate.cs`
- [X] T008 [P] Create `DestinationArea` entity and `IDestinationAreaRepository` in `backend/src/Domain/Entities/DestinationArea.cs` and `backend/src/Domain/Repositories/IDestinationAreaRepository.cs`
- [X] T009 [P] Create `IRadicadoSequenceGenerator` domain interface in `backend/src/Domain/Services/IRadicadoSequenceGenerator.cs`
- [X] T010 [P] Create `IColombianHolidayService` domain interface in `backend/src/Domain/Services/IColombianHolidayService.cs`
- [X] T011 Create `RadicadoSequence` entity and atomic SQL Server sequence generator in `backend/src/Infrastructure/Services/RadicadoSequenceGenerator.cs`
- [X] T012 [P] Create EF Core entity configurations in `backend/src/Infrastructure/Persistence/Configurations/DestinationAreaConfiguration.cs` and `RadicadoSequenceConfiguration.cs`
- [X] T013 Register new entities, initial area seed data, and configurations in `backend/src/Infrastructure/Persistence/PqrsdfDbContext.cs`

**Checkpoint**: Core domain primitives and database infrastructure ready for user stories.

---

## Phase 3: User Story 4 - Statutory Business Days Due Date Calculation (Priority: P2)

**Goal**: Accurately calculate statutory response deadlines in official Colombian business days (15 days standard, 30 days for denuncias) excluding weekends and statutory holidays per Law 51 of 1983 (Ley Emiliani).

**Independent Test**: Can be tested independently by running unit tests asserting exact business day deadlines forward from any calendar date, including holiday weekends, Holy Week, and end-of-year rollovers.

### Tests for User Story 4
- [X] T014 [P] [US4] Unit tests for Colombian holiday calculation (Butcher's algorithm and Ley Emiliani shifts) in `backend/tests/Application.UnitTests/Domain/ColombianHolidayServiceTests.cs`
- [X] T015 [P] [US4] Unit tests for business day due date calculation (15 vs 30 days) in `backend/tests/Application.UnitTests/Domain/DueDateCalculationTests.cs`

### Implementation for User Story 4
- [X] T016 [US4] Implement `ColombianHolidayService` with Butcher's algorithm and Ley Emiliani Monday shifts in `backend/src/Infrastructure/Services/ColombianHolidayService.cs`
- [X] T017 [US4] Implement `DueDateCalculator` domain service orchestrating business days calculation in `backend/src/Domain/Services/DueDateCalculator.cs`

**Checkpoint**: Due date calculation engine complete and tested independently.

---

## Phase 4: User Story 2 - Dynamic Destination Area and Request Type Selection (Priority: P1)

**Goal**: Allow unauthenticated citizens to view the six official PQRSDF types and load active destination administrative areas dynamically from the backend catalog.

**Independent Test**: Can be tested by calling `GET /api/v1/pqrsdf/areas` and asserting active areas returned, and rendering the frontend form selector.

### Tests for User Story 2
- [X] T018 [P] [US2] Unit tests for `GetActiveDestinationAreasQueryHandler` in `backend/tests/Application.UnitTests/Features/Pqrsdf/GetActiveDestinationAreasQueryHandlerTests.cs`
- [X] T019 [P] [US2] Hook test for `useDestinationAreas` in `frontend/tests/pqrsdf/useDestinationAreas.test.ts`

### Implementation for User Story 2
- [X] T020 [US2] Implement `GetActiveDestinationAreasQuery`, `GetActiveDestinationAreasQueryHandler`, and `DestinationAreaDto` in `backend/src/Application/Features/Pqrsdf/UseCases/GetActiveDestinationAreas/`
- [X] T021 [US2] Implement `DestinationAreaRepository` in `backend/src/Infrastructure/Persistence/Repositories/DestinationAreaRepository.cs`
- [X] T022 [US2] Expose `GET /api/v1/pqrsdf/areas` in `backend/src/Api/Controllers/V1/PqrsdfController.cs`
- [X] T023 [P] [US2] Implement `useDestinationAreas` hook with typed API client in `frontend/src/app/pqrsdf/hooks/useDestinationAreas.ts`

**Checkpoint**: Destination areas can be queried and loaded dynamically.

---

## Phase 5: User Story 1 - Public PQRSDF Ticket Registration (Priority: P1) 🎯 MVP

**Goal**: Allow citizens to submit a PQRSDF ticket publicly, validating plain text inputs (character counters), supporting anonymous filings exclusively for Denuncias and Sugerencias, and persisting the ticket with a unique `YYYY-NNNNNNNN` radicado.

**Independent Test**: Can be tested by submitting valid tickets via `POST /api/v1/pqrsdf`, asserting HTTP 201 with unique radicado and calculated deadline, and verifying that anonymous submissions are allowed only for Denuncias and Sugerencias.

### Tests for User Story 1
- [X] T024 [P] [US1] Unit tests for `RadicadoNumber` value object invariants and format in `backend/tests/Application.UnitTests/Domain/RadicadoNumberTests.cs`
- [X] T025 [P] [US1] Unit tests for `MakePqrsdfCommandHandler` (standard, anonymous, and invalid input flows) in `backend/tests/Application.UnitTests/Features/Pqrsdf/MakePqrsdfCommandHandlerTests.cs`
- [X] T026 [P] [US1] Component tests for `PqrsdfForm` in `frontend/tests/pqrsdf/PqrsdfForm.test.tsx`

### Implementation for User Story 1
- [X] T027 [P] [US1] Create `PqrsdfTicket` aggregate root enforcing domain invariants and anonymous rules in `backend/src/Domain/Entities/PqrsdfTicket.cs`
- [X] T028 [P] [US1] Create `PqrsdfTicketConfiguration` for EF Core in `backend/src/Infrastructure/Persistence/Configurations/PqrsdfTicketConfiguration.cs`
- [X] T029 [P] [US1] Create `IPqrsdfTicketRepository` and `PqrsdfTicketRepository` in `backend/src/Domain/Repositories/IPqrsdfTicketRepository.cs` and `backend/src/Infrastructure/Persistence/Repositories/PqrsdfTicketRepository.cs`
- [X] T030 [US1] Implement `MakePqrsdfCommand`, `MakePqrsdfCommandHandler`, `MakePqrsdfRequest`, `MakePqrsdfResponse`, and `MakePqrsdfCommandValidator` in `backend/src/Application/Features/Pqrsdf/UseCases/MakePqrsdf/`
- [X] T031 [US1] Expose `POST /api/v1/pqrsdf` in `backend/src/Api/Controllers/V1/PqrsdfController.cs`
- [X] T032 [P] [US1] Create accessible visual `CharacterCounter` component in `frontend/src/app/pqrsdf/components/CharacterCounter.tsx`
- [X] T033 [P] [US1] Implement `usePqrsdfForm` hook with Zod schema validation and anonymous toggle in `frontend/src/app/pqrsdf/hooks/usePqrsdfForm.ts`
- [X] T034 [US1] Implement public accessible `PqrsdfForm` component with plain text fields, character counters, conditional anonymous fields, and anti-double-click lock in `frontend/src/app/pqrsdf/components/PqrsdfForm.tsx`

**Checkpoint**: Core public filing workflow functional and verifiable end-to-end.

---

## Phase 6: User Story 3 - Immediate Filing Confirmation and Legal Receipt (Priority: P1)

**Goal**: Present the citizen with an immediate, accessible confirmation screen displaying the assigned radicado number, submission timestamp, calculated statutory deadline, and an action to copy the radicado to clipboard.

**Independent Test**: Can be tested by submitting a ticket and verifying the UI switches to confirmation mode with radicado, due date, and copy-to-clipboard action without re-submitting on refresh.

### Tests for User Story 3
- [X] T035 [P] [US3] Component tests for `ConfirmationReceipt` in `frontend/tests/pqrsdf/ConfirmationReceipt.test.tsx`

### Implementation for User Story 3
- [X] T036 [P] [US3] Implement `ConfirmationReceipt` component with clipboard copy action and return button in `frontend/src/app/pqrsdf/components/ConfirmationReceipt.tsx`
- [X] T037 [US3] Integrate `PqrsdfForm` and `ConfirmationReceipt` in `frontend/src/app/pqrsdf/page.tsx`

**Checkpoint**: End-to-end citizen journey from public form to receipt screen complete.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Verification, OpenAPI contracts synchronization, and quality gates

- [X] T038 [P] Regenerate OpenAPI TypeScript types in `frontend/src/shared/api/generated/schema.d.ts`
- [X] T039 [P] Add architectural tests in `backend/tests/Architecture.Tests/PqrsdfArchitectureTests.cs` verifying CQRS use case co-location and Clean Architecture boundaries
- [X] T040 Write and execute concurrency test verifying 50 simultaneous submissions produce 0 duplicate radicados in `backend/tests/Application.UnitTests/Domain/RadicadoConcurrencyTests.cs`
- [X] T041 Execute end-to-end verification scenarios per `specs/003-pqrsdf-make-ticket/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Story 4 (Phase 3)**: Depends on Foundational completion — provides the due date engine for US1
- **User Story 2 (Phase 4)**: Depends on Foundational completion — provides destination area catalog for US1
- **User Story 1 (Phase 5 - MVP)**: Depends on US4 (due date calculation) and US2 (destination areas)
- **User Story 3 (Phase 6)**: Depends on US1 completion — displays confirmation for registered ticket
- **Polish (Phase 7)**: Depends on all user stories complete

### Parallel Opportunities

- **Phase 1**: `T002` (package install) and `T003` (messages) in parallel
- **Phase 2**: `T005`, `T006`, `T007`, `T008`, `T009`, `T010`, `T012` in parallel (distinct files)
- **Phase 3**: `T014` and `T015` unit tests in parallel
- **Phase 4 & 5**: Once Foundational is complete, US4 and US2 can proceed in parallel
- **Frontend & Backend in US1**: `T032` (`CharacterCounter`) and `T033` (`usePqrsdfForm`) can be developed in parallel with backend `T027`-`T030`
- **Phase 7**: `T038` and `T039` in parallel

---

## Implementation Strategy (MVP First)

1. **Setup & Foundation**: Complete Phase 1 and Phase 2.
2. **Engines & Catalogs**: Complete Phase 3 (US4 Due Date Calculator) and Phase 4 (US2 Destination Areas).
3. **Core Filing (MVP)**: Complete Phase 5 (US1 Ticket Registration). At this point, public filing works end-to-end.
4. **Receipt & UX**: Complete Phase 6 (US3 Confirmation Screen).
5. **Validation & Polish**: Complete Phase 7 (Concurrency test, quickstart verification, architecture tests).
