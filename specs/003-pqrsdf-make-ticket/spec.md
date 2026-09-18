# Feature Specification: Public PQRSDF Ticket Registration

**Feature Branch**: `feature/make-ticket`

**Created**: 2026-09-18

**Status**: Draft

**Input**: User description from `pqrsdf-docs/pqrsdf-make-ticket.md`:
> "Módulo de Radicación de PQRSDF (Acceso Público)
> Historia de Usuario: Como Ciudadano, quiero radicar una PQRSDF mediante un formulario público para comunicar mi solicitud, queja o sugerencia a la entidad sin necesidad de crear una cuenta.
> Requerimientos:
> - El formulario debe ser accesible sin autenticación.
> - Debe permitir seleccionar el tipo de solicitud (Petición, Queja, Reclamo, Sugerencia, Denuncia, Felicitación) y el área destino, cargada dinámicamente desde el backend.
> - Debe solicitar datos del solicitante, asunto y descripción.
> - El backend debe calcular la fecha de vencimiento en días hábiles (15 días por regla general, 30 días exclusivamente para denuncias).
> - El cálculo debe excluir sábados, domingos y días festivos colombianos.
> - Se debe generar un número de radicado único y concurrente con el formato `AAAA-NNNNNNNN`.
> Criterios de Aceptación:
> - Al enviar el formulario con datos válidos, el sistema persiste la información y devuelve una pantalla de confirmación.
> - La pantalla de confirmación muestra claramente el número de radicado generado y la fecha máxima de respuesta.
> - El sistema no genera duplicados en el formato de radicado, incluso bajo condiciones de concurrencia."

---

## Clarifications

### Session 2026-09-18

- Q: ¿Se permite la radicación anónima o la identificación es siempre obligatoria? → A: Radicación anónima opcional permitida exclusivamente para Denuncias y Sugerencias; para Peticiones, Quejas, Reclamos y Felicitaciones los datos del solicitante (nombre completo, tipo y número de documento, correo electrónico) son obligatorios.
- Q: ¿Cómo debe comportarse el consecutivo de radicado al cambiar de año calendario? → A: Reinicio anual; cada nuevo año (YYYY) la secuencia de 8 dígitos reinicia en 00000001 (e.g. 2026-00000001, 2027-00000001) según los estándares de gestión documental pública en Colombia.
- Q: ¿Cómo opera el inicio del cómputo de días hábiles según la fecha/hora de radicación? → A: Cómputo a partir del día hábil siguiente; la fecha de radicación se fija con la fecha calendario oficial en Colombia (UTC-5), y los 15 o 30 días hábiles inician su conteo a partir del primer día hábil inmediatamente posterior a la fecha de radicación.
- Q: ¿Qué formato de captura se utiliza para el asunto y la descripción? → A: Texto plano estricto con contador visual de caracteres; campo de entrada para asunto (5-150 caracteres) y área de texto multilinea para descripción (10-4000 caracteres), previniendo inyección de código y optimizando la experiencia móvil.
- Q: ¿Debe enviarse un correo electrónico de confirmación tras radicar la solicitud? → A: Entrega exclusivamente en pantalla de confirmación web; el número de radicado y la fecha máxima de respuesta se presentan en la pantalla de éxito con opción de copiar al portapapeles. El despacho de correos electrónicos transaccionales se difiere explícitamente a una feature posterior de notificaciones.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Public PQRSDF Ticket Registration (Priority: P1)

As an unauthenticated citizen,  
I want to submit a PQRSDF ticket through a public web form specifying the request type, destination area, applicant contact information, subject, and description,  
So that I can formally register my petition, complaint, claim, suggestion, denunciation, or compliment with the institution without creating an account or authenticating.

**Why this priority**: This is the core citizen-facing business function of the entire PQRSDF system. Without the ability to submit tickets publicly, citizens cannot exercise their constitutional right to petition.

**Independent Test**: Can be independently tested by accessing the public form in an incognito browser session, filling out all mandatory fields with valid data, submitting the request, and verifying that the ticket is stored with an initial "Registered" status and the response confirms receipt.

**Acceptance Scenarios**:

1. **Given** an unauthenticated citizen accessing the filing portal, **When** they fill in valid applicant information, select a standard request type (e.g., Petición), select a valid destination area, provide a subject and description, and submit, **Then** the ticket is successfully registered, assigned a unique radicado number in format `YYYY-NNNNNNNN`, assigned a due date calculated as 15 Colombian business days from submission, and persisted in the system.
2. **Given** an unauthenticated citizen filing a "Denuncia" (Denunciation/Report), **When** they submit all required valid details, **Then** the system registers the ticket and assigns a due date calculated specifically as 30 Colombian business days from submission.
3. **Given** an unauthenticated citizen filling out the form, **When** they omit any mandatory field (applicant name, identification type, identification number, email, request type, destination area, subject, or description) or provide an invalid email format, **Then** the form prevents submission, highlights the invalid fields, and displays clear validation feedback messages in Spanish.
4. **Given** multiple citizens submitting filings simultaneously under peak traffic, **When** concurrent requests are processed, **Then** every submission receives a strictly unique sequential radicado number without duplicate values, deadlocks, or data collisions.
5. **Given** an unauthenticated citizen selecting "Denuncia" or "Sugerencia", **When** they choose the option to file anonymously, **Then** the form disables and omits applicant personal contact fields, marks the ticket as anonymous, and registers the ticket successfully without storing personal contact data.

---

### User Story 2 - Dynamic Destination Area and Request Type Selection (Priority: P1)

As an unauthenticated citizen,  
I want the form to dynamically load active institutional destination areas and provide clear options for the six official PQRSDF types,  
So that I can accurately categorize my request and direct it to the appropriate institutional department.

**Why this priority**: Accurate routing and standardized categorization are essential prerequisites for valid ticket processing and legal SLA calculation.

**Independent Test**: Can be tested by opening the filing form and confirming that the destination area selection reflects the active organizational units provided by the catalog service, and that the six official PQRSDF types (Petición, Queja, Reclamo, Sugerencia, Denuncia, Felicitación) are available.

**Acceptance Scenarios**:

1. **Given** the citizen opens the filing form, **When** the page loads, **Then** the destination area field dynamically displays all active administrative units retrieved from the backend catalog.
2. **Given** the citizen interacts with the request type selector, **When** reviewing the available options, **Then** they can choose from exactly six types: Petición, Queja, Reclamo, Sugerencia, Denuncia, and Felicitación, each displaying a brief descriptive tooltip or label in Spanish.
3. **Given** a temporary communication failure when loading destination areas, **When** the citizen accesses the form, **Then** a friendly alert message in Spanish informs them of the connection issue with a retry button, avoiding an empty unhandled dropdown.

---

### User Story 3 - Immediate Filing Confirmation and Legal Receipt (Priority: P1)

As a citizen who has just submitted a PQRSDF,  
I want to see an immediate, clear confirmation screen containing my assigned radicado number and calculated maximum response deadline,  
So that I receive formal proof of receipt and know the exact legal date by which the entity must respond.

**Why this priority**: Legal certainty and citizen trust depend on receiving immediate confirmation and a traceable radicado number upon filing.

**Independent Test**: Can be tested by submitting a valid ticket and asserting that the resulting confirmation screen renders the exact radicado number, submission timestamp, and the calculated due date.

**Acceptance Scenarios**:

1. **Given** a successfully submitted PQRSDF, **When** the transaction completes, **Then** the citizen is immediately transitioned to a confirmation screen that prominently displays the assigned radicado number (`YYYY-NNNNNNNN`), the submission date, and the calculated maximum response date.
2. **Given** the citizen on the confirmation screen, **When** they inspect the view, **Then** they are presented with an option to copy the radicado number to their clipboard and clear guidance on how to track future updates.
3. **Given** a citizen on the confirmation screen, **When** they navigate away or refresh, **Then** the form does not resubmit the ticket, preventing accidental duplicate registrations.

---

### User Story 4 - Statutory Business Days Due Date Calculation (Priority: P2)

As a citizen applicant and compliance auditor,  
I want the expiration date to be computed strictly in official Colombian business days, excluding weekends and official Colombian national holidays,  
So that the due date strictly adheres to Colombian administrative legal frameworks (15 business days for general requests, 30 business days for denunciations).

**Why this priority**: Compliance with Colombian public administration regulations (Law 1755 of 2015 and Law 51 of 1983) is legally binding and prevents legal actions for untimely responses.

**Independent Test**: Can be tested across multiple calendar dates (including Fridays, holiday weekends, and Holy Week) to verify that weekends and official holidays are excluded and that the resulting deadline equals exactly the expected business days.

**Acceptance Scenarios**:

1. **Given** a standard petition submitted on a Friday preceding a Colombian holiday Monday (Emiliani Law), **When** the backend calculates the due date, **Then** Saturday, Sunday, and Monday are excluded from the 15-business-day count.
2. **Given** a denunciation submitted during any calendar month, **When** the backend calculates the due date, **Then** the system computes exactly 30 business days forward, skipping all intervening weekends and statutory holidays.

---

### Edge Cases

- **Submissions during non-business hours / Midnight**: When a ticket is submitted at any hour or during weekends/holidays, the filing timestamp is recorded in UTC and mapped to the official Colombian calendar filing date (UTC-5). In accordance with Colombian administrative law, the 15 or 30 business days computation always commences on the first Colombian business day immediately following the calendar filing date.
- **High concurrency on the sequence counter**: When multiple concurrent requests arrive simultaneously in the same millisecond, the database sequence generator must guarantee atomic, non-colliding, sequential `NNNNNNNN` values.
- **Year rollover**: When transitioning to a new calendar year (January 1st at 00:00 UTC-5), the radicado year prefix advances to the new year (`YYYY`) and the 8-digit sequence restarts at `00000001` (e.g., `2026-00000001` ... `2027-00000001`), guaranteeing independent annual sequences.
- **Double click / Fast resubmission**: If the citizen clicks the submit button multiple times in rapid succession, the user interface must disable the submit button immediately upon the first click and show a progress indicator to prevent duplicate submissions.
- **Accented characters and special punctuation**: Citizens submitting names and descriptions with Colombian Spanish characters (tildes `á`, `é`, `í`, `ó`, `ú`, `ñ`, `ü`, inverted question marks `¿`, etc.) must have their text preserved faithfully without corruption or encoding issues.
- **Potentially malicious content (HTML/Script tags)**: Text fields containing `<script>` or HTML tags must be sanitized to prevent cross-site scripting (XSS) while keeping harmless textual descriptions intact.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a public PQRSDF registration form accessible without requiring user authentication, login credentials, or account creation.
- **FR-002**: The system MUST dynamically retrieve and display active institutional destination areas from the backend catalog.
- **FR-003**: The system MUST provide exactly the six official PQRSDF request types:
  1. *Petición* (General Petition / Request)
  2. *Queja* (Complaint regarding service)
  3. *Reclamo* (Claim regarding dissatisfaction with an administrative decision)
  4. *Sugerencia* (Suggestion for improvement)
  5. *Denuncia* (Report of irregular or corrupt conduct)
  6. *Felicitación* (Compliment / Commendation)
- **FR-004**: The system MUST capture the following citizen applicant data for standard submissions:
  - Full Name (required for standard filing)
  - Identification Type (required for standard filing: Cédula de Ciudadanía `CC`, Cédula de Extranjería `CE`, Tarjeta de Identidad `TI`, Pasaporte `PA`, NIT `NIT`)
  - Identification Number (required for standard filing, alphanumeric)
  - Email Address (required for standard filing, valid email format)
  - Phone Number (optional, numeric format)
  If the request type is *Denuncia* or *Sugerencia*, the system MUST provide an explicit option to file anonymously, making all applicant personal data fields optional and omitted upon submission. For *Petición*, *Queja*, *Reclamo*, and *Felicitación*, applicant personal data remains strictly mandatory.
- **FR-005**: The system MUST capture the following ticket content data in strict plain text:
  - Request Type (required)
  - Destination Area (required)
  - Subject / Asunto (required, plain text single-line input, 5 to 150 characters, with visual remaining character indicator)
  - Description / Descripción (required, plain text multiline textarea, 10 to 4000 characters, with visual remaining character indicator)
- **FR-006**: The system MUST validate all mandatory fields and format constraints on both client-side and server-side before persisting data.
- **FR-007**: The system MUST generate a unique, non-colliding radicado number using the format `YYYY-NNNNNNNN` (e.g., `2026-00000001`), where `YYYY` is the four-digit year of filing and `NNNNNNNN` is an 8-digit zero-padded monotonic sequence that restarts at `00000001` on January 1st of each calendar year.
- **FR-008**: The system MUST guarantee atomic uniqueness of the radicado number under high concurrency, preventing any duplicate radicado numbers.
- **FR-009**: The system MUST calculate the maximum legal response due date in official Colombian business days, starting the count on the first business day immediately following the calendar filing date (UTC-5):
  - 15 business days for Petición, Queja, Reclamo, Sugerencia, and Felicitación.
  - 30 business days exclusively for Denuncia.
- **FR-010**: The due date calculation MUST exclude Saturdays, Sundays, and official Colombian national holidays (both fixed religious/civic dates and movable holidays regulated by Law 51 of 1983 - Ley Emiliani).
- **FR-011**: The system MUST persist the ticket with an initial operational status of "Registrado" (Registered) and record the creation timestamp in UTC.
- **FR-012**: The system MUST render an immediate confirmation screen displaying the generated radicado number, request type, submission date, calculated maximum response date, and an action to copy the radicado number to clipboard. Transactional email notifications are explicitly deferred to a subsequent dedicated notification feature.
- **FR-013**: The system MUST sanitize all incoming string inputs to prevent Cross-Site Scripting (XSS) and injection attacks while preserving Spanish characters, tildes, and punctuation.
- **FR-014**: All user-facing texts (form labels, instructions, placeholders, validation feedback, error messages, and confirmation screens) MUST be displayed strictly in Spanish.

---

### Key Entities *(include if feature involves data)*

- **PqrsdfTicket**:
  - `Id`: Unique internal identifier (UUID).
  - `RadicadoNumber`: Value Object representing the official tracking number (`YYYY-NNNNNNNN`).
  - `Type`: Value Object representing the PQRSDF category (Petition, Complaint, Claim, Suggestion, Denunciation, Compliment).
  - `DestinationAreaId`: Reference to the organizational unit responsible for processing the request.
  - `IsAnonymous`: Boolean indicating whether the ticket was filed anonymously (permitted only for Denuncia and Sugerencia).
  - `Applicant`: Value Object containing citizen personal and contact information (null when `IsAnonymous` is true).
  - `Subject`: Value Object encapsulating the concise summary of the request.
  - `Description`: Value Object encapsulating the detailed narrative of the request.
  - `CreatedAtUtc`: Timestamp of filing in UTC.
  - `DueDate`: Value Object representing the calculated legal deadline date in Colombian business days.
  - `Status`: Current lifecycle state, initialized to `Registered`.

- **Applicant (Value Object)**:
  - `FullName`: Full legal name of the citizen applicant.
  - `IdentificationType`: Type of identification document (`CC`, `CE`, `TI`, `PA`, `NIT`).
  - `IdentificationNumber`: Document number.
  - `Email`: Contact email address for receiving legal notifications and resolution.
  - `PhoneNumber`: Optional contact telephone number.

- **DestinationArea**:
  - `Id`: Unique internal identifier (UUID).
  - `Name`: Official department or administrative area name (e.g., "Atención al Ciudadano", "Jurídica", "Financiera").
  - `Code`: Short identifier code (e.g., "ATC", "JUR", "FIN").
  - `IsActive`: Boolean indicating whether the area is currently eligible to receive new filings.

- **ColombianCalendar / Holiday**:
  - Domain service or calendar specification defining statutory non-working holidays in Colombia for any given year, incorporating fixed dates and Monday-shifted holidays per Law 51 of 1983.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Citizens can complete and submit a PQRSDF ticket in under 3 minutes without requiring login or authentication.
- **SC-002**: 100% of generated radicado numbers strictly adhere to the `YYYY-NNNNNNNN` format with 0 collisions or duplications, even under 50 simultaneous concurrent requests.
- **SC-003**: 100% of calculated due dates accurately exclude Saturdays, Sundays, and statutory Colombian holidays, resulting in exactly 15 business days for standard types and 30 business days for Denuncias.
- **SC-004**: The confirmation screen with the radicado number and due date is displayed to the citizen in less than 2 seconds after submitting a valid form under normal network conditions.
- **SC-005**: 100% of validation errors and form feedback messages are presented in clear, user-friendly Spanish.
- **SC-006**: Form validation prevents 100% of incomplete or malformed submissions from being persisted.

---

## Assumptions

- **Target Audience & Connectivity**: Citizens access the public web form using modern web browsers (Chrome, Firefox, Edge, Safari) on desktop or mobile devices with internet connectivity.
- **File Attachments Scope**: File attachments and document uploads are deferred to a separate dedicated feature and are explicitly out of scope for this initial ticket filing MVP slice.
- **Email Notifications Scope**: Automated email notifications upon ticket creation are deferred to a dedicated notifications feature; the radicado number and due date are provided immediately on-screen with clipboard copy functionality.
- **Applicant Data Policy**: Applicant contact fields (Full Name, Document Type, Document Number, and Email) are required for Petición, Queja, Reclamo, and Felicitación to ensure delivery of notifications and tracking. Anonymous filing is permitted exclusively for Denuncia and Sugerencia.
- **Destination Areas Catalog**: The backend provides a seed of active destination areas (e.g., "Atención al Ciudadano", "Dirección General", "Oficina Jurídica", "Gestión Financiera", "Control Interno") for the initial deployment.
- **Colombian Holiday Calendar**: Colombian holiday rules follow Law 51 of 1983 (Ley Emiliani), which moves certain religious and civic holidays to the following Monday, and fixed national holidays (New Year's Day, Labor Day, Independence Day, Battle of Boyacá, Christmas, etc.).
- **Time Zone**: While timestamps are persisted in UTC internally, business day calculation and citizen display adhere to Colombia standard time (UTC-5 / America/Bogota).
