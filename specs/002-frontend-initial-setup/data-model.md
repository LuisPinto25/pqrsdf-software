# Phase 1: Data Model & TypeScript Entities

**Feature**: Frontend Initial Setup with Next.js App Router and Screaming Architecture  
**Specification**: [specs/002-frontend-initial-setup/spec.md](file:///home/luis/Documents/Mi%20universidad/Especialización/Segundo%20semestre/arquitectura%20web/pqrsdf-software/specs/002-frontend-initial-setup/spec.md)  
**Date**: 2026-09-18  
**Status**: Completed  

---

## 1. Result Pattern Entities (`src/shared/types/result.ts`)

Encapsulates success and error outcomes as a discriminated union, eliminating unhandled runtime exceptions for expected failures.

```typescript
/**
 * Represents a successful operation holding a value of type T.
 */
export type Ok<T> = {
  readonly ok: true;
  readonly value: T;
};

/**
 * Represents a failed operation holding an error of type E.
 */
export type Err<E> = {
  readonly ok: false;
  readonly error: E;
};

/**
 * Result monad representing either success (Ok<T>) or failure (Err<E>).
 */
export type Result<T, E = Error> = Ok<T> | Err<E>;
```

### Invariants & Validation Rules
- `ok` is an immutable boolean discriminator (`true` for `Ok`, `false` for `Err`).
- When `ok: true`, property `value` is guaranteed present and type-safe.
- When `ok: false`, property `error` is guaranteed present and type-safe.
- Direct property access on `result.value` without checking `result.ok` is blocked by the TypeScript compiler.

---

## 2. API Error Contract (`src/shared/types/api.ts`)

Standardized HTTP error entity representing client, network, and server errors returned by `apiClient`.

```typescript
export interface ApiError {
  /**
   * HTTP status code (e.g. 400, 401, 404, 500).
   * Status 0 indicates client-side network failure or timeout.
   */
  readonly status: number;

  /**
   * User-facing error message localized in Spanish.
   */
  readonly message: string;

  /**
   * Machine-readable error code (e.g., 'VALIDATION_ERROR', 'NETWORK_OFFLINE').
   */
  readonly code?: string;

  /**
   * Optional validation details or backend ProblemDetails payload.
   */
  readonly details?: Record<string, unknown> | unknown[];
}
```

### Invariants & Validation Rules
- `status` MUST be a non-negative integer.
- `message` MUST be non-empty and formatted in Spanish for direct user presentation.

---

## 3. Shared UI Component Entities (`src/shared/components/`)

### LoadingSpinnerProps (`src/shared/components/LoadingSpinner.tsx`)

```typescript
export type SpinnerSize = 'sm' | 'md' | 'lg';

export interface LoadingSpinnerProps {
  /**
   * Visual size of the spinner.
   * Default: 'md'
   */
  readonly size?: SpinnerSize;

  /**
   * Accessible text read by screen readers.
   * Default: 'Cargando contenido...'
   */
  readonly label?: string;

  /**
   * Optional additional CSS classes for styling.
   */
  readonly className?: string;
}
```

### EmptyStateProps (`src/shared/components/EmptyState.tsx`)

```typescript
export interface EmptyStateProps {
  /**
   * Title text displayed in bold.
   */
  readonly title: string;

  /**
   * Informative description explaining why no items are available.
   */
  readonly description?: string;

  /**
   * Optional action button configuration.
   */
  readonly action?: {
    readonly label: string;
    readonly onClick: () => void;
  };

  /**
   * Optional icon or graphic slot.
   */
  readonly icon?: React.ReactNode;

  /**
   * Optional container styling classes.
   */
  readonly className?: string;
}
```

---

## 4. OpenAPI Contract Representation (`src/shared/api/generated/schema.d.ts`)

Contract types generated dynamically via `openapi-typescript` from the backend OpenAPI specification.

```typescript
/**
 * Root interface representing OpenAPI paths, components, and schemas.
 * Generated automatically by `pnpm generate-api`.
 */
export interface paths {
  [path: string]: {
    get?: { responses: { 200: { content: { "application/json": unknown } } } };
    post?: { responses: { 200: { content: { "application/json": unknown } } } };
    // Other HTTP verbs
  };
}

export interface components {
  schemas: {
    [name: string]: unknown;
  };
}
```

---

## 5. Message Catalog Entity (`messages/es.json`)

Centralized Spanish translations dictionary structured by namespaces.

```typescript
export interface MessagesCatalog {
  Common: {
    title: string;
    loading: string;
    retry: string;
    unexpectedError: string;
    offlineError: string;
  };
  Navigation: {
    home: string;
    pqrsdf: string;
    auth: string;
    dashboard: string;
  };
  Pqrsdf: {
    title: string;
    description: string;
    statusPlaceholder: string;
  };
  Auth: {
    title: string;
    description: string;
  };
  Dashboard: {
    title: string;
    description: string;
  };
  NotFound: {
    title: string;
    description: string;
    backHome: string;
  };
  ErrorBoundary: {
    title: string;
    description: string;
    retryAction: string;
  };
}
```
