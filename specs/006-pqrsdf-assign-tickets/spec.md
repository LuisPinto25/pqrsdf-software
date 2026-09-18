# Feature Specification: PQRSDF Ticket Assignment to Staff

**Feature Branch**: `006-pqrsdf-assign-tickets`

**Created**: 2026-09-18

**Status**: Draft

**Input**: User description from `pqrsdf-docs/pqrsdf-asign-tickets.md`:
> "### 6. Módulo de Asignación (Administrador)
> Historia de Usuario: Como Administrador, quiero visualizar las solicitudes sin asignar para designarlas al funcionario correspondiente y mantener el flujo de trabajo activo.
> Requerimientos:
> - Solo los usuarios con rol Administrador pueden ver las solicitudes no asignadas y ejecutar la acción de asignación.
> - Permitir seleccionar un funcionario de una lista y vincularlo al radicado.
> Criterios de Aceptación:
> - Un usuario con rol "Funcionario" no puede acceder al endpoint ni a la UI de asignación.
> - Al completarse la asignación, la PQRSDF aparece en la bandeja del funcionario elegido."

---

## Clarifications

### Session 2026-09-18

- **Q1: Transición de Estado del Radicado tras la Asignación**: ¿A qué estado del ciclo de vida debe transicionar la PQRSDF cuando el Administrador confirma la asignación a un funcionario?  
  → **A**: **Opción B — Transición directa al estado `InReview` ("En Trámite")**. Al completarse la asignación, el estado del radicado cambia automáticamente de `Registered` a `InReview`, indicando que la solicitud entra de inmediato en su fase formal de análisis y respuesta.
- **Q2: Política de Reasignación y Justificación de Traslado**: ¿Debe el Administrador tener la facultad de reasignar solicitudes ya asignadas a otro funcionario, y bajo qué condiciones de justificación/auditoría?  
  → **A**: **Opción A — Permitir reasignación entre funcionarios con motivo/justificación obligatoria**. El Administrador puede reasignar un radicado ya asignado a otro funcionario antes de su resolución final, exigiendo el ingreso de una justificación obligatoria (mínimo 10 caracteres) que se preserva inmutable en el registro de auditoría de asignaciones. No se permite desasignar a un estado nulo sin responsable.
- **Q3: Organización y Filtrado del Listado de Funcionarios Elegibles**: ¿Cómo debe presentarse la lista de funcionarios elegibles al momento de asignar en la interfaz?  
  → **A**: **Listado global con indicador de carga y límite estricto de 5 solicitudes activas**. Se presenta el listado global de funcionarios activos mostrando su carga actual (número de radicados asignados en curso). El sistema impone una regla de capacidad estricta: un funcionario puede tener un **máximo de 5 solicitudes asignadas activas simultáneamente**; los funcionarios que alcancen 5 solicitudes activas quedan deshabilitados para nuevas asignaciones hasta que resuelvan o liberen trámites pendientes.
- **Q4: Nivel de detalle e inspección de la solicitud previo a la asignación**: ¿Cómo debe el Administrador examinar el contenido del requerimiento ciudadano antes de realizar la asignación?  
  → **A**: **Opción A — Panel lateral (drawer) o modal de vista previa rápida**. Al hacer clic en un radicado de la lista de no asignados, se despliega un panel lateral con la descripción completa del ciudadano, asunto, tipo, área y fechas límites, integrando el selector de asignación directamente en dicho panel para completar la acción sin abandonar la cola de trabajo.
- **Q5: Criterio de ordenamiento prioritario y filtros en la bandeja de no asignadas**: ¿Cómo debe ordenarse por defecto la cola de solicitudes sin asignar y qué capacidades de filtrado debe tener el Administrador?  
  → **A**: **Opción A — Prioridad por urgencia legal con filtros por tipo y área**. La lista se ordena por defecto según el menor tiempo restante para su vencimiento legal (días hábiles restantes ascendente / fecha límite más próxima primero), e incluye selectores de filtro rápido por Tipo de PQRSDF y Área de Destino para focalizar la distribución operativa.
- **Q6: Presentación de la Bandeja de Asignaciones del Funcionario (`/dashboard`)**: ¿Cómo debe estructurarse y organizarse la bandeja de solicitudes asignadas para el funcionario?  
  → **A**: **Opción A — Tabla priorizada por vencimiento legal con notas del administrador**. La bandeja del funcionario en `/dashboard` presenta sus solicitudes activas (`InReview`, hasta un máximo de 5) ordenadas por urgencia legal (menor tiempo restante primero con semáforo visual de días hábiles), visualizando número de radicado, tipo, asunto, fecha de asignación y las notas o instrucciones de trabajo emitidas por el Administrador.
- **Q7: Umbrales del semáforo visual de vencimiento legal**: ¿Cuáles deben ser los umbrales de días hábiles restantes para las insignias de color en las bandejas de trabajo?  
  → **A**: **Opción A — 3 niveles estándar**. Rojo (Crítico / Vencido): ≤ 3 días hábiles restantes (o vencido con días <= 0); Amarillo (Atención): de 4 a 7 días hábiles restantes; Verde (A tiempo): ≥ 8 días hábiles restantes.
- **Q8: Navegación y localización de solicitudes para reasignación en el panel del Administrador**: ¿Cómo debe el Administrador localizar y seleccionar solicitudes ya asignadas para ejecutar reasignaciones?  
  → **A**: **Opción A — Pestañas ("Sin Asignar" / "En Trámite") con buscador por radicado**. La vista de gestión del Administrador organiza el trabajo mediante dos pestañas principales: "Sin Asignar" (cola de distribución prioritaria) y "En Trámite" (solicitudes con funcionario asignado), incorporando una barra de búsqueda por número de radicado en ambas vistas para localizar y abrir el panel lateral de reasignación.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Unassigned PQRSDF Queue (Priority: P1)

As an authenticated Administrator,  
I want to view a centralized list of all newly filed, unassigned PQRSDF requests,  
So that I can quickly inspect pending citizen petitions and distribute them to the appropriate officials before legal deadlines expire.

**Why this priority**: Unassigned requests cannot be processed or answered. Providing immediate visibility into unassigned tickets is the foundational prerequisite for assigning workflow responsibilities.

**Independent Test**: Can be tested independently by logging in as an `Administrador`, navigating to the assignment management interface, and verifying that all unassigned tickets are rendered with their radicado number, filing date, legal due date, remaining business days, request type, destination area, and summary.

**Acceptance Scenarios**:

1. **Given** an authenticated user with role `Administrador` on the assignment interface, **When** unassigned PQRSDF requests exist in the system, **Then** the system displays the queue sorted by default by nearest legal due date (ascending remaining business days) with radicado number, submission date, due date, remaining Colombian business days badge, request type, destination area, and citizen subject, offering filter controls by Request Type and Destination Area. Selecting any ticket row opens a slide-over preview drawer detailing the citizen's full text.
2. **Given** an authenticated user with role `Funcionario` or an unauthenticated user, **When** they attempt to access the unassigned tickets queue view or its backend endpoint, **Then** the system denies access immediately, returning an HTTP 403 Forbidden (or 401 Unauthorized) response and preventing any data disclosure.
3. **Given** an authenticated `Administrador` viewing the queue, **When** there are zero unassigned requests in the system, **Then** the interface displays an informative, courteous empty-state message in Spanish indicating that all received requests are currently assigned.

---

### User Story 2 - Assign PQRSDF to an Official with Capacity Enforcement (Priority: P1)

As an authenticated Administrator,  
I want to select an unassigned PQRSDF ticket, choose an eligible active official (Funcionario) who has not reached the 5-ticket workload limit, and confirm the assignment,  
So that the ticket transitions to active review (`InReview`), clear operational ownership is established, and the official is not overloaded beyond policy limits.

**Why this priority**: Ticket assignment transitions a passive filing into an active, accountable investigation/resolution workflow with clear operational ownership while protecting staff from burnout and guaranteeing timely attention.

**Independent Test**: Can be tested independently by selecting an unassigned ticket, choosing an eligible active Funcionario with fewer than 5 active tickets from the selection list, confirming the assignment, and asserting that the ticket transitions to `InReview`, disappears from the unassigned queue, and appears in the official's pending inbox.

**Acceptance Scenarios**:

1. **Given** an unassigned ticket displayed in the administrator's assignment view, **When** the administrator selects the ticket, chooses an active `Funcionario` having less than 5 active tickets, and confirms the assignment, **Then** the system associates the ticket with the chosen official, updates its status to `InReview` ("En Trámite"), records an assignment audit log with the administrator's identity, timestamp, and optional note, and removes the ticket from the unassigned list.
2. **Given** a successfully assigned ticket, **When** the assigned `Funcionario` accesses their personal dashboard/inbox (`/dashboard`), **Then** the newly assigned PQRSDF appears in their active workload table (up to 5 items) ordered by nearest legal deadline, displaying radicado number, request type, citizen subject, assignment date, color-coded remaining business days badge, and any administrative instruction notes provided.
3. **Given** an official who currently has 5 active assigned PQRSDF requests (`InReview`), **When** the administrator opens the assignment selector, **Then** that official is clearly indicated as having reached full capacity (5/5), cannot be selected, and attempts to force assignment to that official are rejected by backend validation with an informative Spanish error message ("El funcionario seleccionado ha alcanzado el límite máximo de 5 solicitudes asignadas activas.").
4. **Given** an assignment attempt where the selected official has been deactivated or does not exist, **When** the administrator confirms the assignment, **Then** the system rejects the operation, presents an error message in Spanish, and retains the ticket in the unassigned queue without partial updates.

---

### User Story 3 - Reassignment with Mandatory Justification and Workload Balancing (Priority: P2)

As an authenticated Administrator,  
I want to view each official's current open workload count and reassign a ticket to another eligible official by providing a mandatory justification,  
So that work can be redistributed when an official is absent or overloaded, maintaining complete auditability of the transfer.

**Why this priority**: Work conditions change (medical leaves, unexpected complexity, workload spikes). Administrators must have the ability to rebalance active assignments without breaking audit traceability.

**Independent Test**: Can be tested by selecting an already-assigned ticket, choosing a different official with available capacity (< 5 tickets), entering a valid justification (>= 10 characters), confirming the reassignment, and verifying that the ticket moves to the new official's inbox and an audit record is stored with the justification text.

**Acceptance Scenarios**:

1. **Given** the administrator opening the official assignment selector, **When** the list of eligible officials loads, **Then** each official entry displays their full name, email, and their current workload ratio (e.g., `3/5 solicitudes activas`), visually distinguishing those with available capacity from those at max capacity.
2. **Given** an Administrator managing assignments, **When** they navigate to the "En Trámite" tab or search by radicado number, **Then** they can select any ticket currently in `InReview` assigned to Official A, choose Official B (workload < 5), enter a mandatory justification of at least 10 characters, and confirm the reassignment, causing the system to update the assigned official to Official B, persist the justification in the audit log, move the ticket to Official B's inbox, and update both officials' active workload counts accordingly.
3. **Given** a reassignment attempt where the administrator leaves the justification empty or enters fewer than 10 characters, **When** submission is attempted, **Then** the system prevents submission and displays an inline validation message in Spanish ("Debe ingresar un motivo de reasignación de al menos 10 caracteres.").
4. **Given** an administrator managing tickets, **When** attempting to unassign a ticket back to null/unassigned without designating a new official, **Then** the system blocks the action, as every reassigned ticket must have a designated responsible official.

---

### Edge Cases

- **Workload Limit Race Condition**: Two administrators simultaneously assign different tickets to the same official who currently has 4 active tickets. The first assignment commits, raising the official's workload to 5/5. The second concurrent assignment transaction fails validation ("El funcionario ha alcanzado el cupo máximo de 5 solicitudes") and rolls back, prompting the second administrator to select an alternative official.
- **Concurrent Assignment Collision on the Same Ticket**: Two administrators attempt to assign the same unassigned ticket at the same time to different officials. The first assignment commits and transitions the ticket to `InReview`; the second attempt is rejected with a concurrency conflict message in Spanish indicating the ticket was already assigned, refreshing the queue view.
- **Official Deactivation during Selection**: An official is deactivated in user management while an administrator has the assignment modal open. Upon submitting the assignment, the system validates active status, rejects the assignment with an explanation ("El funcionario seleccionado no se encuentra activo"), and prompts the administrator to select another official.
- **Tickets Nearing Expiration in Queue**: Unassigned tickets with 3 or fewer Colombian business days remaining before their legal deadline are visually highlighted with an urgency badge ("Próximo a Vencer") to ensure administrators prioritize their distribution.
- **Network Disruption during Assignment**: If the network connection drops while submitting the assignment action, client retry logic prevents duplicate audit records and verifies the server state gracefully.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST restrict access to the unassigned tickets queue and the ticket assignment/reasignment actions strictly to authenticated users with the `Administrador` role.
- **FR-002**: System MUST reject any attempt by users with role `Funcionario` or unauthenticated visitors to access assignment UI views or backend endpoints, returning HTTP 403 Forbidden or 401 Unauthorized.
- **FR-003**: System MUST provide an unassigned tickets queue displaying key ticket metadata (Radicado Number, Request Type, Destination Area, Citizen Subject, Filing Date, Calculated Due Date, and Remaining Colombian Business Days), sorted by default by nearest legal due date (ascending remaining business days) and offering interactive filters by Request Type and Destination Area.
- **FR-004**: System MUST provide a roster of assignable officials containing only active users (`IsActive = true`) with the role `Funcionario`, indicating for each official their current count of active assigned tickets (`InReview`) out of the maximum allowed capacity of 5 (e.g., "3/5").
- **FR-005**: System MUST enforce a strict workload capacity limit of **5 active tickets** per official. Officials with 5 active tickets MUST be disabled for new assignments in the UI and rejected with a domain error if submitted to the backend.
- **FR-006**: System MUST automatically transition the ticket lifecycle status from `Registered` to `InReview` ("En Trámite") upon successful assignment to an official.
- **FR-007**: System MUST allow administrators to reassign an already-assigned ticket (`InReview`) to a different active official who has available capacity (< 5 active tickets), requiring a mandatory justification note of at least 10 characters.
- **FR-008**: System MUST forbid unassigning a ticket to a null/unassigned state once assigned; every ticket in progress must retain an accountable official until closure.
- **FR-009**: System MUST persist an immutable audit trail (`TicketAssignmentHistory`) for every assignment and reassignment, recording: Ticket ID, Previous Assigned User ID (null for initial assignment), New Assigned User ID, Administrator User ID, Timestamp in UTC, and Assignment/Justification Note.
- **FR-010**: System MUST immediately reflect the assignment, causing the ticket to appear in the assigned official's pending inbox (`InReview`) and vanish from the unassigned list.
- **FR-011**: System MUST support an optional administrative instruction note (up to 500 characters) upon initial assignment, and require a mandatory justification note (10 to 500 characters) upon reassignment.
- **FR-012**: System MUST present all client-facing user interfaces, table columns, badges, buttons, modal dialogs, and error messages strictly in Spanish.
- **FR-013**: System MUST provide a quick preview panel (drawer) or modal when an unassigned ticket is selected, rendering the citizen's complete description, subject, request type, destination area, and statutory due dates alongside the official assignment controls without navigating away from the queue.
- **FR-014**: System MUST provide an assigned workload inbox in `/dashboard` for authenticated users with role `Funcionario`, rendering their active assigned requests (`InReview`, up to 5) sorted by nearest statutory due date, displaying radicado number, request type, citizen subject, assignment date, color-coded remaining business days badge, and any administrative assignment notes.
- **FR-015**: System MUST calculate and render remaining business day badges adhering strictly to 3 urgency thresholds: Red (Crítico/Vencido: ≤ 3 remaining business days or overdue), Yellow (Atención: 4 to 7 remaining business days), and Green (A tiempo: ≥ 8 remaining business days) across both administrator and official views.
- **FR-016**: System MUST organize the administrator's assignment view with two distinct operational tabs: "Sin Asignar" (unassigned distribution queue) and "En Trámite" (assigned tickets undergoing processing), both equipped with an interactive radicado search input to facilitate ticket lookup and reassignment drawer invocation.

---

### Key Entities *(include if feature involves data)*

- **Ticket / Radicado**: Represents the citizen's PQRSDF submission undergoing lifecycle processing.
  - Relevant attributes: `RadicadoNumber`, `Status` (`Registered`, `InReview`, etc.), `DestinationAreaId`, `AssignedToUserId` (nullable user identifier of the assigned official), `AssignedAtUtc` (nullable timestamp), `AssignmentNote` (optional administrative note).
- **User (Funcionario)**: Internal staff member eligible to receive and process PQRSDF tickets.
  - Relevant attributes: `Id`, `FullName`, `Email`, `Role` (`Funcionario`), `IsActive`.
  - Computed/queried domain concept: `ActiveTicketsCount` (number of tickets in `InReview` currently assigned to this user, with invariant `ActiveTicketsCount <= 5`).
- **TicketAssignmentHistory / AuditLog**: Immutable record of assignment and transfer events.
  - Relevant attributes: `Id`, `TicketId`, `PreviousAssignedUserId` (nullable), `NewAssignedUserId`, `AssignedByUserId` (administrator identifier), `AssignedAtUtc`, `Reason` / `Note`.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of unauthorized requests (from unauthenticated users or users with role `Funcionario`) to assignment endpoints are denied with HTTP 401 Unauthorized or 403 Forbidden.
- **SC-002**: 100% of successful initial assignments automatically update the ticket status to `InReview` and link the responsible official.
- **SC-003**: 0% violations of the workload capacity limit: the system strictly prevents assigning any ticket to an official who already has 5 active tickets in `InReview`.
- **SC-004**: 100% of ticket reassignments enforce and record a justification reason of at least 10 characters in the permanent audit history.
- **SC-005**: Administrators can filter unassigned tickets, inspect official workloads, and confirm assignment in under 30 seconds.
- **SC-006**: Assigned tickets appear in the chosen official's pending inbox within 1 second of assignment confirmation.
- **SC-007**: 100% of user interface elements, error notices, empty states, and validation feedback are rendered in Spanish.
- **SC-008**: 100% of urgency badges accurately reflect the 3 standard legal deadline thresholds (Red ≤ 3 days, Yellow 4-7 days, Green ≥ 8 days).

---

## Assumptions

- **Authentication and Authorization**: Feature `005-auth-user` provides the JWT authentication mechanism, user identity, and role claims (`Administrador` vs `Funcionario`).
- **Ticket Data Store**: Feature `003-pqrsdf-make-ticket` defines the `Ticket` aggregate and radicado numbering schema.
- **Official Inbox Feature Co-existence**: The official's inbox query filters tickets where `AssignedToUserId == currentUserId` and status is `InReview`.
- **Single Unified Relational Database**: As mandated by the PQRSDF Software Constitution, all ticket, assignment history, and user tables reside within the same relational database instance without separate message brokers.
- **Active Tickets Definition**: An active ticket count for workload calculation comprises all tickets assigned to the official that have not reached a final closed/resolved state (`InReview`).
