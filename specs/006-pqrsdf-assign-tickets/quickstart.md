# Quickstart & Validation Guide: PQRSDF Ticket Assignment

**Feature**: `006-pqrsdf-assign-tickets`  
**Date**: 2026-09-18  

This guide defines end-to-end validation scenarios that prove the ticket assignment and workload management feature operates according to all requirements and constitutional gates.

---

## 1. Prerequisites & Environment Setup

1. **Start Database & Backend**:
   ```bash
   cd backend
   dotnet run --project src/Api
   ```
2. **Start Frontend Client**:
   ```bash
   cd frontend
   npm run dev
   ```
3. **Seeded Test Accounts**:
   - Administrator: `admin@pqrsdf.gov.co` / `Admin123*`
   - Official 1: `funcionario@pqrsdf.gov.co` / `Funcionario123*`
   - Official 2 (Secondary for reassignment testing): `funcionario2@pqrsdf.gov.co` / `Funcionario123*` (or provisioned via seed)

---

## 2. Validation Scenarios

### Scenario 1: Unassigned Queue Visibility and Role Enforcement (RBAC)

1. **Test Unauthorized Access**:
   - Request `GET /api/v1/assignments/unassigned` without an `Authorization` header → verify HTTP `401 Unauthorized`.
   - Log in as `funcionario@pqrsdf.gov.co`, obtain JWT, and request `GET /api/v1/assignments/unassigned` → verify HTTP `403 Forbidden`.
   - In Next.js frontend, navigate to `/dashboard/assignments` while logged in as Funcionario → verify automatic redirection or "Acceso Denegado" view.
2. **Test Administrator Access**:
   - Log in as `admin@pqrsdf.gov.co`, navigate to `/dashboard/assignments`.
   - Verify unassigned requests appear in the "Sin Asignar" tab, sorted by nearest statutory due date.
   - Verify filter controls for "Tipo de Solicitud" and "Área de Destino" filter the table without page reloads.

---

### Scenario 2: Quick Detail Inspection (Drawer) & Initial Assignment

1. **Inspect Ticket**:
   - Click on any unassigned ticket row.
   - Verify slide-over panel (drawer) opens smoothly displaying:
     - Citizen radicado number and full description text.
     - Destination area and filing date.
     - Due date and urgency badge (Red / Yellow / Green).
     - Embedded official assignment selector.
2. **Execute Assignment**:
   - In the drawer selector, choose `funcionario@pqrsdf.gov.co` (showing active count `0/5`).
   - Add an optional instruction note: `"Favor revisar antecedentes de petición similar"`.
   - Click **Asignar Solicitud**.
   - Verify:
     - Ticket disappears immediately from the "Sin Asignar" tab.
     - Database reflects `Status = InReview`, `AssignedToUserId` populated, and an audit record in `TicketAssignmentHistories`.

---

### Scenario 3: Workload Limit Enforcement (Max 5 Active Tickets)

1. **Fill Official's Capacity to 5/5**:
   - Assign 4 additional tickets to `funcionario@pqrsdf.gov.co` until their workload reaches `5/5`.
2. **Verify Capacity Invariant**:
   - In the official selector for a new unassigned ticket, observe that `funcionario@pqrsdf.gov.co` is rendered as `5/5 (Cupo Lleno)` and disabled for selection.
   - Send direct API call attempting to assign a 6th ticket to this official:
     ```bash
     curl -X POST http://localhost:5000/api/v1/assignments/{radicado}/assign \
       -H "Authorization: Bearer <ADMIN_JWT>" \
       -H "Content-Type: application/json" \
       -d '{"officialId": "<OFFICIAL_UUID>"}'
     ```
   - Verify response is HTTP `409 Conflict` with code `PqrsdfTicket.OfficialWorkloadLimitReached`.

---

### Scenario 4: Reassignment with Mandatory Justification

1. **Locate Assigned Ticket**:
   - As Administrator, navigate to the **"En Trámite"** tab in `/dashboard/assignments`.
   - Enter radicado in search bar → table filters to the target in-review ticket.
2. **Execute Reassignment**:
   - Click ticket to open detail drawer and select **Reasignar Funcionario**.
   - Select Official 2 (capacity < 5).
   - Attempt to submit with empty justification or fewer than 10 characters → verify client-side and server-side validation error: *"Debe ingresar un motivo de reasignación de al menos 10 caracteres."*
   - Enter valid justification: `"Reasignado por licencia temporal del funcionario titular."` and confirm.
   - Verify:
     - Official 1 active workload decrements from 5 to 4.
     - Official 2 active workload increments.
     - Audit log contains `AssignmentType = Reassignment` with the justification text preserved.

---

### Scenario 5: Official Dashboard Inbox & SLA Urgency Badges

1. **Verify Official's View**:
   - Log out and log in as `funcionario@pqrsdf.gov.co`.
   - On `/dashboard`, verify the **Mis Solicitudes Asignadas** table renders active assigned tickets (up to 5).
   - Verify each row displays radicado, request type, citizen subject, assignment date, admin notes, and the visual urgency badge:
     - $\le 3$ business days: Red badge **Crítico / Vencido**
     - $4$ to $7$ business days: Yellow badge **Atención**
     - $\ge 8$ business days: Green badge **A tiempo**
   - Verify all texts, headers, and tooltips are strictly in Spanish.

---

## 3. Automated Verification Commands

```bash
# Backend unit & integration tests
dotnet test backend/tests/Application.UnitTests --filter "FullyQualifiedName~Assign"
dotnet test backend/tests/Domain.UnitTests --filter "FullyQualifiedName~PqrsdfTicket"

# Frontend unit tests
cd frontend && npm run test:unit
```
