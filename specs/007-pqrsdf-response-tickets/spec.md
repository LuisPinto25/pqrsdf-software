# Feature Specification: PQRSDF Ticket Management and Official Response

**Feature Branch**: `007-pqrsdf-response-tickets`

**Created**: 2026-09-18

**Status**: Draft

**Input**: User description from `pqrsdf-docs/pqrsdf-response-tickets.md`:
> "### 5. Módulo de Gestión y Respuesta (Funcionario)
> 
> **Historia de Usuario:** Como Funcionario, quiero abrir el detalle de una solicitud para cambiar su estado con una justificación, o registrar la respuesta final para dar por cerrado el caso.
> 
> **Requerimientos:**
> - Vista de detalle accesible desde la bandeja.
> - Permitir cambiar el estado de la solicitud, exigiendo obligatoriamente un texto de justificación.
> - Permitir registrar la respuesta final al ciudadano.
> - Al registrar la respuesta, el estado de la solicitud debe cambiar a cerrado.
> 
> **Criterios de Aceptación:**
> - Si el funcionario intenta cambiar el estado sin proveer justificación, el sistema arroja error de validación.
> - Al enviar la respuesta final, la PQRSDF ya no suma días de gestión y pasa a estado cerrado.
> - El ciudadano puede ver inmediatamente el cambio de estado y la respuesta en el módulo público."

---

## Clarifications

### Session 2026-09-19

- **Q1: Transiciones de Estado Disponibles en el Cambio Manual de Estado**: ¿Qué estados destino deben estar disponibles en el selector durante un cambio manual de estado?  
  → **A**: **Opción A — Diferenciación estricta entre estados de trámite y cierre definitivo**. El cambio manual de estado permite gestionar y registrar avances de tramitación interna sobre estados activos (`InReview` / "En Trámite" con justificación obligatoria del hito u observación operativa), reservando el estado `Closed` ("Cerrado") de forma exclusiva para el acto formal de registrar y despachar la respuesta institucional definitiva al ciudadano.
- **Q2: Alcance de Autorización para Gestionar y Responder Solicitudes**: ¿Quiénes tienen autorización para abrir las acciones de gestión, cambiar el estado y registrar la respuesta final de un radicado en particular?  
  → **A**: **Opción A — Funcionario Asignado y Administrador exclusivamente**. La autorización para mutar el radicado (cambiar de estado/registrar justificaciones operativas y formalizar la respuesta definitiva) está restringida con estricto control de acceso al funcionario que tiene asignado el radicado (`AssignedToUserId == CurrentUserId`) y a usuarios con rol `Administrador`. Cualquier intento de otro funcionario es rechazado con error de autorización (HTTP 403 Forbidden).
- **Q3: Persistencia y Visibilidad de las Justificaciones de Estado**: ¿Cómo y dónde deben persistirse las justificaciones de cambio de estado, y qué nivel de visibilidad tendrán en el panel interno vs. la consulta ciudadana pública?  
  → **A**: **Opción A — Historial de auditoría interno exclusivo**. Las justificaciones de cambio de estado se persisten como registros de auditoría inmutables (incluyendo estado anterior, nuevo estado, justificación obligatoria, usuario responsable y timestamp UTC). Este historial es visible exclusivamente dentro del panel interno de gestión (para funcionarios y administradores). En el portal público de consulta ciudadana (`/pqrsdf/search`), únicamente se visualiza el estado actual y la respuesta definitiva, resguardando la confidencialidad de notas y deliberaciones técnicas internas.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Full Ticket Detail from Official Inbox (Priority: P1)

As an authenticated Official (Funcionario),  
I want to open and inspect the full operational and citizen details of an assigned PQRSDF from my inbox,  
So that I can thoroughly understand the request, review citizen statements and legal deadlines, and determine the necessary course of action.

**Why this priority**: Opening and examining the ticket details is the essential prerequisite for analyzing citizen needs and performing any subsequent status changes or formal responses.

**Independent Test**: Can be tested independently by logging in as an Official, selecting an assigned ticket in the personal inbox table, and verifying that the detail view opens showing full citizen text, applicant identity (or anonymous flag), filing date, legal deadline, remaining SLA business days, destination area, and assignment instructions.

**Acceptance Scenarios**:

1. **Given** an authenticated Official on their personal inbox (`/dashboard`), **When** they click "Gestionar" or select a ticket row, **Then** the system opens the full detail view (slide-over drawer or modal) displaying the complete citizen narrative, radicado number, category, destination area, filing timestamp, legal due date, SLA badge, citizen details (or anonymous label), and received administrative instructions.
2. **Given** an Official inspecting a ticket detail, **When** the ticket is assigned to another official or is not within the authorized scope, **Then** the detail view restricts mutation actions (disabling or hiding status change and response triggers) or denies access in accordance with authorization rules.

---

### User Story 2 - Register Final Official Response and Close Ticket (Priority: P1)

As an authenticated Official responsible for an assigned PQRSDF,  
I want to write and submit the institutional final response to the citizen,  
So that the citizen's petition is legally resolved, the ticket status changes to "Cerrado" (Closed), and the calculation of business days in progress halts immediately.

**Why this priority**: Delivering the institutional response and legally concluding the petition is the ultimate goal of the PQRSDF lifecycle and satisfies statutory deadlines.

**Independent Test**: Can be tested independently by opening an assigned ticket in `InReview` status, entering a valid response text, submitting the resolution, and verifying that the ticket transitions to `Closed`, records the completion timestamp, stops incrementing management days, releases official active workload capacity, and displays the resolution in citizen tracking.

**Acceptance Scenarios**:

1. **Given** an authorized Official viewing an assigned ticket in `InReview` status, **When** they enter a formal institutional response of valid length (between 10 and 4,000 characters) and submit it, **Then** the system saves the response, marks the ticket status as `Closed`, sets the resolution timestamp to the current date and time, halts any further SLA business day counting, and reduces the official's active workload count by 1.
2. **Given** an Official attempting to register a final response, **When** they submit an empty, whitespace-only, or text shorter than 10 characters (or exceeding 4,000 characters), **Then** the system prevents submission and displays an explicit validation error in Spanish explaining the length requirements.
3. **Given** a ticket that is already in `Closed` status, **When** any user attempts to submit another final response, **Then** the system rejects the operation with a conflict error indicating that the ticket is already closed.

---

### User Story 3 - Change Ticket Status with Mandatory Justification (Priority: P2)

As an authenticated Official,  
I want to update the operational status of an assigned ticket while providing an explanatory justification,  
So that team members, supervisors, and audit logs understand the operational progress or changes in processing condition.

**Why this priority**: Managing intermediate states provides visibility into complex workflows (such as pending external information or specialized technical analysis) before the final resolution is issued.

**Independent Test**: Can be tested independently by selecting a new allowed status on an active ticket, providing a justification note, submitting the change, and confirming that the ticket status updates and the justification is persisted.

**Acceptance Scenarios**:

1. **Given** an authorized Official in the ticket detail view, **When** they select a new allowed target status and provide a justification of valid length (minimum 10 characters), **Then** the system updates the ticket status, records the change with timestamp and actor identity, and reflects the updated status across the interface.
2. **Given** an Official attempting to change the status, **When** they omit the justification or enter fewer than 10 characters, **Then** the system blocks the state change and displays a validation error in Spanish stating that a justification of at least 10 characters is mandatory.
3. **Given** a ticket in `Closed` status, **When** a user attempts to manually change its status, **Then** the system rejects the modification with an error indicating that closed tickets cannot have their status altered.

---

### User Story 4 - Citizen Real-Time Tracking of Status and Resolution (Priority: P2)

As a Citizen checking a filed PQRSDF through the public consultation portal (`/pqrsdf/search`),  
I want to immediately see updated ticket status and read the complete institutional response once resolved,  
So that I have full transparency regarding the outcome of my petition without delays or needing in-person inquiries.

**Why this priority**: Transparency is a core mandate of citizen public service; the citizen must have instant access to their case outcome while strictly protecting official and citizen sensitive data.

**Independent Test**: Can be tested independently by querying a radicado number in the public portal after an official registers a response or changes status, verifying that the new status appears instantly along with the resolution text and resolution date.

**Acceptance Scenarios**:

1. **Given** a citizen consulting their radicado number in the public search portal, **When** the ticket has been closed with an official response, **Then** the portal displays the status badge as "Cerrado" (Closed), presents the full institutional response text with its date of emission, shows the SLA timeline marked as completed, and does not show active overdue warnings or elapsed days accumulation.
2. **Given** a citizen consulting their radicado number, **When** the ticket underwent a status update prior to closure, **Then** the portal displays the current operational status in Spanish without leaking internal administrative notes or staff personal data.

---

### Edge Cases

- What happens if the Official's session expires while drafting a long response text? The frontend warns or retains draft text in client-side state so the user does not lose their typed response upon re-authenticating.
- What happens if two officials or an administrator and official attempt to submit a status update or response simultaneously? The system processes the first transaction and rejects subsequent concurrent modifications on already closed/updated tickets with a concurrency or conflict message in Spanish.
- What happens if a ticket is overdue when the final response is submitted? The system records the response and closes the case; the final elapsed overdue days at moment of resolution remain locked, and no further overdue days accrue.
- What happens if the Official enters text containing malicious script tags or SQL sequences? The system sanitizes input at both client and server boundaries, treating all text strictly as plain text.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an accessible detail view for any ticket listed in the Official's inbox (`/dashboard`), triggered by clicking the row or an explicit "Gestionar" action.
- **FR-002**: The detail view MUST display the radicado number, request type, destination area, citizen subject, full citizen description, filing date, legal due date, and dynamic SLA badge (remaining business days and urgency level).
- **FR-003**: System MUST allow an authorized Official to change the operational status of an assigned ticket, strictly requiring a textual justification of at least 10 characters and at most 500 characters.
- **FR-004**: System MUST reject any status change attempt if the justification is empty, whitespace-only, or shorter than 10 characters, returning an explicit validation error in Spanish.
- **FR-005**: System MUST allow an authorized Official to register a formal final response to the citizen with a length between 10 and 4,000 characters.
- **FR-006**: Upon registering the final response, the system MUST automatically transition the ticket status to `Closed` (`Cerrado`) and record the response timestamp in UTC.
- **FR-007**: Once a ticket is in `Closed` status, the system MUST immediately halt any calculation or accumulation of SLA management days and overdue business days.
- **FR-008**: Once a ticket transitions to `Closed`, the system MUST decrement the assigned official's active workload count, freeing capacity for future assignments under the 5-ticket workload limit.
- **FR-009**: The public consultation module (`/pqrsdf/search`) MUST immediately reflect the updated status and display the institutional response text and resolution date when queried by radicado number.
- **FR-010**: System MUST enforce strict separation between operational status updates and formal case closure: manual status updates are restricted to active in-progress stages (`InReview` / "En Trámite" with mandatory stage note or internal justification), strictly reserving the `Closed` (`Cerrado`) status for the dedicated workflow of submitting the formal institutional response to the citizen.
- **FR-011**: System MUST enforce strict ownership and role authorization: only the specific Official assigned to the ticket (`AssignedToUserId`) and users with the `Administrador` role are authorized to update operational status or submit the final response. Unauthorized access attempts MUST be rejected with HTTP 403 Forbidden.
- **FR-012**: System MUST persist all status changes and their mandatory justifications in an immutable audit history log (`TicketStatusHistory`), recording ticket ID, previous status, new status, responsible actor ID, justification text (10-500 chars), and UTC timestamp. This history MUST be viewable in the internal ticket management drawer/view by Officials and Administrators, and MUST NOT be exposed in the public citizen consultation interface.

### Key Entities *(include if feature involves data)*

- **PqrsdfTicket**: Aggregate root representing the filing. Key operational attributes for this feature:
  - `Status`: Current lifecycle status (`Registered`, `Assigned`, `InReview`, `Answered`, `Closed`).
  - `ResponseText`: Full text of the institutional answer delivered to the citizen (10-4,000 chars).
  - `ResponseDateUtc`: Timestamp when the final response was registered.
  - `AssignedToUserId`: Identifier of the assigned official responsible for the ticket.
- **TicketStatusHistory / AuditLog**: Record documenting status transitions and administrative events:
  - `TicketId`: Reference to the affected ticket.
  - `PreviousStatus`: Status prior to the modification.
  - `NewStatus`: New status applied.
  - `ChangedByUserId`: Identifier of the user who executed the change.
  - `Justification`: Mandatory textual explanation for the change (10-500 chars).
  - `ChangedAtUtc`: Exact timestamp of the event.
- **TicketResolution**: Data contract encapsulated within the ticket or public read model:
  - `ResponseText`: Text delivered to the citizen.
  - `ResponseDate`: Date of response delivery.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Authorized officials can complete and submit a final response in less than 30 seconds from opening the ticket detail view.
- **SC-002**: 100% of tickets closed with an official response halt SLA calculation and stop accumulating management days immediately upon submission.
- **SC-003**: 100% of attempts to change ticket status without a justification of at least 10 characters are rejected with immediate user-friendly feedback.
- **SC-004**: Citizens consulting the public portal can view the updated status and response within 1 second of submission without requiring cache purges or server restarts.
- **SC-005**: Closing an assigned ticket instantly updates the official's active workload badge (e.g. from 5/5 to 4/5) without page reload.

---

## Assumptions

- Officials access their assigned tickets through the existing authenticated dashboard inbox (`/dashboard`).
- The legal calculation of Colombian business days and holidays already implemented in the domain service is reused to freeze SLA metrics upon closure.
- Response text is provided as formatted plain text; rich multimedia attachments are out of scope for this MVP phase unless clarified.
- The citizen accesses the response using their radicado number through the public search interface (`/pqrsdf/search`) without requiring login credentials.
