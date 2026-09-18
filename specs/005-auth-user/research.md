# Research: User Authentication & Role-Based Access Control

**Feature**: `005-auth-user`  
**Date**: 2026-09-18  
**Status**: Completed  

---

## 1. Password Hashing Strategy (BCrypt)

### Context & Requirements
The specification requires validating user credentials securely using the BCrypt hashing algorithm (`POST /api/auth/login`). Passwords must never be stored in plain text or using weak hashing functions (MD5, SHA1, unsalted SHA256).

### Decision
- **Chosen Solution**: `BCrypt.Net-Next` (version 4.x) wrapped behind the domain/application abstraction `IPasswordHasher`.
- **Interface Definition** in `Application/Common/Interfaces/IPasswordHasher.cs`:
  ```csharp
  public interface IPasswordHasher
  {
      string HashPassword(string password);
      bool VerifyPassword(string password, string passwordHash);
  }
  ```
- **Implementation** in `Infrastructure/Security/BCryptPasswordHasher.cs`:
  - Uses `BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: 11)` and `BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash)`.
  - Constant-time verification prevents side-channel timing attacks.

### Rationale
- Standard, battle-tested algorithm specifically requested in stakeholder requirements.
- Clean Architecture compliance: Application layer depends solely on `IPasswordHasher` abstraction, keeping external cryptographic libraries confined to Infrastructure.

### Alternatives Considered
- *ASP.NET Core Identity PasswordHasher*: Tightly coupled to Microsoft Identity ecosystem; introduces unnecessary tables, identity options, and complex entity hierarchies for an MVP.
- *Argon2id*: Strong modern standard, but BCrypt was explicitly specified in project requirements and provides excellent security for staff accounts.

---

## 2. JWT Generation, Signing & Verification Architecture

### Context & Requirements
Upon successful credential validation, the system must generate a signed JSON Web Token (JWT) with an 8-hour lifespan (28,800 seconds) and include the user's role claim (`Funcionario` or `Administrador`), email, display name, and subject identifier. Internal endpoints must enforce Bearer token validation, while citizen endpoints remain public.

### Decision
- **Token Generator Abstraction** in `Application/Common/Interfaces/IJwtTokenGenerator.cs`:
  ```csharp
  public interface IJwtTokenGenerator
  {
      string GenerateToken(User user, TimeSpan expiration);
  }
  ```
- **Token Service Implementation** in `Infrastructure/Security/JwtTokenGenerator.cs`:
  - Generates token using `System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler`.
  - Signing Algorithm: `SecurityAlgorithms.HmacSha256`.
  - Secret key configured via `JwtSettings:Secret` in `appsettings.json` (minimum 256 bits / 32 characters).
  - Standard Claims:
    - `JwtRegisteredClaimNames.Sub`: User ID (`Guid.ToString()`).
    - `JwtRegisteredClaimNames.Email`: User email.
    - `JwtRegisteredClaimNames.Name`: User full name.
    - `ClaimTypes.Role`: `user.Role.ToString()` (`"Funcionario"` or `"Administrador"`).
    - `JwtRegisteredClaimNames.Jti`: Unique token identifier (`Guid.NewGuid()`).
- **API Authentication Pipeline** in `Api/Program.cs`:
  - `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)`
  - Validates Issuer, Audience, IssuerSigningKey, and ClockSkew (set to 0 or 1 minute).
  - Public endpoints marked with `[AllowAnonymous]`; protected endpoints marked with `[Authorize]`.

### Rationale
- Standards-compliant OAuth2/JWT Bearer scheme supported natively by ASP.NET Core and OpenAPI/Swagger.
- Decouples token creation from ASP.NET Core HTTP context, enabling pure unit testing in the Application layer.

### Alternatives Considered
- *Cookie-only session backend*: Violates API decoupled client-server architecture where third-party clients or mobile apps may query endpoints with standard Bearer tokens.
- *OAuth2 Authorization Server / IdentityServer*: Heavyweight infrastructure overhead unnecessary for a single internal tenant MVP.

---

## 3. Clean Architecture & CQRS Pattern in Application Layer

### Context & Requirements
Project Constitution Principle II mandates Clean Architecture with CQRS organizational patterns under `Application/Features/[FeatureName]/UseCases/[UseCaseName]/`, with co-located commands, handlers, and unique DTOs, and DIP enforced.

### Decision
- Feature folder: `Application/Features/Auth/UseCases/LoginUser/`
- Co-located files:
  - `LoginUserCommand.cs`: Command record holding `Email` and `Password`.
  - `LoginUserCommandHandler.cs`: Handles command execution, fetches `User` via `IUserRepository`, verifies password via `IPasswordHasher`, verifies `IsActive`, calls `IJwtTokenGenerator`, and returns `Result<LoginUserResponse>`.
  - `LoginUserResponse.cs`: Response DTO containing `AccessToken`, `TokenType`, `ExpiresIn`, and nested `UserProfileDto`.
  - `LoginUserRequestDto.cs`: Input payload for API controller.
- Shared DTOs: `UserProfileDto` placed in `Application/Shared/Dtos/UserProfileDto.cs` for cross-feature reuse (e.g., ticket assignment).

### Rationale
- Strict compliance with Constitution Principle II and VII.
- Complete isolation of login execution logic, easily testable with mock repository and hasher.

---

## 4. Domain Modeling & Value Objects (DDD)

### Context & Requirements
Constitution Principle III strictly prohibits anemic domain models and primitive obsession. Entities must encapsulate state, business rules, and invariants.

### Decision
- **`User` Aggregate Root** (`Domain/Entities/User.cs`):
  - Private setters with expressive factory methods and mutating methods (`RecordLogin()`, `Deactivate()`, `Activate()`, `ChangePassword()`).
  - Enforces invariant that email cannot be empty, password hash cannot be blank, and role must be defined.
- **Value Objects**:
  - `Email` (reused/adapted from Domain Value Objects): Validates standard email structure.
  - `PasswordHash` (`Domain/ValueObjects/PasswordHash.cs`): Wraps BCrypt hash string with non-empty invariant.
  - `UserRole` (`Domain/Enums/UserRole.cs`): Enum with `Funcionario = 1` and `Administrador = 2`.
- **Repository Interface** in `Domain/Repositories/IUserRepository.cs`:
  ```csharp
  public interface IUserRepository
  {
      Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
      Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);
      Task AddAsync(User user, CancellationToken ct = default);
      Task UpdateAsync(User user, CancellationToken ct = default);
  }
  ```

### Rationale
- Protects domain integrity; ensures user instances are always valid and business logic (e.g. login recording, deactivation) stays in the entity.

---

## 5. Client Token Storage & Next.js App Router Route Guarding

### Context & Requirements
Next.js App Router Screaming Architecture (Principle IV) requires protecting management routes (`/dashboard`, etc.) while keeping public routes (`/`, `/pqrsdf/new`, `/pqrsdf/search`, `/auth`) open. Clarification 1 selected Option A: Bearer token in response body + cookie storage.

### Decision
- **Storage**: When user logs in at `/auth`, client receives the JWT token and writes it to a standard cookie (`auth_token`, `SameSite=Lax`, `Path=/`, `Max-Age=28800`).
- **Next.js Middleware** (`frontend/src/middleware.ts`):
  - Intercepts requests to `/dashboard/:path*` and other internal routes.
  - Inspects `request.cookies.get('auth_token')`.
  - If missing, immediately redirects to `/auth?returnUrl=${encodeURIComponent(pathname)}`.
  - Public routes (`/`, `/pqrsdf/new`, `/pqrsdf/search`, `/auth`) bypass the check.
- **API Client Interceptor** (`frontend/src/shared/api/client.ts`):
  - Automatically reads `auth_token` cookie and adds `Authorization: Bearer <token>` to outbound API requests.
  - If API responds with 401 Unauthorized, clears cookie and triggers client redirection to `/auth` with session expiration message.
- **Post-Login Redirection** (Clarification 2):
  - Successfully authenticated users without a specific `returnUrl` are redirected to `/dashboard`.
  - Users with a `returnUrl` query param are forwarded to their intended destination.

### Rationale
- Next.js `middleware.ts` executes on the edge before page rendering, preventing any flash of protected UI or leaking confidential state.
- Standard cookie enables both server-side middleware and client-side fetch interceptors to operate cohesively.

---

## 6. Login Endpoint Rate Limiting (Brute-Force Mitigation)

### Context & Requirements
Clarification 3 specified an IP-based rate limit of 5 attempts per minute on `POST /api/v1/auth/login`, returning HTTP 429 Too Many Requests with an informative Spanish retry-after notice.

### Decision
- Implemented via ASP.NET Core 10 native `Microsoft.AspNetCore.RateLimiting`.
- Configured in `Program.cs`:
  ```csharp
  builder.Services.AddRateLimiter(options =>
  {
      options.AddPolicy("LoginRateLimit", httpContext =>
          RateLimitPartition.GetFixedWindowLimiter(
              partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
              factory: partition => new FixedWindowRateLimiterOptions
              {
                  PermitLimit = 5,
                  Window = TimeSpan.FromMinutes(1),
                  QueueLimit = 0
              }));
      options.OnRejected = async (context, token) =>
      {
          context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
          context.HttpContext.Response.ContentType = "application/problem+json";
          await context.HttpContext.Response.WriteAsJsonAsync(new
          {
              type = "https://tools.ietf.org/html/rfc6585#section-4",
              title = "Demasiados intentos de acceso",
              status = 429,
              detail = "Demasiados intentos de inicio de sesión fallidos. Por favor intente nuevamente en un minuto."
          }, cancellationToken: token);
      };
  });
  ```
- Controller endpoint annotated with `[EnableRateLimiting("LoginRateLimit")]`.

### Rationale
- Reuses the existing ASP.NET Core native rate limiting infrastructure introduced in Feature 004 without third-party dependencies.
- Prevents staff account lockouts from external attacks while throttling password-guessing bots.

---

## 7. Database Seeding & Development Accounts

### Context & Requirements
Clarification 4 specified two pre-seeded accounts:
1. `admin@pqrsdf.gov.co` (Role: `Administrador`, FullName: "Administrador del Sistema", Password: `Admin123*`)
2. `funcionario@pqrsdf.gov.co` (Role: `Funcionario`, FullName: "Funcionario de PQRSDF", Password: `Funcionario123*`)

### Decision
- Defined in `Infrastructure/Persistence/Configurations/UserConfiguration.cs`:
  - `HasData` with fixed GUIDs:
    - Admin ID: `B81B94C8-E0C7-4187-8A33-89AC54395E01`
    - Funcionario ID: `E49D34F1-9BD7-40A5-926B-9548BE740F02`
  - Pre-hashed BCrypt strings (work factor 11) embedded into the migration seed to avoid runtime hashing during application startup.

### Rationale
- Enables seamless out-of-the-box local execution, automated regression tests, and evaluation without manual SQL insertions.
