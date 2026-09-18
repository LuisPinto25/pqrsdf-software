# Feature Specification: Frontend Initial Setup with Next.js App Router and Screaming Architecture

**Feature Branch**: `002-frontend-initial-setup`

**Created**: 2026-09-18

**Status**: Draft

**Input**: User description: "creación de la configuración inicial para el frontend cumpliendo con los requisitos de la arquitectura que debe seguir, los diferentes patrones y las diferentes reglas teniendo en cuenta estándares de desarrollo para frontend con Nextjs"

## Clarifications

### Session 2026-09-18

- Q: ¿Cómo deben mapearse las carpetas de feature bajo `src/app/` hacia las rutas visibles del navegador? → A: Segmentos directos (`src/app/pqrsdf/`, `src/app/auth/`, `src/app/dashboard/`), garantizando correspondencia 1:1 entre carpetas en disco y URLs públicas (`/pqrsdf/...`, `/auth/...`, `/dashboard/...`).
- Q: ¿Qué contenido inicial deben tener estas 3 carpetas de dominio en la configuración base del frontend? → A: Cada módulo de dominio (`src/app/pqrsdf/`, `src/app/auth/`, `src/app/dashboard/`) DEBE incluir un archivo `page.tsx` placeholder mínimo con interfaz en español, permitiendo verificar de inmediato que las rutas del App Router resuelven correctamente.
- Q: ¿Qué convención de aliases debe estandarizarse en `tsconfig.json`? → A: Aliases idiomáticos de Next.js `@/app/*` (mapeado a `src/app/*`) y `@/shared/*` (mapeado a `src/shared/*`), cubriendo cualquier módulo de dominio y recursos transversales sin reconfigurar aliases para cada feature nueva.
- Q: ¿Qué enfoque de simulación (mocking) de la capa API debe estandarizarse en el entorno de pruebas del frontend? → A: Mocking nativo con utilidades de Vitest (`vi.mock` / `vi.fn`) sobre `@/shared/api/client`, evitando dependencias pesadas adicionales en el setup base y simulando respuestas tipadas `{ data, error }`.
- Q: ¿Dónde deben residir los componentes visuales transversales globales de la aplicación? → A: Centralizados en `src/shared/components/` (consumibles vía `@/shared/components/*`), asegurando que ningún componente visual común cree carpetas técnicas sueltas en la raíz ni viole la Screaming Architecture.
- Q: ¿Debe incluirse y pre-configurarse desde esta configuración inicial una librería para gestión y validación de formularios (como react-hook-form y zod)? → A: Postergar a features de formularios dedicadas; el setup inicial se mantiene minimalista y enfocado en la infraestructura base, sin dependencias prematuras de formularios.
- Q: ¿Cómo debe configurarse la estrategia de soporte de temas (Dark / Light Mode) en este setup inicial? → A: Solo modo claro institucional fijo; se descartan toggles manuales y estilos oscuros en esta fase inicial para mantener el diseño alineado exclusivamente con la paleta clara institucional.
- Q: ¿Deben configurarse herramientas de Git hooks (como Husky y lint-staged) dentro del subproyecto frontend? → A: Sin git hooks locales en la fase inicial; el aseguramiento de calidad se realiza mediante scripts ejecutables de pnpm y validación en CI/CD, evitando fricciones de commit en el monorepo compartido.
- Q: ¿Cuándo debe ejecutarse la regeneración de los tipos TypeScript de OpenAPI? → A: Exclusivamente bajo demanda mediante el script `pnpm generate-api`, garantizando que `pnpm dev` y `pnpm build` no fallen si el backend no se encuentra en ejecución.
- Q: ¿Debe configurarse una infraestructura de internacionalización (i18n) desde este setup base o se utilizará español estático directo en los componentes? → A: Integrar `next-intl` con catálogo de mensajes centralizado (`messages/es.json`) y configuración de localización desde el inicio, desacoplando los textos de la UI de los componentes y facilitando la expansión multilingüe futura manteniendo el español como idioma predeterminado.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Developer Bootstraps Frontend Project (Priority: P1)

As a frontend developer,
I want to initialize and run the Next.js application locally using pnpm and strict TypeScript configuration,
So that I can start developing user interface features in a clean, reproducible, and standardized environment in under 5 minutes.

**Why this priority**: Without a working, standardized build and execution environment, no UI development, testing, or API integration can take place.

**Independent Test**: Can be tested by running `pnpm install`, `pnpm dev`, `pnpm typecheck`, and `pnpm lint` in a clean checkout of `frontend/`, verifying that the development server starts on `http://localhost:3000` and displays the initial landing page with zero TypeScript errors or lint warnings.

**Acceptance Scenarios**:

1. **Given** a developer with Node.js LTS and pnpm installed, **When** they execute `pnpm install` and `pnpm dev` inside `frontend/`, **Then** the local development server starts in less than 5 minutes and serves the application at `http://localhost:3000`.
2. **Given** the frontend codebase, **When** running `pnpm typecheck`, **Then** the TypeScript compiler reports 0 errors with strict mode enabled (`strict: true`).
3. **Given** the frontend codebase, **When** running `pnpm lint`, **Then** ESLint passes with 0 warnings and 0 errors, validating code style and architectural boundaries.

---

### User Story 2 - Developer Navigates Domain-Driven App Router Architecture (Priority: P1)

As an application maintainer or reviewer,
I want the folder structure under `src/app/` to reflect the business domains of the PQRSDF system directly through Next.js App Router file-system routing,
So that the codebase screams the business domain (`app/pqrsdf/`, `app/auth/`, `app/dashboard/`), leverages Next.js nested layouts and server/client components, and prevents generic root technical folders.

**Why this priority**: Aligning Screaming Architecture with Next.js App Router file routing enforces separation of concerns, enables modular feature evolution, and prevents architecture drift from day one.

**Independent Test**: Can be tested by inspecting the directory tree under `src/app/` and verifying that domain modules (`pqrsdf`, `auth`, `dashboard`) are established as routing roots, while common utilities are quarantined in `src/shared/`, and ESLint disallows cross-feature imports.

**Acceptance Scenarios**:

1. **Given** the frontend project layout, **When** an architect reviews `src/app/` or navigates to `/pqrsdf`, `/auth`, and `/dashboard`, **Then** they immediately identify the 3 pre-scaffolded business domain routes and each renders an informative placeholder screen in Spanish confirming active routing.
2. **Given** the source tree, **When** searching for generic technical folders in `src/`, **Then** there are NO root-level `/components`, `/hooks`, or `/pages` folders outside of feature routes or `src/shared/`.
3. **Given** an attempt to import code from one feature domain into an unrelated feature domain (e.g. `auth` importing `pqrsdf`), **When** the linter evaluates the file, **Then** an ESLint boundary violation error is reported.

---

### User Story 3 - Developer Integrates API Contract with Backend via OpenAPI (Priority: P2)

As a frontend developer building UI screens,
I want to consume a strongly typed HTTP API client generated from the backend OpenAPI specification,
So that I can invoke backend endpoints with compile-time safety and handle responses using the `Result<T, ApiError>` pattern without unhandled exceptions.

**Why this priority**: Eliminates manual maintenance of API interfaces, synchronizes client and server contracts, and guarantees robust error handling.

**Independent Test**: Can be tested by running `pnpm generate-api`, confirming that `schema.d.ts` is produced under `src/shared/api/generated/`, and importing `apiClient` to verify that method signatures return typed `{ data, error }` unions.

**Acceptance Scenarios**:

1. **Given** the backend OpenAPI specification available at the URL defined in `OPENAPI_URL`, **When** running `pnpm generate-api`, **Then** `schema.d.ts` is generated in `src/shared/api/generated/` in under 60 seconds with 0 runtime JavaScript bundle overhead.
2. **Given** the shared API client in `src/shared/api/client.ts`, **When** invoking an endpoint, **Then** responses return typed data or an `ApiError` without throwing unhandled exceptions for expected 4xx/5xx HTTP statuses.
3. **Given** an offline or unreachable backend during development, **When** running `pnpm generate-api`, **Then** the script exits with a clear error message without corrupting the existing schema.

---

### User Story 4 - Unhandled Error Resilience and Contingency UI (Priority: P2)

As an end user browsing the PQRSDF portal,
I want unexpected rendering errors or broken routes to display friendly, localized contingency screens in Spanish,
So that I never see a white screen of death, technical stack traces, or broken browser sessions.

**Why this priority**: Protects user trust, adheres to constitutional error governance, and ensures accessible recovery actions.

**Independent Test**: Can be tested by simulating a component render throw and navigating to non-existent URLs, asserting that `error.tsx` and `not-found.tsx` render friendly Spanish messages and recovery buttons.

**Acceptance Scenarios**:

1. **Given** an unexpected runtime error during component rendering, **When** intercepted by Next.js, **Then** `src/app/error.tsx` renders a contingency screen in Spanish with a retry action in under 2 seconds.
2. **Given** a user navigates to an invalid route, **When** resolved, **Then** `src/app/not-found.tsx` renders a styled 404 page in Spanish with a link back to the home portal.

---

## Edge Cases & Error Handling *(mandatory)*

- **Backend API Unreachable**: When the backend is offline or network fails, `apiClient` returns an `Err<ApiError>` with `status: 0` and a user-friendly Spanish message, preventing unhandled Promise rejections.
- **Cross-Domain Import Attempts**: If a developer tries to directly reference private components or logic from another feature module, the build or lint step must fail with `boundaries/element-types`.
- **Missing Environment Variables**: The project must include `.env.example` committed with documentation for all required variables (`NEXT_PUBLIC_API_BASE_URL`, `OPENAPI_URL`), while ignoring `.env*.local`.
- **OpenAPI Codegen Failure**: If the backend OpenAPI endpoint is unavailable, `pnpm generate-api` fails with a descriptive network error and leaves the previously generated `schema.d.ts` untouched.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The frontend MUST be initialized as a **Next.js 15+** application with TypeScript in strict mode (`"strict": true`) and App Router enabled, using **pnpm** as the sole package manager (`pnpm-lock.yaml`).
- **FR-002**: The project MUST live in the `frontend/` subfolder of the monorepo root, coexisting alongside `backend/`.
- **FR-003**: The architecture MUST follow **Screaming Architecture integrated with Next.js App Router file-system routing**: domain modules hang directly under `src/app/` as direct route segments (`src/app/pqrsdf/`, `src/app/auth/`, `src/app/dashboard/`), mapping 1:1 to public browser paths (`/pqrsdf/...`, `/auth/...`, `/dashboard/...`). Generic root `/components`, `/hooks`, or `/pages` folders outside of domain routes or `src/shared/` are strictly prohibited.
- **FR-004**: ESLint MUST be configured with `eslint-plugin-boundaries` to enforce module encapsulation, preventing cross-imports between domain modules while allowing imports from `src/shared/`.
- **FR-005**: Tailwind CSS MUST be configured as the styling system targeting a fixed institutional light theme for components and layouts in this initial setup. Dark mode variants and toggles are out of scope for this baseline.
- **FR-006**: Global Error Boundaries (`src/app/error.tsx`) and not-found screens (`src/app/not-found.tsx`) MUST be implemented in Spanish with user-friendly recovery controls. External error tracking services are out of scope for this initial setup.
- **FR-007**: The frontend MUST implement a zero-dependency `Result<T, E>` pattern (`Ok<T> | Err<E>`) in `src/shared/types/result.ts` with helpers `ok()`, `err()`, `isOk()`, `isErr()`, prohibiting exceptions for expected operational outcomes.
- **FR-008**: The frontend MUST configure on-demand OpenAPI client generation using `openapi-typescript` targeting `OPENAPI_URL`, outputting types to `src/shared/api/generated/schema.d.ts`, with a typed client in `src/shared/api/client.ts` using `openapi-fetch`.
- **FR-009**: The frontend MUST provide reusable base and layout components in `src/shared/components/`: `LoadingSpinner` (accessible loading indicator), `EmptyState` (empty state display), and global layout components, with all user-facing labels in Spanish.
- **FR-010**: All source code (identifiers, files, types, variables, comments) MUST be in **English**.
- **FR-011**: The frontend MUST integrate **next-intl** for internationalization (i18n), maintaining a central messages catalog in Spanish (`messages/es.json`) with `es` as the default locale, ensuring all UI labels, navigation titles, and feedback messages are localized and decoupled from component markup in Spanish per constitutional mandates.
- **FR-012**: The project MUST configure **Vitest** with React Testing Library and JSDOM for unit and component integration testing, including Next.js navigation mocks and native Vitest mocking conventions (`vi.mock`) for `@/shared/api/client`.
- **FR-013**: TypeScript path aliases MUST be configured in `tsconfig.json` strictly for `@/app/*` (mapping to `src/app/*`) and `@/shared/*` (mapping to `src/shared/*`). Deep relative imports (`../../`) across modules are prohibited.
- **FR-014**: Next.js HTTP security headers MUST be configured in `next.config.ts`, including Content Security Policy (CSP), X-Frame-Options (`DENY`), X-Content-Type-Options (`nosniff`), Referrer-Policy, and Permissions-Policy.
- **FR-015**: Prettier MUST be configured as the automated code formatter, integrated with ESLint via `eslint-config-prettier`.
- **FR-016**: A `.env.example` file MUST be committed documenting all required variables, while `.env*.local` files are excluded by `.gitignore`.
- **FR-017**: A `README.md` file MUST be provided in `frontend/` containing at least: Prerequisites (Node.js 20+, pnpm 9+), Installation instructions (< 5 min), and an Available Commands table.

### Key Entities

- **Result<T, E>**: Discriminated union type (`{ ok: true, value: T } | { ok: false, error: E }`).
- **ApiError**: Standardized HTTP error contract (`{ message: string, status: number, code?: string, details?: unknown }`).
- **LoadingSpinner**: Reusable accessible UI loading component.
- **EmptyState**: Reusable UI component for empty data states.
- **OpenAPI Schema**: Auto-generated type contract (`schema.d.ts`) defining paths, endpoints, and DTO schemas.

---

## Success Criteria *(mandatory)*

- **SC-001**: A new developer can set up and run the frontend in under 5 minutes following only the `README.md` (`pnpm install && pnpm dev`).
- **SC-002**: TypeScript compiler (`pnpm typecheck`) completes with 0 errors in strict mode (`strict: true`).
- **SC-003**: ESLint (`pnpm lint`) passes with 0 errors and 0 warnings across the entire codebase.
- **SC-004**: 100% of user-facing UI labels, messages, and placeholders are in Spanish, and 100% of source code is in English.
- **SC-005**: Global Error Boundary contingency screen renders in under 2 seconds upon simulating an unhandled rendering error.
- **SC-006**: OpenAPI client generation (`pnpm generate-api`) completes in under 60 seconds when the backend specification is available.
- **SC-007**: 0 boundary violations exist in the initial codebase, and any cross-domain import attempt is caught by ESLint.
- **SC-008**: Automated test suite (`pnpm test`) runs with 100% passing tests (minimum 1 unit test and 1 component test).
- **SC-009**: HTTP security headers (CSP, X-Frame-Options, X-Content-Type-Options) are present on all application responses.

---

## Assumptions

- The frontend lives in the `frontend/` subfolder of the PQRSDF monorepo, in parallel to `backend/`, without shared workspace tooling.
- Developers use Node.js 20 LTS and pnpm 9+.
- The backend ASP.NET Core API exposes its OpenAPI spec at `http://localhost:5023/openapi.json` during local development.
- Authentication and business workflow UI (creating tickets, tracking) are deferred to dedicated future feature specifications.
- Form management and validation libraries (e.g. React Hook Form, Zod) are deferred to subsequent domain features that implement interactive user forms.
- Git hooks (Husky, lint-staged) are excluded from the local setup; verification is enforced via runnable pnpm scripts and CI/CD quality gates.
- Production build validation (`next build`) and CI/CD pipeline configuration are handled in a subsequent dedicated CI/CD feature.
