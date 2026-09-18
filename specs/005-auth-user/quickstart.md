# Quickstart & Verification Guide: User Authentication & Role-Based Access Control

**Feature**: [User Authentication and Role-Based Access Control](spec.md)  
**Date**: 2026-09-18  
**Status**: Ready for Implementation  

---

## 1. Prerequisites & Setup

Ensure the database container, ASP.NET Core backend, and Next.js frontend are running:

```bash
# 1. Start SQL Server container (if not already running)
docker compose up -d sqlserver

# 2. Run Backend API (.NET 10)
cd backend
dotnet run --project src/Api

# 3. Run Frontend (Next.js 15)
cd frontend
npm run dev
```

The database initializes with two pre-seeded development accounts:
- **Administrador**: `admin@pqrsdf.gov.co` / `Admin123*`
- **Funcionario**: `funcionario@pqrsdf.gov.co` / `Funcionario123*`

---

## 2. End-to-End Verification Scenarios

### Scenario 1: Successful Login as Funcionario
1. Navigate to `http://localhost:3000/auth`.
2. Enter email: `funcionario@pqrsdf.gov.co` and password: `Funcionario123*`.
3. Click **"Iniciar Sesión"**.
4. **Expected Outcome**:
   - Backend responds with HTTP 200 containing `accessToken` and user profile with role `Funcionario`.
   - Frontend stores the JWT in the `auth_token` cookie.
   - User is redirected to `/dashboard` in under 2 seconds.
   - The navigation header displays the user's name ("Funcionario de PQRSDF"), badge "Funcionario", and a "Cerrar Sesión" button.

---

### Scenario 2: Successful Login as Administrador
1. Navigate to `http://localhost:3000/auth`.
2. Enter email: `admin@pqrsdf.gov.co` and password: `Admin123*`.
3. Click **"Iniciar Sesión"**.
4. **Expected Outcome**:
   - Backend responds with HTTP 200 containing `accessToken` with `role: Administrador`.
   - User is redirected to `/dashboard`.
   - Dashboard renders administrative management actions and controls.

---

### Scenario 3: Failed Login with Incorrect Credentials
1. Navigate to `http://localhost:3000/auth`.
2. Enter email: `funcionario@pqrsdf.gov.co` and an incorrect password: `WrongPassword999*`.
3. Click **"Iniciar Sesión"**.
4. **Expected Outcome**:
   - Backend responds with HTTP 401 Unauthorized (`ProblemDetails`).
   - A friendly alert in Spanish displays on the login card:  
     *"Credenciales inválidas. Por favor verifique su correo y contraseña."*
   - No sensitive information is leaked (error message is identical whether email exists or not).

---

### Scenario 4: IP Rate Limiting on Login Endpoint (Brute-Force Protection)
1. Send 6 rapid login attempts within 1 minute from the same IP using `curl`:
   ```bash
   for i in {1..6}; do
     curl -s -o /dev/null -w "%{http_code}\n" -X POST http://localhost:5000/api/v1/auth/login \
       -H "Content-Type: application/json" \
       -d '{"email":"test@pqrsdf.gov.co","password":"wrong"}'
   done
   ```
2. **Expected Outcome**:
   - The first 5 requests return `401`.
   - The 6th request returns `429 Too Many Requests` with a ProblemDetails payload in Spanish indicating rate limit exceeded.

---

### Scenario 5: Protected Route Access & Redirection via Next.js Middleware
1. Open a new Incognito browser window (without active cookies).
2. Attempt to navigate directly to `http://localhost:3000/dashboard`.
3. **Expected Outcome**:
   - Next.js `middleware.ts` intercepts the request before rendering.
   - The browser is immediately redirected to `/auth?returnUrl=%2Fdashboard`.
   - After entering valid credentials and logging in, the user is forwarded directly to `/dashboard`.

---

### Scenario 6: Protected Backend API Endpoint with Bearer Token
1. Query a protected backend endpoint without an `Authorization` header:
   ```bash
   curl -i http://localhost:5000/api/v1/pqrsdf/management
   # Expected: HTTP/1.1 401 Unauthorized
   ```
2. Query the same endpoint with a valid Bearer token obtained from login:
   ```bash
   TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"funcionario@pqrsdf.gov.co","password":"Funcionario123*"}' | jq -r '.accessToken')

   curl -i http://localhost:5000/api/v1/pqrsdf/management \
     -H "Authorization: Bearer $TOKEN"
   # Expected: HTTP/1.1 200 OK (or authorized response)
   ```

---

### Scenario 7: Session Termination (Logout)
1. In an authenticated browser session, click the **"Cerrar Sesión"** button in the top navigation bar.
2. **Expected Outcome**:
   - The `auth_token` cookie is cleared.
   - The user is redirected to `/` (or `/auth`) with a confirmation message.
   - Pressing the browser's Back button or re-entering `/dashboard` triggers the unauthorized middleware redirect to `/auth`.

---

## 3. Automated Test Execution

Run the backend and frontend test suites to verify functionality:

```bash
# Run backend unit & architecture tests
cd backend
dotnet test

# Run frontend tests
cd frontend
npm test
```
