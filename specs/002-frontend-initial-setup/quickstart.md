# Quickstart & Verification Guide: Frontend Initial Setup

**Feature**: Frontend Initial Setup with Next.js App Router and Screaming Architecture  
**Specification**: [specs/002-frontend-initial-setup/spec.md](file:///home/luis/Documents/Mi%20universidad/Especialización/Segundo%20semestre/arquitectura%20web/pqrsdf-software/specs/002-frontend-initial-setup/spec.md)  
**Date**: 2026-09-18  

---

## Prerequisites

- **Node.js**: Version `20.x` LTS or higher (`node -v`)
- **pnpm**: Version `9.x` or higher (`pnpm -v`)
- Working Directory: `frontend/` (located alongside `backend/`)

---

## 1. Setup & Installation (< 5 minutes)

From the `frontend/` directory:

```bash
# 1. Install all dependencies strictly via pnpm
pnpm install

# 2. Copy example environment configuration
cp .env.example .env.local
```

Expected Outcome: Dependencies resolve and install clean into `node_modules` with lockfile `pnpm-lock.yaml`.

---

## 2. Verification Scenarios

### Scenario 1: TypeScript Strict Typecheck
Verifies zero type errors and adherence to TypeScript strict mode.

```bash
pnpm typecheck
```
- **Expected Outcome**: `tsc --noEmit` completes with exit code 0 and 0 errors.

---

### Scenario 2: ESLint Architectural Boundaries & Prettier Formatter
Verifies that no cross-feature import violations exist and code adheres to style guidelines.

```bash
pnpm lint
pnpm format:check
```
- **Expected Outcome**: ESLint passes with 0 warnings and 0 errors. Prettier confirms all files are properly formatted.

---

### Scenario 3: Automated Test Suite (Vitest)
Executes unit and component integration tests with React Testing Library and JSDOM.

```bash
pnpm test
```
- **Expected Outcome**: All tests pass (100% success rate), validating:
  1. `Result<T, E>` monad and helper guards (`ok`, `err`, `isOk`, `isErr`).
  2. `LoadingSpinner` and `EmptyState` component rendering and accessibility.
  3. API client error mapping.

---

### Scenario 4: Local Development Server Execution
Verifies that Next.js App Router compiles and serves the application.

```bash
pnpm dev
```
- **Verification Steps**:
  1. Navigate to `http://localhost:3000`: Portal landing page renders in Spanish.
  2. Navigate to `http://localhost:3000/pqrsdf`: Domain route resolves with Spanish placeholder.
  3. Navigate to `http://localhost:3000/auth`: Auth domain route resolves.
  4. Navigate to `http://localhost:3000/dashboard`: Dashboard domain route resolves.
  5. Navigate to `http://localhost:3000/non-existent-route`: 404 Not Found screen renders in Spanish with a link to return home.

---

### Scenario 5: On-Demand OpenAPI Client Generation
Verifies that client types can be regenerated when backend is running.

```bash
pnpm generate-api
```
- **Expected Outcome**: Generates or updates `src/shared/api/generated/schema.d.ts` without throwing unhandled script crashes. If backend is offline, script logs a descriptive network error without corrupting existing schema.
