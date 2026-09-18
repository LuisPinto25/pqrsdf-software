# Technical Research: Public PQRSDF Ticket Consultation

**Feature**: [Public PQRSDF Ticket Consultation](../spec.md)  
**Date**: 2026-09-18  
**Status**: Completed

---

## Technical Decisions & Findings

### Decision 1: CQRS Query Design & Application Structure

- **Context**: The consultation feature is purely a read operation (`Query`) in CQRS terms. Per Constitution Principle II, use cases must reside in `Application/Features/[FeatureName]/UseCases/[UseCaseName]/` as self-contained units with their query, handler, and exclusive DTOs.
- **Decision**: Create `Application/Features/Pqrsdf/UseCases/GetTicketByRadicado/` containing:
  - `GetTicketByRadicadoQuery.cs`: `record GetTicketByRadicadoQuery(string Radicado) : IRequest<Result<PublicTicketStatusDto>>`
  - `GetTicketByRadicadoQueryHandler.cs`: Executes radicado validation, queries repository, builds timeline, calculates remaining/overdue business days, and maps to `PublicTicketStatusDto`.
  - `PublicTicketStatusDto.cs`: Exclusive read model DTO exposing operational fields and zero PII.
  - `TicketTimelineMilestoneDto.cs`: Milestone data representation for timeline stages.
  - `TicketResolutionDto.cs`: Response content and closure date representation.
- **Rationale**: Strict adherence to CQRS and Clean Architecture prevents bloated controllers and encapsulates business mapping and SLA calculation in the application layer.
- **Alternatives Considered**:
  - Direct DbContext access in Controller: Rejected (violates DIP and Constitution Principle II).
  - Reusing internal domain entities in the API: Rejected (would risk exposing applicant PII and violates DDD).

---

### Decision 2: Ticket Response Storage & Domain Model Evolution

- **Context**: `PqrsdfTicket` aggregate root currently holds `RadicadoNumber`, `Type`, `DestinationAreaId`, `Subject`, `Description`, `DueDate`, `Status`, `CreatedAtUtc`, and `UpdatedAtUtc`. When closed or answered, it must preserve the official response text and the date it was answered.
- **Decision**: Evolve `PqrsdfTicket` aggregate root to include:
  - `string? ResponseText { get; private set; }`
  - `DateTime? ResponseDateUtc { get; private set; }`
  - Domain method `CloseWithResponse(string responseText, DateTime responseDateUtc)` which validates response text (10-4000 chars), transitions `Status` to `TicketStatus.Closed`, and sets `ResponseDateUtc`.
  - Update `PqrsdfTicketConfiguration.cs` in Infrastructure to map `ResponseText` (nullable `nvarchar(4000)`) and `ResponseDateUtc` (nullable `datetime2`).
- **Rationale**: Keeps the aggregate root non-anemic per Constitution Principle III, protects invariants, and prepares the persistence model for the official closure of tickets.
- **Alternatives Considered**:
  - Creating a separate `TicketResponse` table: Over-engineering for this MVP slice since each ticket has exactly one final response.

---

### Decision 3: Remaining & Overdue Business Days Calculation

- **Context**: Colombian administrative law requires tracking deadlines in official Colombian business days (excluding weekends and statutory holidays per Law 51 of 1983 - Ley Emiliani). The system must report remaining business days if within deadline, or business days elapsed in mora if overdue.
- **Decision**: Extend `DueDateCalculator` in `Domain/Services/DueDateCalculator.cs` with helper methods:
  - `CalculateRemainingBusinessDays(DateOnly today, DateOnly dueDate)`:
    - If `today < dueDate`: counts business days starting from `today.AddDays(1)` up to and including `dueDate`.
    - If `today == dueDate`: returns `0` remaining days.
    - If `today > dueDate`: returns `0` (overdue).
  - `CalculateOverdueBusinessDays(DateOnly today, DateOnly dueDate)`:
    - If `today > dueDate`: counts business days starting from `dueDate.AddDays(1)` up to and including `today`.
    - Otherwise returns `0`.
- **Rationale**: Reuses the validated `IColombianHolidayService` logic, ensuring 100% mathematical consistency between the due date determined at filing and the remaining days displayed in tracking.
- **Alternatives Considered**:
  - Simple calendar day subtraction (`(dueDate - today).TotalDays`): Rejected (violates Colombian administrative law where holidays and weekends are legally excluded).

---

### Decision 4: Rate Limiting on Public Tracking Endpoint

- **Context**: The public consultation endpoint requires no citizen authentication. To protect the API against automated scrapers, brute-force radicado enumeration, and DoS attacks, rate limiting of 30 req/min per IP was clarified in Q5.
- **Decision**: Use ASP.NET Core 10's native `Microsoft.AspNetCore.RateLimiting` middleware.
  - Register in `Program.cs` via `builder.Services.AddRateLimiter(...)` configuring an IP-partitioned `FixedWindowLimiter` (30 requests / 1 minute).
  - Apply `[EnableRateLimiting("PublicTrackingPolicy")]` to `[HttpGet("{radicado}")]` in `PqrsdfController`.
  - Configure `OnRejected` callback to return an RFC 7807 ProblemDetails / `Result<PublicTicketStatusDto>.Failure` payload with HTTP 429 and a friendly Spanish message: *"Ha superado el límite de consultas permitidas por minuto. Por favor espere un momento antes de intentar de nuevo."*
- **Rationale**: Standard, zero-dependency, highly performant, and fully compliant with FR-011 and SC-007.
- **Alternatives Considered**:
  - External Redis distributed rate limiter: Rejected (violates Constitution Principle II forbidding external brokers/caches for CQRS).
  - Client-side throttling only: Rejected (insufficient against direct HTTP API script attacks).

---

### Decision 5: Frontend Route & Screaming Architecture under `/pqrsdf/search`

- **Context**: Per user clarification in Q3, the public search view must reside at `/pqrsdf/search` and support deep-linking via query parameter `?radicado=YYYY-NNNNNNNN`.
- **Decision**: Co-locate the entire search feature in Next.js App Router:
  - Directory: `src/app/pqrsdf/search/`
  - `page.tsx`: Page component reading `searchParams` (`radicado`), orchestrating the search experience.
  - Co-located components under `src/app/pqrsdf/search/components/`:
    - `TicketSearchBox.tsx`: Form input with client-side regex formatting (`^\d{4}-\d{8}$`), auto-trim, and clear action.
    - `TicketStatusHeader.tsx`: Shows Radicado, Request Type, Destination Area, and prominent status/overdue badges.
    - `TicketTimeline.tsx`: Stepper component rendering milestones (*Registrado*, *Asignado*, *En trámite*, *Respondido*, *Cerrado*) with Colombian formatted dates.
    - `TicketDetailCard.tsx`: Renders the original Subject and Description in plain text.
    - `TicketResolutionCard.tsx`: Dedicated card rendering the official answer text and closure date when answered/closed.
  - Co-located hook: `src/app/pqrsdf/search/hooks/useTicketSearch.ts` managing query state, loading, error notifications, and URL synchronization (`router.replace`).
  - Shared API client: Add `getTicketByRadicado(radicado: string)` in `src/shared/api/client.ts`.
- **Rationale**: Adheres to Constitution Principle IV (Screaming Architecture under `app/pqrsdf/`), enables deep-linking from filing confirmation receipts, and provides responsive mobile/desktop layout.
