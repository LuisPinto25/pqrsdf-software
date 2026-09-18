# Quickstart & Verification Guide: Public PQRSDF Ticket Registration (003-pqrsdf-make-ticket)

This guide details the procedures to verify and validate the public PQRSDF filing module end-to-end across backend and frontend.

---

## 1. Prerequisites

- **.NET 10 SDK**: Installed and available in PATH (`dotnet --version` -> `10.x`).
- **Node.js & pnpm**: Node.js LTS (v20+) and pnpm (`pnpm --version` -> `9.x+`).
- **Database**: Local SQL Server or LocalDB instance running, connection string defined in `backend/src/Api/appsettings.Development.json`.

---

## 2. Environment Setup

### 2.1 Backend Initialization

```bash
cd backend
dotnet build
# Apply initial migrations and area seeding
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

### 2.2 Frontend Initialization

```bash
cd frontend
pnpm install
```

---

## 3. Verification Scenarios

### Scenario 1: Retrieve Active Destination Areas (Backend API)

**Objective**: Verify that unauthenticated clients can retrieve active administrative units.

```bash
curl -X GET "http://localhost:5000/api/v1/pqrsdf/areas" \
  -H "Accept: application/json"
```

**Expected Response**:
- HTTP Status: `200 OK`
- JSON array containing active areas (e.g. `Atención al Ciudadano`, `Oficina Jurídica`, etc.).

---

### Scenario 2: Register a Standard Petition (P1 Flow)

**Objective**: Submit a valid standard petition and assert a unique radicado number and 15-business-day due date.

```bash
# Replace AREA_ID with a valid Guid from Scenario 1
curl -X POST "http://localhost:5000/api/v1/pqrsdf" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "Petition",
    "destinationAreaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "isAnonymous": false,
    "applicant": {
      "fullName": "Carlos Andrés Gómez",
      "identificationType": "CC",
      "identificationNumber": "1098765432",
      "email": "carlos.gomez@example.com",
      "phoneNumber": "3001234567"
    },
    "subject": "Solicitud de información sobre plan de ordenamiento",
    "description": "Por medio de la presente solicito copia digital del plan de ordenamiento territorial vigente actualizado para el sector norte de la ciudad."
  }'
```

**Expected Response**:
- HTTP Status: `201 Created`
- Body:
  ```json
  {
    "id": "<guid>",
    "radicadoNumber": "2026-00000001",
    "type": "Petition",
    "createdAtUtc": "<timestamp>",
    "dueDate": "<date-15-business-days-ahead>",
    "businessDaysCount": 15,
    "status": "Registered"
  }
  ```

---

### Scenario 3: Register an Anonymous Denunciation (30 Business Days)

**Objective**: Submit an anonymous denunciation without applicant data and verify 30-business-day due date calculation.

```bash
curl -X POST "http://localhost:5000/api/v1/pqrsdf" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "Denunciation",
    "destinationAreaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "isAnonymous": true,
    "applicant": null,
    "subject": "Denuncia por cobro irregular de trámite",
    "description": "Se pone en conocimiento presunto cobro no autorizado para agilizar la expedición de licencias de construcción en la sede centro."
  }'
```

**Expected Response**:
- HTTP Status: `201 Created`
- `radicadoNumber`: Monotonically incremented (e.g., `2026-00000002`).
- `businessDaysCount`: `30`.
- `dueDate`: Calculated exactly 30 Colombian business days ahead, skipping weekends and holidays.

---

### Scenario 4: Reject Anonymous Attempt on Standard Petition (Validation Flow)

**Objective**: Attempt to submit an anonymous petition (which is forbidden per business rules).

```bash
curl -X POST "http://localhost:5000/api/v1/pqrsdf" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "Petition",
    "destinationAreaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "isAnonymous": true,
    "applicant": null,
    "subject": "Petición anónima no permitida",
    "description": "Intento de enviar petición sin registrar los datos del solicitante."
  }'
```

**Expected Response**:
- HTTP Status: `400 Bad Request` or `422 Unprocessable Entity` (ProblemDetails format).
- Title: Validation or Business Rule Error in Spanish explaining that anonymous filing is only allowed for Denuncia and Sugerencia.

---

### Scenario 5: Concurrency Radicado Uniqueness Verification

**Objective**: Execute 50 concurrent filing requests simultaneously and verify zero duplicate radicados.

```bash
# Run the automated concurrency test in backend
cd backend
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~RadicadoConcurrencyTests"
```

**Expected Outcome**:
- 50 requests successfully produce 50 strictly distinct radicado values matching regex `^\d{4}-\d{8}$`.

---

### Scenario 6: Frontend Public Filing Flow (UI Verification)

1. Run frontend dev server: `cd frontend && pnpm dev`.
2. Open browser at `http://localhost:3000/pqrsdf`.
3. Verify that:
   - Form renders cleanly without requiring login.
   - Destination areas dropdown loads dynamically from backend.
   - 6 types are present.
   - Toggling "Radicar de forma anónima" is only visible/enabled when selecting "Denuncia" or "Sugerencia".
   - Subject character counter updates dynamically as user types (5–150).
   - Description character counter updates dynamically as user types (10–4000).
   - Clicking "Radicar solicitud" disables button and displays a loading spinner.
   - Upon success, view switches to confirmation screen with radicado number, due date, and "Copiar radicado" action.
