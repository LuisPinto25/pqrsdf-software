# Phase 0: Technical Research & Architectural Decisions

**Feature**: Frontend Initial Setup with Next.js App Router and Screaming Architecture  
**Specification**: [specs/002-frontend-initial-setup/spec.md](file:///home/luis/Documents/Mi%20universidad/Especialización/Segundo%20semestre/arquitectura%20web/pqrsdf-software/specs/002-frontend-initial-setup/spec.md)  
**Date**: 2026-09-18  
**Status**: Completed  

---

## 1. Screaming Architecture Integrated with Next.js 15 App Router

### Decision
Structure domain features directly as route segments under `src/app/` (`src/app/pqrsdf/`, `src/app/auth/`, `src/app/dashboard/`), mapping 1:1 to browser URLs (`/pqrsdf`, `/auth`, `/dashboard`). Generic root-level technical folders outside of domain routes or `src/shared/` (e.g. `/components`, `/hooks`, `/pages`) are strictly prohibited per Constitution v1.3.0 (Principle IV).

### Rationale
- Next.js App Router relies on file-system routing. Placing feature domain folders directly inside `src/app/` simultaneously satisfies Screaming Architecture and Next.js routing requirements without artificial indirection.
- Allows co-location of domain-specific components, custom hooks, and route handlers directly within their respective domain folders (`src/app/pqrsdf/components/`, etc.).
- Leverages native App Router capabilities: nested layouts (`layout.tsx`), loading skeletons (`loading.tsx`), and error boundaries (`error.tsx`) isolated per feature domain.

### Alternatives Considered
- **Option A: Root `src/features/` with route redirects or re-exports in `src/app/`**: Creates unnecessary duplication where every route file in `src/app/` is a boilerplate 2-line re-export of a component in `src/features/`. Violates simplicity and confuses Next.js file conventions.
- **Option B: Route Groups `src/app/(features)/...`**: Adds syntax noise without architectural benefit, since `/pqrsdf`, `/auth`, and `/dashboard` are distinct public route segments that naturally scream the business domain.

---

## 2. Module Boundary Enforcement (ESLint)

### Decision
Use `eslint-plugin-boundaries` paired with `eslint-import-resolver-typescript` within the ESLint flat config (`eslint.config.mjs`) to enforce structural encapsulation and prevent cross-feature imports.

### Configuration Strategy
- **Element Types**:
  - `domain`: `src/app/(*)/` (captures `domainName` = `pqrsdf`, `auth`, `dashboard`)
  - `shared`: `src/shared/**/*`
  - `root-app`: `src/app/*` (root layout, error, not-found)
- **Dependency Rules**:
  - `domain` may import from `shared` and from itself (`domainName: "${from.domainName}"`).
  - `domain` **cannot** import from another `domain` (e.g. `src/app/auth` cannot import from `src/app/pqrsdf`).
  - `shared` **cannot** import from `domain` or `root-app` (strictly unidirectional dependency flow).
  - `root-app` may import from `shared` and `domain`.

### Rationale
- `eslint-plugin-boundaries` supports dynamic capture tokens (`capture: ['domainName']`), meaning new domains added in the future are automatically protected without updating ESLint rules.
- Prevents spaghetti dependencies and coupling between decoupled business modules.

### Alternatives Considered
- **`import/no-restricted-paths` (`eslint-plugin-import`)**: Requires $O(N^2)$ manual zone pairing whenever a new domain is introduced.
- **`no-restricted-imports` (ESLint built-in)**: Operates only on raw literal strings, unaware of filesystem context or internal vs external module paths.

---

## 3. Zero-Dependency Result Pattern in TypeScript

### Decision
Implement a pure, zero-dependency `Result<T, E>` type in `src/shared/types/result.ts` utilizing TypeScript discriminated unions with helper functions `ok()`, `err()`, `isOk()`, and `isErr()`.

### Implementation Specification
```typescript
export type Ok<T> = { readonly ok: true; readonly value: T };
export type Err<E> = { readonly ok: false; readonly error: E };
export type Result<T, E = Error> = Ok<T> | Err<E>;

export const ok = <T>(value: T): Ok<T> => ({ ok: true, value });
export const err = <E>(error: E): Err<E> => ({ ok: false, error });
export const isOk = <T, E>(result: Result<T, E>): result is Ok<T> => result.ok;
export const isErr = <T, E>(result: Result<T, E>): result is Err<E> => !result.ok;
```

### Rationale
- Aligns with Constitution Principle V: Prohibits throwing exceptions for expected business or operational failures.
- Zero runtime overhead and zero third-party dependencies (eliminates bloat from libraries like `neverthrow` or `fp-ts`).
- Fully type-safe and idiomatic with TypeScript pattern matching and control flow narrowing (`if (isOk(res)) ... else ...`).

---

## 4. OpenAPI Typed Client Generation

### Decision
Use `openapi-typescript` for generating static TypeScript type definitions (`schema.d.ts`) and `openapi-fetch` for executing strongly typed HTTP requests. The generator script is executed **on-demand** via `pnpm generate-api`.

### Rationale
- `openapi-typescript` generates pure TypeScript types (`.d.ts`), resulting in **0 KB runtime bundle overhead**.
- `openapi-fetch` is ultralight (< 5 KB), native fetch-based, and provides compile-time type-checking for endpoints, params, and response bodies based on `schema.d.ts`.
- Running on-demand (rather than during `pnpm dev` or `pnpm build`) ensures that frontend development and CI/CD pipelines can run reliably even if the backend is offline or unreachable.

### Alternatives Considered
- **Orval / RTK Query Codegen**: Generates heavy client classes, hooks, and runtime code, introducing framework lock-in and large bundle sizes.
- **`@hey-api/openapi-ts`**: Powerful but introduces additional plugins and runtime configuration unnecessary for this MVP.

---

## 5. Internationalization (i18n) with `next-intl`

### Decision
Integrate `next-intl` configured for Next.js 15 App Router with a centralized Spanish message dictionary (`messages/es.json`) and `src/i18n/request.ts` using `getRequestConfig`.

### Architecture
- **Locale**: Spanish (`es`) as the default and single locale for this baseline (no localized URL prefix `/es/...` needed).
- **Configuration**:
  - `src/i18n/request.ts`: Uses `getRequestConfig` from `next-intl/server` to dynamically load `messages/es.json`.
  - `next.config.ts`: Wrapped with `createNextIntlPlugin()`.
  - `src/types/global.d.ts`: Augments `next-intl` `AppConfig` with `Messages: typeof es` for compile-time key autocompletion.
  - `src/app/layout.tsx`: Async Server Component providing messages via `<NextIntlClientProvider messages={messages}>`.
  - Server Components use `await getTranslations('Namespace')`.
  - Client Components use `useTranslations('Namespace')`.

### Rationale
- Fulfills Constitution Principle VII: 100% of user-facing UI labels, error messages, and placeholders in Spanish, completely decoupled from component markup.
- Provides type-safe message keys, preventing typos and missing translation strings.

---

## 6. Unit and Component Testing with Vitest

### Decision
Configure **Vitest** with JSDOM and React Testing Library (`@testing-library/react`, `@testing-library/jest-dom`) in `vitest.config.ts`, utilizing native Vitest mocking (`vi.mock`) for `@/shared/api/client` and Next.js navigation mocks.

### Setup Details
- **Test Environment**: `jsdom` with `globals: true` and setup file `tests/setup.ts`.
- **Path Resolution**: `vite-tsconfig-paths` to seamlessly resolve `@/app/*` and `@/shared/*` aliases in tests.
- **Mocking**: Vitest's `vi.mock('next/navigation')` to simulate `useRouter`, `usePathname`, `useSearchParams`.

### Rationale
- Vitest executes substantially faster than Jest (Vite-based ESM pipeline, shared config, parallel execution).
- Native TypeScript support without complex `babel-jest` or `ts-jest` transformations.

---

## 7. HTTP Security Headers and CSP

### Decision
Configure robust HTTP security headers in `next.config.ts` via the `headers()` method:
- **Content-Security-Policy (CSP)**: Allows self scripts, Next.js inline scripts for hydration (`'unsafe-inline'`), styles (`'unsafe-inline'`), images (`'self'`, data:, https:), and connects to `http://localhost:5023` in development.
- **X-Frame-Options**: `DENY` (prevents clickjacking).
- **X-Content-Type-Options**: `nosniff` (prevents MIME sniffing).
- **Referrer-Policy**: `strict-origin-when-cross-origin`.
- **Permissions-Policy**: `camera=(), microphone=(), geolocation=()`.

### Rationale
- Adheres to standard institutional security guidelines (OWASP Top 10) and prevents browser-side exploitation vectors.

---

## 8. Styling and Theming with Tailwind CSS

### Decision
Configure **Tailwind CSS** targeting a fixed institutional light palette without dark mode toggles or dark variants in this baseline setup.

### Rationale
- Matches institutional government portal branding.
- Keeps styling predictable and free from premature dark mode complexity while leaving room for future theme extensions.
