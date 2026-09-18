# Tasks: PQRSDF Ticket Assignment to Staff (006-pqrsdf-assign-tickets)

**Input**: Design documents from `specs/006-pqrsdf-assign-tickets/` (`spec.md`, `plan.md`, `data-model.md`, `research.md`, `contracts/`, `quickstart.md`)  
**Prerequisites**: `plan.md` (complete), `spec.md` (complete), `data-model.md` (complete), `contracts/` (complete), `quickstart.md` (complete)  
**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no blocking dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`)
- Includes exact file paths in all descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Localization keys, contract types, and shared UI badges for the assignment module

- [X] T001 Add Spanish localization keys for assignment module (tabs, filters, urgency badges, workload ratio, modal forms, drawer headers, confirmation messages, error alerts) in `frontend/messages/es.json`
- [X] T002 [P] Create frontend contract types for unassigned tickets, official roster, assignment requests, and inbox in `frontend/src/app/dashboard/assignments/types/assignment.types.ts`
- [X] T003 [P] Create shared SLA urgency badge (`UrgencyBadge.tsx`) mapping 3 levels (Red ≤ 3d/overdue, Yellow 4-7d, Green ≥ 8d) in `frontend/src/shared/components/UrgencyBadge.tsx`
- [X] T004 [P] Create shared official workload badge (`WorkloadBadge.tsx`) rendering active ticket ratio (e.g., "3/5 solicitudes") and full capacity warning in `frontend/src/shared/components/WorkloadBadge.tsx`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain entities, value objects, repository contracts, and database schema mappings that block user story implementation

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T005 Create `AssignmentType` enum (`InitialAssignment = 1`, `Reassignment = 2`) in `backend/src/Domain/Enums/AssignmentType.cs`
- [X] T006 [P] Create `TicketAssignmentHistory` immutable audit domain entity in `backend/src/Domain/Entities/TicketAssignmentHistory.cs`
- [X] T007 Extend `PqrsdfTicket` aggregate root with `AssignedToUserId`, `AssignedAtUtc`, `AssignmentNote`, and domain business methods `AssignToOfficial` and `ReassignToOfficial` in `backend/src/Domain/Entities/PqrsdfTicket.cs`
- [X] T008 [P] Define `ITicketAssignmentHistoryRepository` repository contract in `backend/src/Domain/Repositories/ITicketAssignmentHistoryRepository.cs`
- [X] T009 [P] Extend `IPqrsdfTicketRepository` with queries for unassigned, assigned, and official inbox tickets in `backend/src/Domain/Repositories/IPqrsdfTicketRepository.cs`
- [X] T010 [P] Configure EF Core mapping for `TicketAssignmentHistory` with foreign keys and indexes in `backend/src/Infrastructure/Persistence/Configurations/TicketAssignmentHistoryConfiguration.cs`
- [X] T011 Configure EF Core mapping for `PqrsdfTicket` with assignment columns and composite indexes (`AssignedToUserId`, `Status`, `DueDate`) in `backend/src/Infrastructure/Persistence/Configurations/PqrsdfTicketConfiguration.cs`
- [X] T012 Register `DbSet<TicketAssignmentHistory>` in `backend/src/Infrastructure/Persistence/PqrsdfDbContext.cs`
- [X] T013 Implement `TicketAssignmentHistoryRepository` with EF Core persistence in `backend/src/Infrastructure/Persistence/Repositories/TicketAssignmentHistoryRepository.cs`
- [X] T014 Implement updated query methods in `backend/src/Infrastructure/Persistence/Repositories/PqrsdfTicketRepository.cs`
- [X] T015 Register `ITicketAssignmentHistoryRepository` in `backend/src/Infrastructure/DependencyInjection.cs`

**Checkpoint**: Foundation ready - Domain aggregates, audit entity, repositories, and database mappings complete.

---

## Phase 3: User Story 1 - View Unassigned PQRSDF Queue (Priority: P1) 🎯 MVP

**Goal**: Allow authenticated Administrators to view a centralized list of unassigned PQRSDF requests ordered by nearest statutory due date, filter by request type and area, inspect citizen descriptions in a slide-over drawer, and deny access to non-administrators.

**Independent Test**: Can be tested independently by logging in as an `Administrador`, navigating to `/dashboard/assignments`, and verifying that all unassigned tickets are rendered sorted by nearest due date with filters, clicking a ticket opens the slide-over preview drawer, and non-admins receive 403 Forbidden.

### Tests for User Story 1
- [X] T016 [P] [US1] Unit tests for `GetUnassignedTicketsQueryHandler` (sorting by due date, type/area filters, SLA calculation) in `backend/tests/Application.UnitTests/Features/Pqrsdf/GetUnassignedTicketsQueryHandlerTests.cs`

### Implementation for User Story 1
- [X] T017 [P] [US1] Create `GetUnassignedTicketsQuery`, Request, and Response DTOs in `backend/src/Application/Features/Pqrsdf/UseCases/GetUnassignedTickets/GetUnassignedTicketsQuery.cs`
- [X] T018 [US1] Implement `GetUnassignedTicketsQueryHandler` calculating remaining business days and urgency level via `IColombianCalendarService` in `backend/src/Application/Features/Pqrsdf/UseCases/GetUnassignedTickets/GetUnassignedTicketsQueryHandler.cs`
- [X] T019 [US1] Expose `GET /api/v1/assignments/unassigned` in `backend/src/Api/Controllers/V1/AssignmentsController.cs` protected with `[Authorize(Roles = "Administrador")]`
- [X] T020 [P] [US1] Add `getUnassignedTickets` method to API client in `frontend/src/shared/api/client.ts`
- [X] T021 [P] [US1] Create `TicketDetailDrawer` component rendering citizen description, subject, filing date, due date, and urgency badge in `frontend/src/app/dashboard/assignments/components/TicketDetailDrawer.tsx`
- [X] T022 [US1] Create `UnassignedQueueTable` component rendering list, filters (type, destination area), search input, urgency badges, and detail drawer trigger in `frontend/src/app/dashboard/assignments/components/UnassignedQueueTable.tsx`
- [X] T023 [US1] Create assignments page `frontend/src/app/dashboard/assignments/page.tsx` integrating queue table, detail drawer, client-side RBAC guard for `Administrador`, and error state handling
- [X] T024 [US1] Add "Asignación de Solicitudes" card in `frontend/src/app/dashboard/components/DashboardContent.tsx` conditionally rendered for users with role `Administrador` role

**Checkpoint**: At this point, User Story 1 (MVP) is fully functional and testable independently.

---

## Phase 4: User Story 2 - Assign PQRSDF to an Official with Capacity Enforcement (Priority: P1)

**Goal**: Allow Administrators to select an unassigned ticket, pick an eligible active official (Funcionario) who has not exceeded the 5-ticket capacity limit, confirm the assignment, transition the ticket to `InReview`, and persist an immutable audit trail.

**Independent Test**: Can be tested independently by selecting an unassigned ticket, picking an official with < 5 active tickets, confirming assignment, and asserting that the ticket disappears from the unassigned queue, transitions to `InReview` in the database, and an audit record is stored in `TicketAssignmentHistories`.

### Tests for User Story 2
- [X] T025 [P] [US2] Unit tests for `PqrsdfTicket.AssignToOfficial` domain method (status transition, 5-ticket invariant validation, error handling) in `backend/tests/Domain.UnitTests/Entities/PqrsdfTicketAssignmentTests.cs`
- [X] T026 [P] [US2] Unit tests for `AssignTicketCommandHandler` and `GetAssignableOfficialsQueryHandler` in `backend/tests/Application.UnitTests/Features/Pqrsdf/AssignTicketCommandHandlerTests.cs`

### Implementation for User Story 2
- [X] T027 [P] [US2] Create `GetAssignableOfficialsQuery`, Handler, and DTOs computing active tickets count (0-5) in `backend/src/Application/Features/Pqrsdf/UseCases/GetAssignableOfficials/GetAssignableOfficialsQuery.cs`
- [X] T028 [P] [US2] Create `AssignTicketCommand`, Request, and Response DTOs in `backend/src/Application/Features/Pqrsdf/UseCases/AssignTicket/AssignTicketCommand.cs`
- [X] T029 [US2] Implement `AssignTicketCommandHandler` validating official capacity (< 5), executing domain assignment, persisting audit record, and returning `Result<AssignTicketResponse>` in `backend/src/Application/Features/Pqrsdf/UseCases/AssignTicket/AssignTicketCommandHandler.cs`
- [X] T030 [US2] Expose `GET /api/v1/assignments/officials` and `POST /api/v1/assignments/{radicado}/assign` in `backend/src/Api/Controllers/V1/AssignmentsController.cs`
- [X] T031 [P] [US2] Add `getAssignableOfficials` and `assignTicket` methods to API client in `frontend/src/shared/api/client.ts`
- [X] T032 [P] [US2] Create `AssignOfficialModal` component displaying official roster, workload ratios (`3/5`), optional note field, and disabling officials at capacity in `frontend/src/app/dashboard/assignments/components/AssignOfficialModal.tsx`
- [X] T033 [US2] Integrate `AssignOfficialModal` into `TicketDetailDrawer` and trigger queue refresh upon successful assignment in `frontend/src/app/dashboard/assignments/components/TicketDetailDrawer.tsx`

**Checkpoint**: At this point, User Stories 1 AND 2 are fully functional and integrated.

---

## Phase 5: User Story 3 - Reassignment with Mandatory Justification and Workload Balancing (Priority: P2)

**Goal**: Allow Administrators to navigate between "Sin Asignar" and "En Trámite" tabs, search tickets by radicado, reassign an in-review ticket to another official with mandatory justification (≥ 10 characters), and provide officials with a workload inbox on `/dashboard`.

**Independent Test**: Can be tested independently by navigating to "En Trámite", selecting an assigned ticket, reassigning it to another official with justification, verifying that workloads recalculate and audit logs record the event, and logging in as the assigned official to verify the ticket appears in their `/dashboard` inbox.

### Tests for User Story 3
- [X] T034 [P] [US3] Unit tests for `PqrsdfTicket.ReassignToOfficial` and `ReassignTicketCommandHandler` (mandatory justification ≥ 10 chars, target capacity check) in `backend/tests/Application.UnitTests/Features/Pqrsdf/ReassignTicketCommandHandlerTests.cs`
- [X] T035 [P] [US3] Unit tests for `GetOfficialInboxQueryHandler` in `backend/tests/Application.UnitTests/Features/Pqrsdf/GetOfficialInboxQueryHandlerTests.cs`

### Implementation for User Story 3
- [X] T036 [P] [US3] Create `GetAssignedTicketsQuery`, Handler, and DTOs with radicado search in `backend/src/Application/Features/Pqrsdf/UseCases/GetAssignedTickets/GetAssignedTicketsQuery.cs`
- [X] T037 [P] [US3] Create `GetOfficialInboxQuery`, Handler, and DTOs filtering by `AssignedToUserId == currentUserId` and `Status == InReview` in `backend/src/Application/Features/Pqrsdf/UseCases/GetOfficialInbox/GetOfficialInboxQuery.cs`
- [X] T038 [P] [US3] Create `ReassignTicketCommand`, Handler, and DTOs validating justification length in `backend/src/Application/Features/Pqrsdf/UseCases/ReassignTicket/ReassignTicketCommand.cs`
- [X] T039 [US3] Expose `GET /api/v1/assignments/assigned`, `POST /api/v1/assignments/{radicado}/reassign`, and `GET /api/v1/assignments/my-inbox` in `backend/src/Api/Controllers/V1/AssignmentsController.cs`
- [X] T040 [P] [US3] Add `getAssignedTickets`, `reassignTicket`, and `getOfficialInbox` methods to API client in `frontend/src/shared/api/client.ts`
- [X] T041 [P] [US3] Create `AssignmentTabs` component to switch between "Sin Asignar" and "En Trámite" with radicado search input in `frontend/src/app/dashboard/assignments/components/AssignmentTabs.tsx`
- [X] T042 [P] [US3] Create `InReviewQueueTable` component listing assigned tickets with current official and reassignment trigger in `frontend/src/app/dashboard/assignments/components/InReviewQueueTable.tsx`
- [X] T043 [P] [US3] Create `ReassignOfficialModal` component with official selector, capacity check, and justification validator (10-500 chars) in `frontend/src/app/dashboard/assignments/components/ReassignOfficialModal.tsx`
- [X] T044 [US3] Create `OfficialInboxTable` component for staff `/dashboard` rendering active assigned requests (up to 5) with SLA urgency badges and admin notes in `frontend/src/app/dashboard/components/OfficialInboxTable.tsx`
- [X] T045 [US3] Integrate `OfficialInboxTable` into `frontend/src/app/dashboard/components/DashboardContent.tsx` for authenticated users with role `Funcionario`

**Checkpoint**: All user stories (US1, US2, US3) are fully functional, integrated, and testable independently.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Architecture test verification, test data seeding, and end-to-end scenario validation

- [X] T046 [P] Verify Architecture tests for Clean Architecture layer isolation and CQRS use cases co-location in `backend/tests/Architecture.Tests/ArchitectureTests.cs`
- [X] T047 Seed secondary test official (`funcionario2@pqrsdf.gov.co`) and sample unassigned tickets in `backend/src/Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- [X] T048 Execute all end-to-end validation scenarios defined in `specs/006-pqrsdf-assign-tickets/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories.
- **User Stories (Phase 3+)**: All depend on Foundational phase completion.
  - **User Story 1 (P1)**: Can start immediately after Foundational.
  - **User Story 2 (P1)**: Can start after Foundational; integrates with US1 UI drawer.
  - **User Story 3 (P2)**: Can start after Foundational; integrates with US1 tabs and US2 assignment infrastructure.
- **Polish (Phase 6)**: Depends on completion of User Stories 1, 2, and 3.

---

## Parallel Opportunities

- **Phase 1**: T002, T003, T004 can execute in parallel after T001.
- **Phase 2**: T006, T008, T009, T010 can execute in parallel once T005 and T007 are established.
- **Phase 3 (US1)**: T016 (test), T017 (DTOs), T020 (API client), T021 (drawer), T024 (dashboard card) can execute in parallel.
- **Phase 4 (US2)**: T025, T026 (tests), T027 (officials query), T028 (command DTOs), T031 (client), T032 (modal) can run in parallel.
- **Phase 5 (US3)**: T034, T035 (tests), T036, T037, T038 (queries/command), T040 (client), T041 (tabs), T042 (table), T043 (modal) can run in parallel.

---

## Implementation Strategy: MVP First (User Story 1 Only)

1. Complete **Phase 1: Setup** (T001 - T004).
2. Complete **Phase 2: Foundational** (T005 - T015).
3. Complete **Phase 3: User Story 1** (T016 - T024).
4. **STOP and VALIDATE**: Test User Story 1 independently (admin can log in, view unassigned queue sorted by statutory deadline, filter by type/area, inspect ticket details in drawer, and non-admins receive 403 Forbidden).
5. Proceed to User Story 2 (capacity-aware initial assignment) and User Story 3 (reassignment and staff inbox).
