# Implementation Plan: User Authentication & Role-Based Access Control

**Branch**: `005-auth-user` | **Date**: 2026-09-18 | **Spec**: [specs/005-auth-user/spec.md](spec.md)

**Input**: Feature specification from `/specs/005-auth-user/spec.md`

---

## Summary

Implement the user authentication and role-based access control module for internal staff (`Funcionario` and `Administrador`). The backend exposes a secure login endpoint (`POST /api/v1/auth/login`) with IP-based rate limiting (5 attempts/minute), verifies credentials using BCrypt password hashing, and generates signed JSON Web Tokens (JWT) valid for 8 hours with user claims (`sub`, `email`, `name`, `role`). ASP.NET Core API routes are protected using standard JWT Bearer authentication (`[Authorize]`), ensuring that management operations require valid tokens while citizen operations (ticket submission and public consultation) remain accessible without authentication. On the frontend, Next.js App Router Screaming Architecture is implemented at `src/app/auth/`, storing the JWT in a client cookie enabling `middleware.ts` to enforce server-side route guarding on protected sections (`/dashboard`) and redirect unauthenticated visitors to `/auth?returnUrl=...`.

---

## Technical Context

**Language/Version**: 
- Backend: C# 14 / .NET 10 (ASP.NET Core 10) with `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- Frontend: TypeScript 5.7+ (strict mode enabled) with Next.js 15+ App Router.

**Primary Dependencies**:
- Backend: `BCrypt.Net-Next` (4.x), `Microsoft.AspNetCore.Authentication.JwtBearer` (10.x), `System.IdentityModel.Tokens.Jwt` (8.x), `Microsoft.EntityFrameworkCore.SqlServer` (10.x), `MediatR` (14.x).
- Frontend: `next-intl` (3.x), `lucide-react`, Tailwind CSS (3.x).

**Storage**: Microsoft SQL Server (single unified relational database for reads and writes; no secondary databases or message brokers).

**Testing**: 
- Backend: xUnit, FluentAssertions, Moq, NetArchTest.
- Frontend: Vitest, React Testing Library, JSDOM.

**Target Platform**: Linux / Cross-platform server environment (.NET 10 Runtime) and modern desktop/mobile web browsers.

**Project Type**: Decoupled Client-Server Web Application (Clean Architecture ASP.NET Core Web API + Next.js App Router).

**Performance Goals**:
- Login credential authentication and token issuance latency < 2 seconds.
- IP rate limiting enforced at 5 login attempts/minute per IP address returning HTTP 429 upon threshold breach.
- Instant client-side validation on email and password fields.

**Constraints**:
- Clean Architecture concentric layers with unidirectional dependencies (`Domain` <- `Application` <- `Infrastructure` / `Api`).
- CQRS command isolation under `Application/Features/Auth/UseCases/LoginUser/`.
- Frontend Screaming Architecture co-located in `src/app/auth/` (no generic root technical folders).
- 100% of source code, models, database schemas, and commits strictly in English.
- 100% of user-facing UI labels, form fields, validation feedback, and alerts strictly in Spanish.

**Scale/Scope**: Internal staff and administrator authentication module.

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle / Gate | Status | Justification / Verification |
| :--- | :---: | :--- |
| **I. Stack Tecnológico (ASP.NET Core 10 + Next.js)** | ✅ PASS | Backend uses ASP.NET Core 10; frontend uses Next.js 15 App Router with TypeScript in strict mode. |
| **II. Clean Architecture (4 capas concéntricas)** | ✅ PASS | Decoupled projects (`Domain`, `Application`, `Infrastructure`, `Api`). Pure POCO domain (`User`). DIP enforced via `IPasswordHasher`, `IJwtTokenGenerator`, `IUserRepository`. |
| **II. CQRS Organizativo Co-ubicado** | ✅ PASS | Command isolated under `Application/Features/Auth/UseCases/LoginUser/` with co-located `LoginUserCommand`, `LoginUserCommandHandler`, and unique DTOs. |
| **II. Base de Datos Única (Sin Sobreingeniería)** | ✅ PASS | Single SQL Server database (`Users` table alongside PQRSDF entities); no dual databases or event brokers. |
| **III. DDD & Tipos Fuertes** | ✅ PASS | Non-anemic `User` aggregate root with domain methods (`RecordLogin`, `Deactivate`), strongly typed Value Objects (`Email`, `PasswordHash`), and `UserRole` enum. |
| **IV. Frontend Screaming Architecture (App Router)** | ✅ PASS | Auth views co-located in `src/app/auth/` per Constitution v1.3.0 Principle IV; route protection via Next.js `middleware.ts`. |
| **V. Gobernanza de Errores (Result Pattern & Global Exception)** | ✅ PASS | Command Handler returns explicit `Result<LoginUserResponse, Error>`; API translates to HTTP 200/400/401 via `ApiControllerBase.HandleResult()`; exceptions intercepted by `GlobalExceptionHandler`. |
| **VI. SOLID & Prevención de Code Smells** | ✅ PASS | Single responsibility for command handler; password hashing and token generation decoupled behind specialized abstractions. |
| **VII. Convención de Idioma** | ✅ PASS | Code, classes, and database schemas in English; all staff-facing UI messages, validation feedback, and error alerts strictly in Spanish. |

---

## Project Structure

### Documentation (this feature)

```text
specs/005-auth-user/
├── checklists/
│   └── requirements.md                # Spec quality checklist (16/16 passing)
├── contracts/
│   └── login-contract.json            # API contract for POST /api/v1/auth/login
├── data-model.md                      # Domain model, User aggregate, Value Objects, DB schema, seed data
├── plan.md                            # This implementation plan
├── quickstart.md                      # Verification scenarios and quickstart guide
├── research.md                        # Technical decisions (BCrypt, JWT, CQRS, Next.js route protection, rate limit)
└── spec.md                            # Feature specification with 5 clarified questions
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   └── User.cs                           # Aggregate root with business methods and invariants
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs                          # Email value object (validated, lowercase)
│   │   │   └── PasswordHash.cs                   # Password hash value object
│   │   ├── Enums/
│   │   │   └── UserRole.cs                       # Funcionario (1), Administrador (2)
│   │   └── Repositories/
│   │       └── IUserRepository.cs                # User repository contract
│   ├── Application/
│   │   ├── Common/
│   │   │   └── Interfaces/
│   │   │       ├── IPasswordHasher.cs            # Password hashing abstraction
│   │   │       └── IJwtTokenGenerator.cs         # JWT token generator abstraction
│   │   ├── Features/
│   │   │   └── Auth/
│   │   │       └── UseCases/
│   │   │           └── LoginUser/
│   │   │               ├── LoginUserCommand.cs
│   │   │               ├── LoginUserCommandHandler.cs
│   │   │               ├── LoginUserRequestDto.cs
│   │   │               └── LoginUserResponse.cs
│   │   └── Shared/
│   │       └── Dtos/
│   │           └── UserProfileDto.cs             # Reusable user profile DTO
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs                   # Added DbSet<User>
│   │   │   ├── Configurations/
│   │   │   │   └── UserConfiguration.cs          # EF Core entity mapping + seed accounts
│   │   │   ├── Repositories/
│   │   │   │   └── UserRepository.cs             # EF Core implementation of IUserRepository
│   │   │   └── Migrations/                       # EF Core migration for Users table and seed data
│   │   └── Security/
│   │       ├── BCryptPasswordHasher.cs           # BCrypt.Net-Next implementation
│   │       └── JwtTokenGenerator.cs              # System.IdentityModel.Tokens.Jwt implementation
│   └── Api/
│       ├── Controllers/
│       │   └── V1/
│       │       └── AuthController.cs             # POST /api/v1/auth/login with rate limiting
│       └── Program.cs                            # AddAuthentication(JwtBearer), AddRateLimiter("LoginRateLimit")
└── tests/
    ├── Application.UnitTests/
    │   └── Features/
    │       └── Auth/
    │           └── UseCases/
    │               └── LoginUserCommandHandlerTests.cs
    └── Domain.UnitTests/
        └── Entities/
            └── UserTests.cs

frontend/
├── src/
│   ├── middleware.ts                             # Next.js route guard inspecting auth_token cookie
│   ├── app/
│   │   └── auth/
│   │       ├── page.tsx                          # Login page view (/auth)
│   │       ├── components/
│   │       │   └── LoginForm.tsx                 # Login form with inputs, validation, and alerts
│   │       ├── hooks/
│   │       │   └── useAuth.ts                    # Auth state, login/logout, cookie management
│   │       └── types/
│   │           └── auth.types.ts                 # Frontend auth contract types
│   └── shared/
│       └── api/
│           └── client.ts                         # Added loginStaff(dto) and Bearer token interceptor
└── tests/
    └── app/
        └── auth/
            └── LoginForm.test.tsx                # Component and login interaction tests
```

---

## Phase 0: Outline & Research

- Completed in [research.md](research.md).
- Key technical decisions consolidated:
  1. BCrypt password hashing via `IPasswordHasher` abstraction and `BCrypt.Net-Next`.
  2. JWT token generation with HMAC-SHA256, 8 hours expiration (28,800s), and role claims (`Funcionario`, `Administrador`).
  3. Clean Architecture with CQRS command isolation under `Application/Features/Auth/UseCases/LoginUser/`.
  4. Rich `User` aggregate root adhering to DDD and Constitution Principle III.
  5. Next.js App Router Screaming Architecture at `src/app/auth/` with `middleware.ts` route protection.
  6. Native ASP.NET Core 10 Rate Limiting (5 requests/minute per client IP) on login.
  7. Pre-seeded development accounts (`admin@pqrsdf.gov.co` and `funcionario@pqrsdf.gov.co`).

---

## Phase 1: Design & Contracts

- **Data Model**: Completed in [data-model.md](data-model.md).
- **Interface Contracts**: Completed in [contracts/login-contract.json](contracts/login-contract.json).
- **Validation Guide**: Completed in [quickstart.md](quickstart.md).
- **Post-Design Constitution Check**: All 9 gates verified and passing. Ready for Phase 2 task decomposition (`/speckit-tasks`).
