# Implementation Plan: Frontend Initial Setup with Next.js App Router and Screaming Architecture

**Branch**: `002-frontend-initial-setup` | **Date**: 2026-09-18 | **Spec**: [spec.md](file:///home/luis/Documents/Mi%20universidad/Especialización/Segundo%20semestre/arquitectura%20web/pqrsdf-software/specs/002-frontend-initial-setup/spec.md)

**Input**: Feature specification from `specs/002-frontend-initial-setup/spec.md`

## Summary

Establish the initial frontend architectural baseline for the PQRSDF management system using Next.js 15+ (App Router) in `frontend/`, configured strictly with TypeScript in strict mode (`"strict": true`), Tailwind CSS, and `pnpm`. The architecture integrates Screaming Architecture directly with Next.js file-system routing under `src/app/` (`src/app/pqrsdf/`, `src/app/auth/`, `src/app/dashboard/`), strictly banning generic root technical folders (`/components`, `/hooks`, `/pages`). Module boundaries are enforced via `eslint-plugin-boundaries`, operational error flows follow a zero-dependency `Result<T, E>` pattern, HTTP API contracts are generated on-demand via `openapi-typescript` targeting ASP.NET Core OpenAPI endpoints, automated testing is powered by Vitest, and user-facing text is localized into Spanish via `next-intl` (`messages/es.json`) while maintaining 100% English source code.

## Technical Context

**Language/Version**: TypeScript 5.x / Node.js 20+ LTS  
**Primary Dependencies**: Next.js 15+, React 19+, Tailwind CSS 3.4+, `next-intl` 3.x, `openapi-fetch` 0.13+, `openapi-typescript` 7.x (dev)  
**Storage**: Browser LocalStorage / SessionStorage / Cookies (runtime state; no direct database access in frontend)  
**Testing**: Vitest 2.x, React Testing Library (`@testing-library/react`), JSDOM, `@testing-library/jest-dom`, `vite-tsconfig-paths`  
**Target Platform**: Modern Web Browsers (Chrome, Firefox, Safari, Edge) via Node.js Next.js Server  
**Project Type**: Web Application (Client-Side & Server-Rendered UI with Next.js App Router)  
**Performance Goals**: Initial development server startup < 5 minutes; local page rendering & contingency screens < 2 seconds; OpenAPI codegen < 60 seconds  
**Constraints**: Zero cross-domain imports between `src/app/auth` and `src/app/pqrsdf`; 0 TypeScript errors (`strict: true`); 0 ESLint warnings; 100% Spanish user interface; 100% English source code identifiers; HTTP security headers enforced on all responses  
**Scale/Scope**: Initial setup establishing the foundational frontend monorepo package with 3 core domain routing segments, shared component foundations, typed API infrastructure, and quality gates  

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Rule | Compliance Status | Analysis & Justification |
|---|---|---|
| **I. Stack Tecnológico & Arquitectura Global** | PASS | Frontend resides in `frontend/` using Next.js 15+ App Router, TypeScript strict mode, communicating strictly via RESTful JSON with backend ASP.NET Core 10. |
| **II. Backend Clean Architecture** | N/A | Frontend baseline does not alter backend Clean Architecture layers. |
| **III. DDD & Tipos Fuertes** | PASS | TypeScript strong typing enforced (`strict: true`, no `any`), custom discriminated union `Result<T, E>`, and typed DTOs from OpenAPI `schema.d.ts`. |
| **IV. Screaming Architecture & Next.js App Router** | PASS | Features hang directly under `src/app/` (`src/app/pqrsdf/`, `src/app/auth/`, `src/app/dashboard/`), mapping 1:1 to browser routes. Generic root `/components`, `/hooks`, `/pages` prohibited. Reusable primitives quarantined in `src/shared/`. ESLint boundaries prevent cross-feature imports. |
| **V. Gobernanza de Errores y Excepciones** | PASS | Zero-dependency `Result<T, E>` pattern implemented in `src/shared/types/result.ts` with `ok`, `err`, `isOk`, `isErr`. Unhandled exceptions caught by Next.js global error boundaries (`src/app/error.tsx`). |
| **VI. SOLID & Prevención de Code Smells** | PASS | Modular single-responsibility components; no God Objects; interface segregation across contracts; boundaries enforced by linter. |
| **VII. Idioma y Localización** | PASS | All source code, types, and comments written strictly in English. All UI labels, placeholders, navigation items, and error messages localized in Spanish via `next-intl` (`messages/es.json`). |

## Project Structure

### Documentation (this feature)

```text
specs/002-frontend-initial-setup/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
│   ├── api-client.md
│   ├── i18n.md
│   ├── result-type.md
│   └── shared-components.md
├── checklists/
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Phase 2 output (/speckit-tasks command)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Domain/
│   ├── Application/
│   ├── Infrastructure/
│   └── API/
└── tests/

frontend/
├── messages/
│   └── es.json                        # Central Spanish messages catalog
├── public/
│   └── favicon.ico
├── src/
│   ├── app/                           # Screaming Architecture + App Router
│   │   ├── auth/                      # Authentication domain route segment
│   │   │   └── page.tsx               # Minimal Spanish placeholder
│   │   ├── dashboard/                 # Dashboard domain route segment
│   │   │   └── page.tsx               # Minimal Spanish placeholder
│   │   ├── pqrsdf/                    # PQRSDF domain route segment
│   │   │   └── page.tsx               # Minimal Spanish placeholder
│   │   ├── error.tsx                  # Global error boundary (Spanish)
│   │   ├── globals.css                # Tailwind CSS imports & base styles
│   │   ├── layout.tsx                 # Root layout with NextIntlClientProvider
│   │   ├── not-found.tsx              # Global 404 page (Spanish)
│   │   └── page.tsx                   # Landing home page (Spanish)
│   ├── i18n/
│   │   └── request.ts                 # next-intl request config (getRequestConfig)
│   ├── shared/                        # Cross-cutting foundational resources
│   │   ├── api/
│   │   │   ├── client.ts              # Typed openapi-fetch wrapper
│   │   │   └── generated/
│   │   │       └── schema.d.ts        # Generated OpenAPI TypeScript types
│   │   ├── components/
│   │   │   ├── EmptyState.tsx         # Accessible empty state component
│   │   │   └── LoadingSpinner.tsx     # Accessible loading indicator
│   │   └── types/
│   │       ├── api.ts                 # ApiError interface
│   │       └── result.ts              # Zero-dependency Result<T, E> monad
│   └── types/
│       └── global.d.ts                # next-intl AppConfig type augmentation
├── tests/
│   ├── setup.ts                       # Vitest matchers and global mocks
│   ├── unit/
│   │   └── result.test.ts             # Tests for Result<T, E> helpers
│   └── components/
│       └── LoadingSpinner.test.tsx    # RTL component test
├── .env.example                       # Committed environment template
├── .gitignore                         # Excludes node_modules, .next, .env*.local
├── eslint.config.mjs                  # ESLint flat config with boundaries rules
├── next.config.ts                     # Next.js config with next-intl & CSP headers
├── package.json                       # Scripts, dependencies, metadata
├── pnpm-lock.yaml                     # Locked pnpm dependency tree
├── postcss.config.mjs                 # PostCSS config for Tailwind
├── tailwind.config.ts                 # Tailwind config (light institutional theme)
├── tsconfig.json                      # Strict TypeScript with @/app/* & @/shared/*
└── vitest.config.ts                   # Vitest configuration with JSDOM
```

**Structure Decision**: Selected Web application monorepo structure co-locating `frontend/` alongside `backend/`. Within `frontend/src/`, business domains reside directly in `app/` per Constitution v1.3.0 Principle IV, while shared resources are housed in `shared/`. Path aliases strictly map `@/app/*` to `src/app/*` and `@/shared/*` to `src/shared/*`.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

*No constitutional violations identified. The architecture strictly adheres to Constitution v1.3.0.*
