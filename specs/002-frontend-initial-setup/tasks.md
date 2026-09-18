# Tasks: Frontend Initial Setup with Next.js App Router and Screaming Architecture

**Input**: Design documents from `specs/002-frontend-initial-setup/` (`plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`)  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`  
**Constitution**: Compliant with Constitution v1.3.0 (Principles I, III, IV, V, VI, VII)  
**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (`[US1]`, `[US2]`, `[US3]`, `[US4]`)
- Every task includes exact file paths

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, package dependencies, environment templates, and framework configuration in `frontend/`.

- [ ] T001 Initialize Next.js 15 application configuration, scripts, and dependencies in `frontend/package.json`
- [ ] T002 [P] Configure strict TypeScript compiler settings (`"strict": true`) and path aliases (`@/app/*`, `@/shared/*`) in `frontend/tsconfig.json`
- [ ] T003 [P] Configure Tailwind CSS and PostCSS targeting the institutional light theme in `frontend/tailwind.config.ts`, `frontend/postcss.config.mjs`, and `frontend/src/app/globals.css`
- [ ] T004 [P] Configure committed environment variables template in `frontend/.env.example` and exclusion rules in `frontend/.gitignore`
- [ ] T005 Configure HTTP security headers (CSP, X-Frame-Options, X-Content-Type-Options) and next-intl plugin in `frontend/next.config.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core architectural types, test runner harness, module boundaries linter, and i18n request engine that MUST be complete before ANY user story can proceed.

**⚠️ CRITICAL**: No user story implementation can begin until this phase is complete.

- [ ] T006 Implement zero-dependency `Result<T, E>` monad and type guards (`ok`, `err`, `isOk`, `isErr`) in `frontend/src/shared/types/result.ts`
- [ ] T007 [P] Define `ApiError` interface and HTTP error structures in `frontend/src/shared/types/api.ts`
- [ ] T008 [P] Configure Vitest test runner, JSDOM environment, and global Next.js navigation mocks in `frontend/vitest.config.ts` and `frontend/tests/setup.ts`
- [ ] T009 [P] Configure ESLint flat config with `eslint-plugin-boundaries` preventing cross-domain imports and Prettier in `frontend/eslint.config.mjs`
- [ ] T010 Setup `next-intl` request handler with Spanish default locale in `frontend/src/i18n/request.ts` and type augmentation in `frontend/src/types/global.d.ts`

**Checkpoint**: Foundation ready - user story implementation can now begin.

---

## Phase 3: User Story 1 - Developer Bootstraps Frontend Project (Priority: P1) 🎯 MVP

**Goal**: A developer can install dependencies, run the development server, execute type checking, and lint the codebase cleanly in under 5 minutes with zero errors.

**Independent Test**: Run `pnpm install`, `pnpm typecheck`, `pnpm lint`, and `pnpm dev` inside `frontend/`; verify that the app launches at `http://localhost:3000` displaying the localized landing page with 0 errors.

### Tests for User Story 1
- [ ] T011 [P] [US1] Create unit tests verifying `Result<T, E>` type guards (`ok`, `err`, `isOk`, `isErr`) in `frontend/tests/unit/result.test.ts`

### Implementation for User Story 1
- [ ] T012 [P] [US1] Create central Spanish message dictionary with `Common` and `Navigation` namespaces in `frontend/messages/es.json`
- [ ] T013 [US1] Implement root layout with HTML lang attribute, base fonts, and `NextIntlClientProvider` in `frontend/src/app/layout.tsx`
- [ ] T014 [US1] Implement landing home page with localized institutional portal presentation and domain links in `frontend/src/app/page.tsx`
- [ ] T015 [US1] Create comprehensive developer documentation covering prerequisites, installation, and available scripts in `frontend/README.md`

**Checkpoint**: At this point, User Story 1 is fully functional and delivers the runnable MVP foundation.

---

## Phase 4: User Story 2 - Developer Navigates Domain-Driven App Router Architecture (Priority: P1)

**Goal**: Establish Screaming Architecture under `src/app/` with direct route segments (`pqrsdf`, `auth`, `dashboard`), each rendering a localized placeholder screen while ESLint strictly blocks cross-domain imports.

**Independent Test**: Navigate to `/pqrsdf`, `/auth`, and `/dashboard`; verify each renders an informative placeholder in Spanish; run `pnpm lint` on intentional cross-domain imports and confirm ESLint reports boundary violations.

### Implementation for User Story 2
- [ ] T016 [P] [US2] Add domain message catalogs for `Pqrsdf`, `Auth`, and `Dashboard` namespaces to `frontend/messages/es.json`
- [ ] T017 [P] [US2] Implement PQRSDF domain route placeholder page in Spanish in `frontend/src/app/pqrsdf/page.tsx`
- [ ] T018 [P] [US2] Implement Authentication domain route placeholder page in Spanish in `frontend/src/app/auth/page.tsx`
- [ ] T019 [P] [US2] Implement Dashboard domain route placeholder page in Spanish in `frontend/src/app/dashboard/page.tsx`
- [ ] T020 [US2] Verify and enforce ESLint boundary rules forbidding cross-feature imports between `src/app/auth` and `src/app/pqrsdf` in `frontend/eslint.config.mjs`

**Checkpoint**: User Stories 1 and 2 work independently. Screaming Architecture and module boundaries are fully established.

---

## Phase 5: User Story 3 - Developer Integrates API Contract with Backend via OpenAPI (Priority: P2)

**Goal**: Provide on-demand typed OpenAPI client generation (`pnpm generate-api`) producing `schema.d.ts` and a typed client wrapper returning `Result<T, ApiError>`.

**Independent Test**: Run `pnpm generate-api` to generate `schema.d.ts`; execute unit tests with mocked API responses verifying that `safeRequest` wraps data in `Ok` and maps HTTP errors into `Err<ApiError>` without throwing.

### Tests for User Story 3
- [ ] T021 [P] [US3] Implement unit tests for `safeRequest` handling 200 OK, 4xx/5xx errors, and network failures with Vitest mocks in `frontend/tests/unit/client.test.ts`

### Implementation for User Story 3
- [ ] T022 [P] [US3] Create initial baseline OpenAPI TypeScript schema definition in `frontend/src/shared/api/generated/schema.d.ts`
- [ ] T023 [US3] Implement strongly typed API client wrapper `rawClient` and `safeRequest` using `openapi-fetch` in `frontend/src/shared/api/client.ts`
- [ ] T024 [P] [US3] Configure on-demand OpenAPI codegen script `pnpm generate-api` pointing to `OPENAPI_URL` with offline resilience in `frontend/package.json`

**Checkpoint**: User Story 3 is complete. API contracts and type-safe HTTP communication are ready for domain consumption.

---

## Phase 6: User Story 4 - Unhandled Error Resilience and Contingency UI (Priority: P2)

**Goal**: Provide friendly, localized contingency screens in Spanish for unhandled runtime errors (`error.tsx`) and missing routes (`not-found.tsx`), alongside reusable accessible components (`LoadingSpinner`, `EmptyState`).

**Independent Test**: Navigate to `/invalid-url` to verify `not-found.tsx`; simulate a component throw to verify `error.tsx` renders retry action; run component tests for `LoadingSpinner`.

### Tests for User Story 4
- [ ] T025 [P] [US4] Implement component test verifying `LoadingSpinner` ARIA attributes and accessibility in `frontend/tests/components/LoadingSpinner.test.tsx`

### Implementation for User Story 4
- [ ] T026 [P] [US4] Implement accessible `LoadingSpinner` component with ARIA live region in `frontend/src/shared/components/LoadingSpinner.tsx`
- [ ] T027 [P] [US4] Implement accessible `EmptyState` component with localized copy and action button in `frontend/src/shared/components/EmptyState.tsx`
- [ ] T028 [US4] Implement global Client Error Boundary contingency screen with retry button in Spanish in `frontend/src/app/error.tsx`
- [ ] T029 [US4] Implement global 404 Not Found screen with portal return link in Spanish in `frontend/src/app/not-found.tsx`

**Checkpoint**: All user stories (US1 through US4) are fully functional and resilient.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: End-to-end verification, formatting, and quickstart validation across the entire frontend application.

- [ ] T030 [P] Execute complete automated verification suite (`pnpm typecheck`, `pnpm lint`, `pnpm test`, `pnpm format:check`) per `specs/002-frontend-initial-setup/quickstart.md`
- [ ] T031 Perform manual smoke testing of development server and all routes (`/`, `/pqrsdf`, `/auth`, `/dashboard`, 404) per `specs/002-frontend-initial-setup/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately.
- **Foundational (Phase 2)**: Depends on Setup completion - **BLOCKS all user stories**.
- **User Stories (Phase 3+)**: All depend on Foundational phase completion.
  - US1 (P1) and US2 (P1) form the core application structure.
  - US3 (P2) and US4 (P2) build upon the foundation and shared types.
- **Polish (Phase 7)**: Depends on all user stories being implemented.

### User Story Dependencies

- **User Story 1 (P1)**: Can start immediately after Foundational (Phase 2). No dependency on other stories.
- **User Story 2 (P1)**: Can start after Foundational (Phase 2). Integrates with root layout from US1.
- **User Story 3 (P2)**: Depends on `Result<T, E>` and `ApiError` from Foundational. Independent of US2 and US4.
- **User Story 4 (P2)**: Depends on message catalog from US1/Foundational. Independent of US3.

### Parallel Opportunities

- **Phase 1 (Setup)**: Tasks T002, T003, T004 can run in parallel.
- **Phase 2 (Foundational)**: Tasks T007, T008, T009 can run in parallel after T006.
- **Phase 3 (User Story 1)**: Test T011 and message catalog T012 can run in parallel before T013/T014.
- **Phase 4 (User Story 2)**: Placeholder domain routes T017, T018, T019 can run in parallel.
- **Phase 5 (User Story 3)**: Test T021 and schema T022 can run in parallel.
- **Phase 6 (User Story 4)**: Component test T025, `LoadingSpinner` T026, and `EmptyState` T027 can run in parallel.

---

## Parallel Example: User Story 2 (Domain Routes)

```bash
# Launch domain placeholder pages concurrently:
Task: "Implement PQRSDF domain route placeholder in frontend/src/app/pqrsdf/page.tsx"
Task: "Implement Authentication domain route placeholder in frontend/src/app/auth/page.tsx"
Task: "Implement Dashboard domain route placeholder in frontend/src/app/dashboard/page.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)
1. Complete Phase 1: Setup (T001–T005)
2. Complete Phase 2: Foundational (T006–T010)
3. Complete Phase 3: User Story 1 (T011–T015)
4. **STOP and VALIDATE**: Run `pnpm install`, `pnpm typecheck`, `pnpm lint`, `pnpm test`, and `pnpm dev` to verify the working application.

### Incremental Delivery
1. Setup + Foundational → Solid, type-safe base with ESLint boundaries and Result monad.
2. User Story 1 → Working application runtime, root layout, and landing page in Spanish (MVP).
3. User Story 2 → Screaming Architecture domains (`pqrsdf`, `auth`, `dashboard`) with route boundaries.
4. User Story 3 → On-demand OpenAPI codegen and typed API client.
5. User Story 4 → Accessible contingency UI (`error.tsx`, `not-found.tsx`, `LoadingSpinner`, `EmptyState`).
6. Polish → 100% test pass rate, 0 type errors, 0 lint warnings.
