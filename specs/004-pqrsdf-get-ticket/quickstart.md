# Quickstart & Verification Guide: Public PQRSDF Ticket Consultation

**Feature**: [Public PQRSDF Ticket Consultation](../spec.md)  
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

---

## 2. End-to-End Verification Scenarios

### Scenario 1: Query Active PQRSDF Ticket by Radicado

1. Navigate to `http://localhost:3000/pqrsdf/search`.
2. Enter an existing active radicado number (e.g., `2026-00000001`).
3. Click the **"Consultar"** button.
4. **Expected Outcome**:
   - The query executes and loads results in < 2 seconds.
   - The interface renders:
     - Radicado Number (`2026-00000001`).
     - Request Type (e.g., `Petición`).
     - Destination Area (e.g., `Atención al Ciudadano`).
     - Asunto (*Subject*) and Descripción (*Description*) in plain text.
     - Filing Date and Legal Expiration Due Date.
     - Remaining Colombian business days countdown.
     - Visual Timeline showing current stage (e.g., `Registrado` or `En trámite`) and completed stages.
     - **0% PII**: Applicant name, identification, email, and telephone number are completely absent from both UI and API JSON response.

---

### Scenario 2: Query Closed / Answered Ticket with Final Resolution

1. Enter a radicado number corresponding to a resolved or closed ticket (e.g., status `Cerrado` or `Respondido`).
2. Trigger the search.
3. **Expected Outcome**:
   - Status badge displays `Cerrado` or `Respondido`.
   - The timeline shows all stages up to closure marked as completed.
   - A dedicated **"Respuesta Oficial"** card renders displaying:
     - The official response text provided by the institution.
     - The date and time when the response was issued.
   - The active remaining days counter is stopped or indicates case closed.

---

### Scenario 3: Query Overdue Ticket (Vencida)

1. Query a ticket whose statutory due date has already passed without closure.
2. **Expected Outcome**:
   - An alert badge prominently displays **"Vencida"** in warning/red style.
   - The interface explicitly indicates elapsed business days in mora (e.g., *"Vencida hace 3 días hábiles"*).

---

### Scenario 4: Non-Existent Radicado Lookup (404)

1. Enter a validly formatted but non-existent radicado (e.g., `2026-99999999`).
2. Trigger the search.
3. **Expected Outcome**:
   - The system displays a friendly Spanish error banner: *"No se encontró ninguna solicitud con el radicado ingresado. Por favor verifique el número e intente de nuevo."*
   - No technical stack traces or database errors are displayed.

---

### Scenario 5: Direct Link & URL Query Parameter Access

1. Open a browser tab directly to: `http://localhost:3000/pqrsdf/search?radicado=2026-00000001`.
2. **Expected Outcome**:
   - The input field is prefilled automatically with `2026-00000001`.
   - The ticket data is retrieved and rendered automatically without requiring an additional click.

---

### Scenario 6: Rate Limiting Verification (HTTP 429)

1. Execute more than 30 rapid GET requests in less than a minute:
   ```bash
   for i in {1..35}; do curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5000/api/v1/pqrsdf/2026-00000001; done
   ```
2. **Expected Outcome**:
   - The first 30 requests return HTTP 200 OK.
   - Requests 31 to 35 return HTTP 429 Too Many Requests with message: *"Ha superado el límite de consultas permitidas por minuto. Por favor espere un momento antes de intentar de nuevo."*

---

## 3. Automated Test Execution

```bash
# Run backend domain, application, and API tests
cd backend
dotnet test

# Run frontend unit and component tests
cd frontend
npm test
```
