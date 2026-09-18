# Tasks: User Authentication & Role-Based Access Control (005-auth-user)

**Input**: Design documents from `specs/005-auth-user/` (`spec.md`, `plan.md`, `data-model.md`, `research.md`, `contracts/`, `quickstart.md`)  
**Prerequisites**: `plan.md` (complete), `spec.md` (complete), `data-model.md` (complete), `contracts/` (complete), `quickstart.md` (complete)  
**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no blocking dependencies)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`, `US4`)
- Includes exact file paths in all descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project dependencies, configuration, rate limiter policies, and message localization

- [ ] T001 Add `BCrypt.Net-Next` NuGet package to `backend/src/Infrastructure/Pqrsdf.Infrastructure.csproj` and `Microsoft.AspNetCore.Authentication.JwtBearer` to `backend/src/Api/Pqrsdf.Api.csproj`
- [ ] T002 [P] Add Spanish localization keys for auth, login form, validation feedback, session alerts, role badges, and logout in `frontend/messages/es.json`
- [ ] T003 [P] Configure JWT settings (`Issuer`, `Audience`, `Secret`, `ExpiryMinutes`) in `backend/src/Api/appsettings.json` and `backend/src/Api/appsettings.Development.json`
- [ ] T004 [P] Configure IP-based rate limiting policy (`LoginRateLimit`, 5 req/min) with custom 429 ProblemDetails response in `backend/src/Api/Program.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain entities, value objects, repository contracts, security abstractions, and database schema mappings that block user story implementation

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T005 Create `UserRole` enum (`Funcionario = 1`, `Administrador = 2`) in `backend/src/Domain/Enums/UserRole.cs`
- [ ] T006 [P] Create `Email` and `PasswordHash` Value Objects with invariant validation in `backend/src/Domain/ValueObjects/Email.cs` and `backend/src/Domain/ValueObjects/PasswordHash.cs`
- [ ] T007 Create `User` aggregate root with domain methods (`Create`, `RecordLogin`, `Deactivate`, `Activate`, `ChangePassword`) in `backend/src/Domain/Entities/User.cs`
- [ ] T008 [P] Define `IUserRepository` interface in `backend/src/Domain/Repositories/IUserRepository.cs`
- [ ] T009 [P] Define `IPasswordHasher` and `IJwtTokenGenerator` abstractions in `backend/src/Application/Common/Interfaces/IPasswordHasher.cs` and `backend/src/Application/Common/Interfaces/IJwtTokenGenerator.cs`
- [ ] T010 [P] Create `UserProfileDto` in `backend/src/Application/Shared/Dtos/UserProfileDto.cs`
- [ ] T011 Implement `BCryptPasswordHasher` using `BCrypt.Net-Next` in `backend/src/Infrastructure/Security/BCryptPasswordHasher.cs`
- [ ] T012 [P] Implement `JwtTokenGenerator` using `System.IdentityModel.Tokens.Jwt` in `backend/src/Infrastructure/Security/JwtTokenGenerator.cs`
- [ ] T013 Implement `UserRepository` with EF Core queries in `backend/src/Infrastructure/Persistence/Repositories/UserRepository.cs`
- [ ] T014 Configure `UserConfiguration` with column constraints, unique email index, and pre-seeded development accounts (`admin@pqrsdf.gov.co` and `funcionario@pqrsdf.gov.co`) in `backend/src/Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- [ ] T015 Register `DbSet<User>` in `backend/src/Infrastructure/Persistence/AppDbContext.cs` and create EF Core migration for `Users` table and seed data
- [ ] T016 Register security and repository services in `backend/src/Infrastructure/DependencyInjection.cs`

**Checkpoint**: Foundation ready - User aggregate, security services, and database schema in place.

---

## Phase 3: User Story 1 - Secure Staff Login and Role-Based Redirection (Priority: P1) 🎯 MVP

**Goal**: Allow internal staff members (Funcionario or Administrador) to log in using their credentials (email/password), validate against BCrypt hashes, issue an 8-hour JWT with role claims, and redirect to the management dashboard.

**Independent Test**: Can be tested independently by navigating to `/auth`, submitting valid credentials for `funcionario@pqrsdf.gov.co` or `admin@pqrsdf.gov.co`, and verifying that the system issues a valid JWT and redirects to `/dashboard` in < 2 seconds, while invalid credentials return a generic 401 error message in Spanish.

### Tests for User Story 1
- [ ] T017 [P] [US1] Unit tests for `User` domain aggregate root and invariants in `backend/tests/Domain.UnitTests/Entities/UserTests.cs`
- [ ] T018 [P] [US1] Unit tests for `LoginUserCommandHandler` (successful login, invalid credentials 401, inactive account 401) in `backend/tests/Application.UnitTests/Features/Auth/LoginUserCommandHandlerTests.cs`

### Implementation for User Story 1
- [ ] T019 [P] [US1] Create `LoginUserCommand`, `LoginUserRequestDto`, and `LoginUserResponse` DTOs in `backend/src/Application/Features/Auth/UseCases/LoginUser/LoginUserCommand.cs`
- [ ] T020 [US1] Implement `LoginUserCommandHandler` orchestrating user retrieval, password verification via `IPasswordHasher`, active check, login timestamp update, and JWT token issuance via `IJwtTokenGenerator` in `backend/src/Application/Features/Auth/UseCases/LoginUser/LoginUserCommandHandler.cs`
- [ ] T021 [US1] Expose `POST /api/v1/auth/login` with `[EnableRateLimiting("LoginRateLimit")]` and `[AllowAnonymous]` in `backend/src/Api/Controllers/V1/AuthController.cs`
- [ ] T022 [P] [US1] Create frontend auth contract types in `frontend/src/app/auth/types/auth.types.ts`
- [ ] T023 [P] [US1] Add `loginStaff` method to API client in `frontend/src/shared/api/client.ts`
- [ ] T024 [P] [US1] Create `LoginForm` component with email/password input, loading states, validation, and localized Spanish alerts in `frontend/src/app/auth/components/LoginForm.tsx`
- [ ] T025 [US1] Implement `useAuth` hook managing authentication state, storing JWT in `auth_token` cookie, and handling redirection to `/dashboard` in `frontend/src/app/auth/hooks/useAuth.ts`
- [ ] T026 [US1] Implement login page at `/auth` rendering breadcrumbs and `LoginForm` in `frontend/src/app/auth/page.tsx`
- [ ] T027 [P] [US1] Adapt `/dashboard` page to render role badge, user display name, and role-based welcome in `frontend/src/app/dashboard/page.tsx`

**Checkpoint**: At this point, User Story 1 (MVP) is fully functional and testable independently.

---

## Phase 4: User Story 2 - API Route Protection and Role Authorization (Priority: P1)

**Goal**: Backend API strictly enforces JWT Bearer authentication and role-based authorization on internal management endpoints while keeping citizen public endpoints freely accessible.

**Independent Test**: Can be tested independently by querying internal endpoints without a token (verifying 401 Unauthorized), querying with a valid Bearer token (verifying 200 OK), and querying citizen endpoints without a token (verifying 200 OK).

- [ ] T028 [US2] Configure JWT Bearer authentication and role policies (`Funcionario`, `Administrador`) in `backend/src/Api/Program.cs`
- [ ] T029 [US2] Configure Swagger / OpenAPI to support JWT Bearer authorization in `backend/src/Api/Program.cs`
- [ ] T030 [US2] Apply `[Authorize]` attribute to internal management endpoints while ensuring public endpoints (`PqrsdfController`, `SystemController`) remain accessible via `[AllowAnonymous]` in `backend/src/Api/Controllers/`
- [ ] T031 [P] [US2] Integration test verifying 401 for unauthenticated access to protected endpoints and 200 with Bearer token in `backend/tests/Application.UnitTests/Features/Auth/AuthEndpointSecurityTests.cs`

**Checkpoint**: API security gateway enforced; internal routes locked down, public citizen endpoints open.

---

## Phase 5: User Story 3 - Frontend Navigation Guarding and Redirect Handling (Priority: P2)

**Goal**: Frontend application automatically guards private management views (`/dashboard`), redirecting unauthenticated visitors to `/auth` with destination preservation (`?returnUrl=...`), and seamlessly restores the intended destination after login.

**Independent Test**: Can be tested independently in an incognito window by attempting to navigate directly to `/dashboard`, verifying redirection to `/auth?returnUrl=%2Fdashboard`, and verifying that successful login forwards immediately to `/dashboard`.

- [ ] T032 [US3] Create Next.js route guard middleware inspecting `auth_token` cookie and redirecting unauthenticated visits to protected routes to `/auth?returnUrl=...` in `frontend/src/middleware.ts`
- [ ] T033 [US3] Update `LoginForm` and `useAuth` hook to read `returnUrl` from query parameters and redirect to it upon successful login in `frontend/src/app/auth/components/LoginForm.tsx`
- [ ] T034 [P] [US3] Update client API fetch interceptor to attach `Authorization: Bearer <token>` from cookie and trigger re-login redirect upon receiving 401 in `frontend/src/shared/api/client.ts`

**Checkpoint**: Frontend route guarding operational; sensitive views protected before render.

---

## Phase 6: User Story 4 - Session Termination (Logout) (Priority: P2)

**Goal**: Authenticated staff can log out at any time from the navigation bar, clearing the client token and session state, and preventing subsequent access to management sections without re-authenticating.

**Independent Test**: Can be tested independently by logging in, clicking "Cerrar Sesión", verifying token cookie removal, and confirming that navigating back to `/dashboard` triggers the unauthenticated redirect.

- [ ] T035 [US4] Implement `logout` action in `useAuth` clearing `auth_token` cookie and resetting user state in `frontend/src/app/auth/hooks/useAuth.ts`
- [ ] T036 [P] [US4] Add user profile indicator (name, role badge) and "Cerrar Sesión" button in the global navigation bar in `frontend/src/shared/components/Navigation.tsx`
- [ ] T037 [US4] Verify logout navigation directs user to `/` with localized feedback and denies browser history backward navigation in `frontend/src/app/auth/hooks/useAuth.ts`

**Checkpoint**: Complete session lifecycle verified (Login -> Protected Operations -> Logout).

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Verification against quality criteria, automated testing, and quickstart scenarios

- [ ] T038 [P] Run backend unit, domain, and architecture tests with `dotnet test` in `backend/`
- [ ] T039 [P] Run frontend build, lint, and tests with `npm run build && npm test` in `frontend/`
- [ ] T040 Execute end-to-end quickstart validation scenarios from `specs/005-auth-user/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately.
- **Foundational (Phase 2)**: Depends on Phase 1 completion - BLOCKS all user stories.
- **User Stories (Phase 3+)**: Depend on Phase 2 completion.
  - **User Story 1 (P1)**: Independent MVP slice.
  - **User Story 2 (P1)**: Depends on User Story 1 login token generation to validate Bearer tokens.
  - **User Story 3 (P2)**: Depends on User Story 1 frontend login form and cookie setup.
  - **User Story 4 (P2)**: Depends on User Story 3 session setup.
- **Polish (Phase 7)**: Depends on all user stories being completed.

---

## Parallel Opportunities

- **Setup**: T002, T003, T004 can run in parallel after T001.
- **Foundational**: T006, T008, T009, T010, T012 can run in parallel once domain enums/entities are drafted.
- **User Story 1**: Unit tests (T017, T018), DTOs (T019), and frontend UI components (T022, T023, T024, T027) can proceed in parallel.
- **User Story 2**: T031 integration tests can run alongside controller annotations.
- **Polish**: T038 and T039 can run in parallel.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T004).
2. Complete Phase 2: Foundational (T005-T016).
3. Complete Phase 3: User Story 1 (T017-T027).
4. **Validate MVP**: Test `/auth` login with seed accounts, verify token in cookie and redirection to `/dashboard`.

### Incremental Delivery

1. Setup + Foundational -> Core database & security ready.
2. User Story 1 -> Login & JWT token issuance operational (MVP).
3. User Story 2 -> API endpoints protected with Bearer authorization.
4. User Story 3 -> Next.js route protection with `returnUrl`.
5. User Story 4 -> Logout and session termination.
6. Polish -> Full test suite and quickstart verification.
