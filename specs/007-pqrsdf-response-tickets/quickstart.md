# Quickstart & Verification Guide: PQRSDF Ticket Management and Official Response

**Feature**: `007-pqrsdf-response-tickets`  
**Date**: 2026-09-19  
**Status**: Completed  

---

## Prerequisites

1. **Backend Database**: Local SQL Server or Docker container running with migrations applied.
2. **Staff Seed User**:
   - Funcionario: `funcionario@pqrsdf.gov.co` (Password: `Funcionario123*`)
   - Administrador: `admin@pqrsdf.gov.co` (Password: `Admin123*`)
3. **Assigned Ticket**: At least one ticket in `InReview` status assigned to the test Funcionario (from feature 006).

---

## Automated Verification Scenarios

### 1. Backend Unit Tests

Execute domain and application layer tests:

```bash
# Run unit tests for TicketStatusHistory and PqrsdfTicket response methods
dotnet test backend/tests/Pqrsdf.Domain.UnitTests/

# Run CQRS handler tests for RespondTicket and ChangeTicketStatus
dotnet test backend/tests/Pqrsdf.Application.UnitTests/
```

Expected Outcome: All tests pass with 100% assertions on `Result.Success` and `Result.Failure` scenarios.

### 2. Frontend Unit Tests

Execute component tests for management modal and inbox interaction:

```bash
cd frontend
npm run test
```

Expected Outcome: Components render character counters, validate min/max text lengths, handle submission states, and update table items on response.

---

## Manual End-to-End Verification Flow

### Scenario 1: Inspect Ticket Detail and Applicant Data from Official Inbox

1. Navigate to `http://localhost:3000/auth` and log in as `funcionario@pqrsdf.gov.co`.
2. On `/dashboard`, view the table **"Mis Solicitudes Asignadas"**.
3. Click on a ticket row or click **"Gestionar"**.
4. **Verify**:
   - Detail drawer opens displaying radicado number, citizen full statement, category, filing date, and SLA urgency badge.
   - Applicant personal details (name, email, phone) are visible to the official.
   - Administrative instructions from assignment are rendered.

### Scenario 2: Change Operational Status with Mandatory Justification

1. In the open management drawer, select the tab/action **"Actualizar Estado / Justificación"**.
2. Leave the justification field empty and click "Guardar".
3. **Verify**: Frontend shows validation alert: `"Debe ingresar una justificación obligatoria de al menos 10 caracteres."`
4. Type: `"Se solicita ampliación de concepto a la subdirección técnica."` (>= 10 chars).
5. Click **"Guardar"**.
6. **Verify**:
   - Status update succeeds.
   - The "Historial de Gestión" section refreshes to show the new entry with timestamp and official name.
   - In the database, a row is inserted in `TicketStatusHistories`.

### Scenario 3: Submit Final Institutional Response and Verify Case Closure

1. In the management drawer, select the tab/action **"Registrar Respuesta Final"**.
2. Type an institutional response between 10 and 4,000 characters:
   `"Estimado ciudadano, se ha realizado la verificación en terreno y se concluye favorablemente la solicitud presentada."`
3. Click **"Enviar Respuesta y Cerrar Radicado"**.
4. **Verify**:
   - Modal closes with success notification in Spanish.
   - The ticket disappears from the official's active inbox (or is marked as closed).
   - The official's active workload badge decrements (e.g. from `3/5` to `2/5`).

### Scenario 4: Verify Public Consultation Portal (`/pqrsdf/search`)

1. Open a private/incognito window and navigate to `http://localhost:3000/pqrsdf/search`.
2. Enter the closed radicado number and submit search.
3. **Verify**:
   - Status badge indicates **"Cerrado"**.
   - SLA remaining days counter is halted / marked as finalized without overdue accumulation.
   - The full institutional response text and resolution date are displayed.
   - Internal status change justifications and staff notes are strictly hidden from public view.
