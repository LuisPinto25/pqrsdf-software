# Feature Specification: Public PQRSDF Ticket Consultation

**Feature Branch**: `feature/get-ticket`

**Created**: 2026-09-18

**Status**: Draft

**Input**: User description from `pqrsdf-docs/pqrsdf-get-ticket.md`:
> "### 2. Módulo de Consulta Pública
> Historia de Usuario: Como Ciudadano, quiero consultar el estado de mi solicitud usando mi número de radicado para hacerle seguimiento sin comprometer mi privacidad.
> Requerimientos:
> - Pantalla pública con un campo de entrada para el número de radicado.
> - El sistema debe mostrar: estado actual, fecha de radicación, fecha de vencimiento y días restantes.
> - Si la solicitud ya fue cerrada, debe mostrar el contenido de la respuesta.
> - No se debe exponer ningún dato sensible o personal del solicitante.
> Criterios de Aceptación:
> - Si el usuario ingresa un radicado existente, el sistema devuelve exclusivamente la línea de tiempo y estado.
> - Si la solicitud tiene respuesta final, el texto es visible en esta pantalla.
> - La UI no expone nombre, correo, ni documento del creador de la solicitud bajo ninguna circunstancia."

---

## Clarifications

### Session 2026-09-18

- Q: ¿Cuál es el nivel de visibilidad del asunto y la descripción original del ciudadano en la consulta pública? → A: Exposición completa de contenido; se muestran asunto, descripción original del ciudadano, estado, línea de tiempo y respuesta institucional, garantizando la ocultación estricta y absoluta de los datos personales y de contacto del solicitante (nombre completo, tipo y número de documento, correo electrónico, teléfono).
- Q: ¿Deben ser visibles las justificaciones intermedias de cambio de estado en la línea de tiempo pública? → A: Hitos limpios y respuesta final única; la línea de tiempo muestra exclusivamente el nombre de cada estado y su fecha de ocurrencia, manteniendo las justificaciones intermedias como registro de auditoría interna y exponiendo al ciudadano únicamente el texto oficial de la respuesta final de cierre.
- Q: ¿Cómo debe estructurarse la navegación y la ruta pública de consulta en el frontend? → A: Bajo la ruta dedicada `/pqrsdf/search` con soporte de parámetro de consulta `?radicado=...` (ej. `/pqrsdf/search?radicado=YYYY-NNNNNNNN`), permitiendo tanto la búsqueda interactiva mediante campo de entrada como el enlace directo o prellenado desde otras vistas.
- Q: ¿Cómo debe presentarse visualmente una solicitud que superó su fecha límite sin respuesta? → A: Alerta explícita de mora; el sistema muestra una insignia destacada ("Vencida") y el número de días hábiles transcurridos tras el vencimiento (ej. "Vencida hace X días hábiles") con estilo visual de advertencia.
- Q: ¿Cómo debe protegerse la consulta pública contra escaneo masivo o fuerza bruta? → A: Límite de tasa por IP (Rate Limiting); restricción de hasta 30 consultas por minuto por dirección IP, devolviendo una respuesta estándar 429 con mensaje amigable en español si se supera el umbral.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Public Status and Timeline Consultation by Radicado Number (Priority: P1)

As an unauthenticated citizen,  
I want to look up my PQRSDF request by typing my unique radicado tracking number into a public search view,  
So that I can verify its current operational status, filing date, legal due date, remaining business days, and historical timeline without logging in or creating an account.

**Why this priority**: Tracking filed requests is the core citizen right after submission. Without a public tracking mechanism, citizens cannot know the state of their requests or verify compliance with statutory deadlines.

**Independent Test**: Can be tested independently by navigating to the public tracking view, entering an existing radicado number (e.g., `2026-00000001`), submitting the query, and verifying that the system renders the ticket's current status, filing date, due date, calculated remaining business days, and the progress timeline.

**Acceptance Scenarios**:

1. **Given** an unauthenticated citizen on the public tracking view, **When** they enter a valid, existing radicado number and trigger the search, **Then** the system displays the ticket's current status, request type, destination area, subject, description, filing date, statutory response due date, remaining Colombian business days, and the visual progress timeline.
2. **Given** an unauthenticated citizen on the tracking view, **When** they enter a non-existent radicado number or one that does not match any record, **Then** the system displays a clear, courteous message in Spanish stating that no request was found with that radicado, allowing them to verify and try again.
3. **Given** an unauthenticated citizen on the tracking view, **When** they enter an invalid radicado format (e.g., letters, missing hyphens, or wrong digit length), **Then** the interface provides immediate inline feedback explaining the required format (`YYYY-NNNNNNNN`) before querying.
4. **Given** a radicado number entered with leading or trailing whitespace, **When** submitted, **Then** the system automatically trims the input and processes the search seamlessly.

---

### User Story 2 - Final Response and Closure Details Display (Priority: P1)

As a citizen checking a resolved or closed PQRSDF request,  
I want to view the official response text and closure date directly in the consultation view,  
So that I can read the final answer and formal resolution provided by the institution.

**Why this priority**: For resolved or closed tickets, receiving and reviewing the institutional answer is the ultimate objective and conclusion of the citizen's filing journey.

**Independent Test**: Can be tested by searching for a radicado associated with a closed or answered ticket and confirming that the response text and answer date are clearly presented in the consultation interface.

**Acceptance Scenarios**:

1. **Given** a PQRSDF ticket with status "Cerrado" (Closed) or "Respondido" (Answered), **When** the citizen consults the radicado, **Then** the consultation view displays the full official response text, response date, and indicates that the case has been completed.
2. **Given** a PQRSDF ticket currently in an active or pending status (e.g., "Registrado", "Asignado", "En trámite"), **When** the citizen consults the radicado, **Then** the response section is hidden or clearly indicates that the request is currently being processed and pending response.

---

### User Story 3 - Absolute Applicant Privacy and Data Protection (Priority: P1)

As a citizen consulting a request in a public space or shared computer,  
I want to ensure that none of my personal or contact details are revealed in the public tracking query,  
So that my privacy, identity, and personal data (PII) are completely safeguarded against unauthorized third-party visibility.

**Why this priority**: The public tracking portal requires only the radicado number and no authentication. Therefore, strict privacy compliance is essential to prevent anyone guessing or obtaining a radicado number from seeing who filed the request.

**Independent Test**: Can be tested by querying tickets filed both with full citizen data and anonymously, inspecting both the user interface and the underlying API response payloads, and verifying that applicant name, document type, document number, email, and phone number are completely absent.

**Acceptance Scenarios**:

1. **Given** any existing radicado query processed through the public portal, **When** reviewing the displayed interface and the data transmitted from the server, **Then** under no circumstances is applicant name, document type, document number, email, or telephone number included or exposed.
2. **Given** an anonymous ticket or an identified ticket, **When** displayed in the public tracking view, **Then** both render identical privacy-safe fields showing only operational timeline and status data.

---

### User Story 4 - Statutory Remaining Days and Expiration Alerting (Priority: P2)

As a citizen tracking my request timeline,  
I want to see the remaining official business days until the legal deadline, and an explicit indication if the ticket has exceeded its statutory deadline,  
So that I can accurately determine whether the entity is still within the legal timeframe or if the petition is overdue.

**Why this priority**: Colombian administrative law (Law 1755 of 2015) sets strict terms for public entities. Providing clear business day counting builds institutional transparency.

**Independent Test**: Can be tested with tickets having due dates in the future (displaying positive remaining business days) and tickets with past due dates (displaying overdue warning and days elapsed past deadline).

**Acceptance Scenarios**:

1. **Given** an open ticket whose due date is in the future, **When** consulted, **Then** the system calculates and displays the exact number of remaining Colombian business days (excluding weekends and statutory holidays).
2. **Given** an open ticket whose due date has passed without resolution or closure, **When** consulted, **Then** the system prominently displays an overdue alert badge ("Vencida") along with the explicit count of Colombian business days elapsed past the statutory deadline (e.g., "Vencida hace X días hábiles").
3. **Given** a closed ticket, **When** consulted, **Then** the system indicates that the ticket was closed and does not show active countdowns.

---

### Edge Cases

- **Radicado format malformed**: Radicado input strings that do not conform to `YYYY-NNNNNNNN` (e.g., `1234`, `abc`, `2026-123`) are rejected at the client level before making a request, and also validated on the server with a clear 400 Bad Request if bypassed.
- **Radicado not found**: When a well-formed radicado number does not match any existing record in the database, the system returns a safe, uniform 404 response with a friendly Spanish notification: "No se encontró ninguna solicitud con el radicado ingresado. Por favor verifique el número e intente de nuevo."
- **Rapid repeated searches / Click spamming**: If a citizen clicks the search button repeatedly or rapidly submits searches, the UI disables the search button while loading and displays a loading spinner to prevent duplicate calls.
- **Accidental copy-paste of spaces or punctuation**: If a citizen pastes a radicado with leading/trailing spaces or tabs, the system sanitizes and trims the input automatically.
- **Tickets filed on non-working days or outside business hours**: The filing date displayed is the official Colombian calendar date of filing, and the timeline reflects the official progression milestones.
- **Long response text**: Final responses containing multiple paragraphs are rendered cleanly with readable formatting, line breaks, and proper overflow management.
- **Rate limit exceeded**: When requests from a single client IP address exceed 30 requests per minute, the service returns an HTTP 429 Too Many Requests response with a user-friendly Spanish message: "Ha superado el límite de consultas permitidas por minuto. Por favor espere un momento antes de intentar de nuevo."

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a public consultation page accessible without requiring user authentication, login credentials, or account creation.
- **FR-002**: The consultation interface MUST provide an input field allowing the citizen to enter a radicado tracking number.
- **FR-003**: The system MUST validate the radicado number format against the standard `YYYY-NNNNNNNN` structure (4 digits, hyphen, 8 digits), automatically trimming whitespace.
- **FR-004**: The system MUST strictly OMIT all Personally Identifiable Information (PII) of the applicant from both public query responses and the user interface. Under no circumstances shall full name, identification type, identification number, email, or phone number be exposed.
- **FR-005**: For an existing radicado, the system MUST retrieve and present the following operational details:
  - Radicado Number
  - Request Type (*Petición*, *Queja*, *Reclamo*, *Sugerencia*, *Denuncia*, *Felicitación*)
  - Assigned Destination Area
  - Subject (*Asunto*)
  - Description (*Descripción* original del ciudadano)
  - Filing Date (Official Colombian date)
  - Legal Due Date (Calculated statutory expiration date)
  - Current Lifecycle Status (*Registrado*, *Asignado*, *En trámite*, *Respondido*, *Cerrado*)
  - Remaining Colombian business days until expiration (for active tickets within terms), or overdue alert indicator with elapsed business days in mora (for open tickets whose deadline has elapsed, e.g., "Vencida hace X días hábiles").
- **FR-006**: The system MUST render an intuitive visual timeline representing the lifecycle stages of the request (*Registrado*, *Asignado*, *En trámite*, *Respondido*, *Cerrado*), displaying exclusively the milestone status name, date of transition, completed stages, current active stage, and pending future stages. Internal officer justifications for state transitions MUST remain strictly private for internal audit and shall NOT be exposed in the public timeline.
- **FR-007**: When the ticket has reached a resolved or closed state (*Respondido* or *Cerrado*), the system MUST display:
  - Official response text provided by the institution
  - Response / Closure date
- **FR-008**: When a radicado number is not found, the system MUST display a clear and user-friendly message in Spanish indicating that no record was found, without exposing internal database or infrastructure errors.
- **FR-009**: All user-facing text, field labels, status descriptions, timeline milestones, and error messages MUST be rendered strictly in Spanish.
- **FR-010**: The public tracking module MUST be accessible under the dedicated route `/pqrsdf/search`, supporting interactive searches as well as direct URL query parameter resolution (`/pqrsdf/search?radicado=YYYY-NNNNNNNN`), with clear navigation links from the main portal and the PQRSDF area.
- **FR-011**: The system MUST enforce an IP-based rate limit of maximum 30 requests per minute on public ticket consultation queries to protect the service against brute-force radicado enumeration and automated denial of service.

---

### Key Entities *(include if feature involves data)*

- **PublicTicketStatus** (Conceptual Read Model):
  - `RadicadoNumber`: The unique official tracking code (`YYYY-NNNNNNNN`).
  - `RequestType`: The PQRSDF classification (e.g., Petición, Queja, Reclamo, Sugerencia, Denuncia, Felicitación).
  - `DestinationAreaName`: Name of the organizational unit handling the request.
  - `Subject`: Concise summary of the request as filed.
  - `Description`: Detailed narrative of the request as filed.
  - `Status`: Current operational state (*Registrado*, *Asignado*, *En trámite*, *Respondido*, *Cerrado*).
  - `FilingDate`: Official date of filing (Colombian time zone).
  - `DueDate`: Official maximum legal response deadline.
  - `RemainingBusinessDays`: Integer indicating remaining business days (or null/0 when overdue or closed).
  - `IsOverdue`: Boolean indicating if an open ticket is past its statutory due date.
  - `OverdueBusinessDays`: Integer indicating Colombian business days elapsed past statutory deadline when `IsOverdue` is true.
  - `Timeline`: Ordered collection of milestone events (`TicketTimelineMilestone`).
  - `Resolution`: Optional response details when resolved (`TicketResolution`), containing response text and response date.

- **TicketTimelineMilestone**:
  - `Status`: Milestone status identifier.
  - `Title`: Descriptive milestone title in Spanish (e.g., "Radicado", "Asignado", "En trámite", "Respuesta emitida").
  - `Date`: Timestamp when the milestone was reached (if reached).
  - `IsCompleted`: Boolean indicating if this milestone has already occurred.
  - `IsCurrent`: Boolean indicating if this is the active operational stage.

- **TicketResolution**:
  - `ResponseText`: The official response text provided to the citizen.
  - `ResponseDate`: Timestamp when the final answer was recorded.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Citizens can query and visualize their ticket status and timeline in under 2 seconds from submitting a valid radicado number under normal network conditions.
- **SC-002**: 0% exposure of citizen applicant personal data (name, identification, email, phone) in public API endpoints and UI views, verified by automated tests.
- **SC-003**: 100% of queries for existing radicado numbers display accurate filing dates, due dates, current status, and correct remaining business days.
- **SC-004**: 100% of closed or answered tickets display the full official response text and closure date in the public view.
- **SC-005**: 100% of non-existent or malformed radicado lookups present user-friendly, descriptive Spanish feedback without unhandled exceptions or crashes.
- **SC-006**: 100% of user interface elements, labels, and feedback messages are written in grammatically correct Spanish.
- **SC-007**: 100% of automated or excessive queries from any single IP exceeding 30 requests per minute receive an HTTP 429 response with friendly Spanish guidance, preventing brute-force radicado enumeration without service disruption.

---

## Assumptions

- **Authentication Freedom**: Public consultation requires no citizen login, password, or session token; the radicado number is the sole lookup identifier.
- **Information Privacy Boundary**: To balance transparency with privacy, public tracking exposes request metadata (Radicado, Request Type, Area, Status, Timeline, Dates, Remaining Days, and Final Response text upon closure), but strictly strips and omits all applicant personal details (Name, Document, Email, Phone).
- **Business Day Calculation**: Remaining days computation adheres strictly to official Colombian statutory non-working holidays and weekends (per Law 51 of 1983 and Law 1755 of 2015), ensuring consistency with the deadline calculation made during ticket registration.
- **Direct Link Support / Query Parameter**: The consultation view is accessible at `/pqrsdf/search`, supporting entering the radicado directly in the search field as well as receiving a prefilled radicado via query parameter (e.g., `/pqrsdf/search?radicado=2026-00000001`), facilitating direct access from confirmation receipts or bookmarking.
- **Responsive Layout**: The consultation and timeline view adapts cleanly to mobile devices, tablets, and desktop displays.
