# Feature Specification: User Authentication and Role-Based Access Control

**Feature Branch**: `feature/auth-user`

**Created**: 2026-09-18

**Status**: Draft

**Input**: User description from `pqrsdf-docs/auth-user.md`:
> "### 3. Módulo de Autenticación
> Historia de Usuario: Como Funcionario o Administrador, quiero iniciar sesión en el sistema con mis credenciales para acceder a la bandeja de gestión según mis permisos.
> Requerimientos:
> - Endpoint de login (`POST /api/auth/login`) que valide credenciales usando BCrypt.
> - El sistema debe generar y devolver un token JWT.
> - El token JWT debe incluir el rol del usuario (Funcionario o Administrador) en sus claims.
> Criterios de Aceptación:
> - Un usuario con credenciales correctas recibe un JWT válido y es redirigido a su bandeja.
> - Las rutas de API y del Frontend (excepto radicación y consulta pública) están protegidas y deniegan el acceso si no hay un JWT válido."

---

## Clarifications

### Session 2026-09-18

- Q: How should the JWT token returned by `POST /api/auth/login` be stored and transmitted by the Next.js frontend to balance API standards, security, and Next.js App Router route guarding? → A: Option A — Bearer Token in Response Body + Cookie Storage: Login API returns `{ token, user, expiresIn }`; frontend stores the token in a cookie to enable Next.js `middleware.ts` to guard client routes, and attaches `Authorization: Bearer <token>` to all protected API calls.
- Q: When an authenticated user logs in successfully without a prior `returnUrl`, to which route should they be redirected based on their role? → A: Option A — Unified `/dashboard`: Both `Funcionario` and `Administrador` land on `/dashboard`, which dynamically adapts its operational widgets, actions, and navigation options based on the user's role claim.
- Q: How should the authentication endpoint (`POST /api/auth/login`) handle rapid successive failed login attempts to protect staff accounts from brute-force attacks? → A: Option A — IP-based Rate Limiting: Restrict login attempts to a maximum of 5 attempts per minute per client IP, returning HTTP 429 Too Many Requests with an informative Spanish retry-after feedback message to mitigate brute-force attacks without enabling account lockout DoS against staff.
- Q: What initial seed accounts should be provisioned in the database during startup/migration to facilitate development, evaluation, and role testing? → A: Option A — Two Pre-seeded Accounts: Seed `admin@pqrsdf.gov.co` (`Administrador`, FullName: "Administrador del Sistema") and `funcionario@pqrsdf.gov.co` (`Funcionario`, FullName: "Funcionario de PQRSDF") with secure pre-hashed BCrypt passwords (e.g. `Admin123*` and `Funcionario123*`) for immediate validation of both roles.
- Q: What should be the lifespan of the issued JWT access token and how should expiration be handled on the frontend? → A: Option A — 8-Hour Shift Token (No Refresh Token): Token is valid for 8 hours (28,800 seconds) corresponding to a standard operational work shift. When expired, any subsequent API call returns 401 Unauthorized, and the frontend redirects the user to `/auth` with an informative session expiration message in Spanish.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Secure Staff Login and Role-Based Redirection (Priority: P1)

As an internal staff member (Funcionario or Administrador),  
I want to authenticate into the system using my registered credentials (email and password),  
So that I can access my designated management workspace according to my assigned permissions and responsibilities.

**Why this priority**: Authentication is the fundamental security gateway for all internal operations. Without login capability, staff cannot manage, assign, or resolve PQRSDF tickets.

**Independent Test**: Can be tested independently by navigating to the `/auth` login interface, submitting valid credentials for a seeded Funcionario and Administrador account, and verifying that the system successfully returns an authenticated session with the expected role claims and redirects to `/dashboard`.

**Acceptance Scenarios**:

1. **Given** an unauthenticated staff user on the login screen, **When** they submit valid email and password for a user with role `Funcionario`, **Then** the system authenticates the user, generates a signed JWT token containing the user identity and `Funcionario` role claim, and redirects them to `/dashboard` displaying staff-oriented ticket operations.
2. **Given** an unauthenticated administrator on the login screen, **When** they submit valid email and password for a user with role `Administrador`, **Then** the system authenticates the user, generates a signed JWT token containing the user identity and `Administrador` role claim, and redirects them to `/dashboard` displaying administrative management controls.

3. **Given** an unauthenticated user on the login screen, **When** they enter non-existent or incorrect credentials, **Then** the system rejects the authentication attempt, returns an HTTP 401 Unauthorized status, and displays a generic, safe error message in Spanish ("Credenciales inválidas. Por favor verifique su correo y contraseña.") without revealing whether the email exists.
4. **Given** a user account marked as inactive or suspended, **When** valid credentials for that account are submitted, **Then** the system rejects access with an informative error message in Spanish ("La cuenta de usuario se encuentra inactiva. Contacte al administrador.").

---

### User Story 2 - API Route Protection and Role Authorization (Priority: P1)

As the system security guardian,  
I want the backend API to strictly enforce authentication and role authorization on internal endpoints,  
So that unauthorized parties cannot access, view, or alter internal PQRSDF management records while keeping citizen public endpoints freely accessible.

**Why this priority**: Ensures confidentiality, data protection, and integrity of citizen claims. Public citizen endpoints must remain accessible while management endpoints are strictly guarded.

**Independent Test**: Can be tested independently by sending HTTP requests with and without valid Bearer tokens to both public endpoints (ticket creation, ticket tracking) and protected endpoints (internal ticket list, status updates), validating HTTP status codes (200, 401, 403).

**Acceptance Scenarios**:

1. **Given** an unauthenticated request sent to a protected management API endpoint, **When** processed by the server, **Then** the system rejects the request with HTTP 401 Unauthorized and standard ProblemDetails format.
2. **Given** a request sent with an invalid, expired, or malformed JWT token, **When** processed by the server, **Then** the system rejects the request with HTTP 401 Unauthorized.
3. **Given** an authenticated request from a user with role `Funcionario` to an endpoint restricted exclusively to `Administrador`, **When** processed by the server, **Then** the system rejects the request with HTTP 403 Forbidden.
4. **Given** an unauthenticated request to public endpoints (such as ticket filing `POST /api/v1/pqrsdf/tickets` or public consultation `GET /api/v1/pqrsdf/tickets/{radicado}`), **When** processed by the server, **Then** the system allows execution normally without requiring authentication.

---

### User Story 3 - Frontend Navigation Guarding and Redirect Handling (Priority: P2)

As an unauthenticated user or unauthorized visitor,  
I want the frontend application to automatically protect private management views,  
So that direct URL access to administrative sections redirects me to the login view and preserves my destination for post-login redirection.

**Why this priority**: Prevents unauthorized viewing of administrative layouts or internal data structures, and provides a seamless user journey by restoring the requested destination after authentication.

**Independent Test**: Can be tested independently by opening an incognito browser window, attempting to navigate directly to `/dashboard` or `/pqrsdf/management`, verifying immediate redirection to `/auth` with query parameter `?returnUrl=...`, and verifying return to the destination upon successful login.

**Acceptance Scenarios**:

1. **Given** an unauthenticated user attempting to access a protected frontend route (e.g., `/dashboard` or `/pqrsdf/management`), **When** navigation occurs, **Then** the client router immediately intercepts the request and redirects to `/auth` with the target path preserved in `returnUrl`.
2. **Given** an unauthenticated citizen visiting public routes (`/`, `/pqrsdf/new`, `/pqrsdf/search`), **When** navigating, **Then** the application renders the public pages immediately without authentication barriers.
3. **Given** a user completing authentication after being redirected from a protected route, **When** login succeeds, **Then** the application automatically redirects the user to the original `returnUrl` rather than the default landing page.

---

### User Story 4 - Session Termination (Logout) (Priority: P2)

As an authenticated staff member,  
I want to be able to log out of the system securely from the user interface,  
So that my active session is terminated and subsequent users of the device cannot access management features without re-authenticating.

**Why this priority**: Required for shared workstation security and compliance with organizational information security standards.

**Independent Test**: Can be tested independently by logging in, clicking the logout action in the navigation bar, verifying that the session state and tokens are cleared, and confirming that immediate attempts to navigate back to protected views redirect to the login page.

**Acceptance Scenarios**:

1. **Given** an authenticated user on any internal screen, **When** they select the logout action, **Then** the application clears the client-side authentication tokens, resets user state, and redirects the user to the public landing page or login view with a confirmation message.
2. **Given** a user who has logged out, **When** they click the browser back button or enter a protected URL, **Then** the system denies access and presents the login interface.

---

### Edge Cases

- **Empty or Whitespace Inputs**: Submitting empty email or password values triggers client-side and server-side validation with localized messages in Spanish without issuing authentication attempts.
- **Account Inactivity / Deactivation**: When an account has been deactivated by an administrator, any existing active token should be rejected upon critical operations, and new login attempts must be blocked.
- **Expired Token Lifecycle**: If a user's session token expires after 8 hours while viewing an internal screen, any subsequent asynchronous API call returns 401 Unauthorized, prompting the frontend to redirect the user to `/auth` with an informative Spanish alert ("Su sesión ha expirado. Por favor inicie sesión nuevamente.") while saving the attempted action's returnUrl.
- **Malformed or Forged Tokens**: If a client sends an altered JWT signature or invalid token header, the API immediately discards the token and responds with 401 Unauthorized without crashing.

- **Repeated Failed Login Attempts & Rate Limiting**: The system limits authentication attempts to a maximum of 5 requests per minute per client IP. Exceeding this threshold results in an HTTP 429 Too Many Requests response with a `Retry-After` header and an error message in Spanish ("Demasiados intentos de inicio de sesión fallidos. Por favor intente nuevamente en unos minutos."). Within the allowed threshold, failed attempts return generic 401 responses in constant comparison time to prevent timing attacks.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an authentication endpoint (`POST /api/v1/auth/login` or `POST /api/auth/login`) accepting user identifier (email) and password credentials.
- **FR-002**: System MUST verify user passwords against stored cryptographic hashes using the BCrypt hashing algorithm.
- **FR-003**: System MUST generate and return a signed JSON Web Token (JWT) upon successful authentication valid for 8 hours (28,800 seconds), containing standard claims: subject identifier (`sub`), user email, display name, and assigned role (`role`).

- **FR-004**: System MUST support two distinct internal roles: `Funcionario` (authorized for ticket processing and response drafting) and `Administrador` (authorized for overall management, configuration, and reporting).
- **FR-005**: System MUST reject authentication attempts with invalid credentials, non-existent users, or mismatched passwords, returning an HTTP 401 Unauthorized response with a generic Spanish error message.
- **FR-006**: System MUST reject authentication for users whose status is inactive (`IsActive = false`).
- **FR-007**: System MUST enforce JWT Bearer authorization on all management and administrative API endpoints, returning 401 for unauthenticated requests and 403 for role-insufficient requests.
- **FR-008**: System MUST maintain public accessibility without authentication for citizen-facing endpoints: filing PQRSDF tickets (`POST /api/v1/pqrsdf/tickets`), public ticket consultation (`GET /api/v1/pqrsdf/tickets/{radicado}`), destination areas lookup, and system health checks.
- **FR-009**: System MUST protect client-side internal routes (e.g., `/dashboard`, management views) via route protection / middleware, redirecting unauthenticated users to `/auth`.
- **FR-010**: System MUST preserve the attempted destination path during login redirects (`returnUrl`) and navigate the user to that destination upon successful login.
- **FR-011**: System MUST provide a logout mechanism that purges stored credentials/tokens from client storage and redirects the user to the public home view.
- **FR-012**: System MUST render all client-facing UI components, forms, buttons, and error messages strictly in Spanish, adhering to institutional branding.
- **FR-013**: System MUST store the issued JWT token in a client cookie enabling Next.js `middleware.ts` to perform server-side route guarding, and forward it via HTTP `Authorization: Bearer <token>` header in client API requests.
- **FR-014**: System MUST enforce an IP-based rate limit of 5 login attempts per minute on the login endpoint (`POST /api/v1/auth/login`), returning HTTP 429 Too Many Requests when the limit is exceeded.
- **FR-015**: System MUST provision default seed accounts during database initialization for evaluation: an administrator (`admin@pqrsdf.gov.co`) and an official (`funcionario@pqrsdf.gov.co`) with secure pre-hashed BCrypt passwords.

---

### Key Entities *(include if feature involves data)*

- **User**: Represents internal system operators and administrators.
  - Key attributes: `Id` (unique identifier), `Email` (unique email value object), `PasswordHash` (BCrypt hash string), `FullName` (string), `Role` (`Funcionario` or `Administrador`), `IsActive` (boolean flag), `CreatedAtUtc` (timestamp), `LastLoginAtUtc` (nullable timestamp).
- **UserRole**: Enumeration of operational roles within the system:
  - `Funcionario`: Staff member who manages, reviews, and answers assigned PQRSDF tickets.
  - `Administrador`: Administrative user with full privileges over configuration, accounts, and system audits.
- **AuthToken**: Value representation of the authenticated session:
  - Key attributes: `AccessToken` (signed JWT string), `TokenType` ("Bearer"), `ExpiresIn` (seconds or expiration timestamp), `Role` (assigned role name), `User` (basic user profile info: Id, Email, FullName).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of unauthorized requests to protected management endpoints are rejected with appropriate HTTP status codes (401 Unauthorized or 403 Forbidden).
- **SC-002**: Users with valid credentials complete login and arrive at their role-designated management workspace in under 2 seconds.
- **SC-003**: 0% user enumeration vulnerability: error messages and response times on failed authentication attempts are uniform regardless of whether the email exists in the database.
- **SC-004**: 100% of public citizen operations (filing tickets and checking ticket status) continue to operate with zero authentication friction and without requiring citizen credentials.
- **SC-005**: 100% of direct URL attempts by unauthenticated users to access protected management views are successfully intercepted and redirected to the login interface.

---

## Assumptions

- **Initial Administrative & Staff Seed**: The system initializes with two default user accounts seeded into the database (`admin@pqrsdf.gov.co` and `funcionario@pqrsdf.gov.co`) to allow immediate verification of both operational roles.

- **Single Unified Database**: In compliance with the project constitution, user records and credentials reside within the single unified relational database alongside PQRSDF entities.
- **Token Lifespan**: Access tokens are configured with a standard operational lifetime (e.g., 8 hours corresponding to a standard workday shift) before requiring re-authentication.
- **Internal User Scope**: Public citizens filing or querying PQRSDF tickets are not required to register or authenticate; this authentication feature applies exclusively to internal staff and administrators.
- **Secure Password Policies**: Minimum length of 8 characters containing letters and numbers is enforced for internal accounts.
