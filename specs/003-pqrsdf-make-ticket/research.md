# Research: Public PQRSDF Ticket Registration (003-pqrsdf-make-ticket)

## Technical Decisions and Rationale

### 1. Colombian Statutory Holiday & Business Days Computation (Ley Emiliani / Ley 51 de 1983)

- **Decision**: Implement an algorithmic, self-contained domain service (`ColombianHolidayService`) that dynamically computes all official Colombian statutory holidays for any given calendar year, combined with an iterative business day calculator (`DueDateCalculator`).
- **Rationale**:
  - Colombia has 18 statutory holidays per year governed by Law 51 of 1983 (Ley Emiliani):
    1. Fixed holidays: January 1 (New Year), May 1 (Labor Day), July 20 (Independence), August 7 (Battle of Boyacá), December 8 (Immaculate Conception), December 25 (Christmas).
    2. Easter-dependent movable holidays: Holy Thursday, Good Friday, Ascension of the Lord (Easter + 43 days -> shifted to next Monday), Corpus Christi (Easter + 64 days -> next Monday), Sacred Heart of Jesus (Easter + 71 days -> next Monday).
    3. Emiliani calendar-shifted fixed holidays: Epiphany (Jan 6), Saint Joseph (Mar 19), Saint Peter & Saint Paul (Jun 29), Assumption of Mary (Aug 15), Discovery of America / Race Day (Oct 12), All Saints (Nov 1), Independence of Cartagena (Nov 11) — each shifted to the following Monday if they do not fall on a Monday.
  - Computing these mathematically using Butcher's Algorithm (Meeus/Jones/Butcher algorithm for Gregorian Easter) and Emiliani Monday-shift rules avoids external web API dependencies, eliminates network latency, guarantees 100% offline availability, and ensures determinism across test suites.
- **Alternatives Considered**:
  - *External Public API (e.g. Nager.Date or Colombian Gov API)*: Rejected because external network dependencies introduce latency, failure points, rate limits, and potential downtime for a core business requirement.
  - *Static Database Table with pre-seeded dates for 2026-2030*: Evaluated, but requires ongoing maintenance/seeding over the system lifespan. The algorithmic service can compute any year past, present, or future with zero database roundtrips.

---

### 2. High-Concurrency Radicado Sequence Generation (`YYYY-NNNNNNNN`) with Annual Reset

- **Decision**: Implement an atomic sequence generator backed by SQL Server using an entity/table `RadicadoSequence` with optimistic/pessimistic row locking (`UPDLOCK, ROWLOCK`) or native SQL Server sequence per year, encapsulated behind an `IRadicadoSequenceGenerator` interface in `Domain`.
- **Rationale**:
  - Format is `YYYY-NNNNNNNN` where `NNNNNNNN` is an 8-digit zero-padded number (e.g., `2026-00000001`).
  - Clarification Q2 established that each calendar year restarts the counter at `00000001`.
  - Under peak concurrency (e.g., 50+ simultaneous requests), querying `MAX(RadicadoNumber)` in EF Core leads to race conditions, transaction rollbacks, or duplicate key violations.
  - An atomic sequence table `RadicadoSequences (Year INT PRIMARY KEY, CurrentValue BIGINT NOT NULL)` updated via an atomic SQL statement (`UPDATE RadicadoSequences SET CurrentValue = CurrentValue + 1 OUTPUT INSERTED.CurrentValue WHERE Year = @Year`) guarantees zero duplicates, zero gaps under committed transactions, and instant sub-millisecond increment without deadlocks.
- **Alternatives Considered**:
  - *`MAX(RadicadoNumber) + 1`*: Rejected due to high risk of concurrency collisions and table scans on high ticket volumes.
  - *GUID-based identifier*: Rejected because Colombian legal standards require an official sequential tracking number (`AAAA-NNNNNNNN`).
  - *SQL Server native `SEQUENCE` object*: While efficient, creating a new `CREATE SEQUENCE` dynamically each year requires DDL permissions at runtime. The single atomic counter table approach handles year changes automatically using standard DML (`INSERT ... ON CONFLICT / MERGE`).

---

### 3. Frontend Form Architecture, Validation & Character Counters (App Router Screaming Architecture)

- **Decision**: Build the filing form using React state / React Hook Form with Zod schema validation co-located under `src/app/pqrsdf/`, integrating client-side validation, live character countdown indicators, and the typed API client `@/shared/api/client`.
- **Rationale**:
  - Constitution v1.3.0 Principle IV strictly mandates Screaming Architecture inside `src/app/pqrsdf/`.
  - Form requires dynamic behavior:
    - Loading destination areas from backend on mount.
    - Conditionally showing/hiding applicant fields when the citizen toggles "Radicar de forma anónima" (permitted strictly for `Denuncia` and `Sugerencia` per Clarification Q1).
    - Character countdown for Subject (`5` to `150` characters) and Description (`10` to `4000` characters) per Clarification Q4.
    - Immediate disabling of submit button with loading spinner on first click to prevent duplicate submissions.
  - Zod allows sharing validation rule semantics and Spanish error messages between the form UI and schema contracts.
- **Alternatives Considered**:
  - *Uncontrolled native HTML form*: Simpler, but lacks dynamic conditional field rendering, live remaining character counting, and accessible field error messaging.
  - *Generic `/components/forms` folder*: Strictly forbidden by Constitution Principle IV (no generic technical folders at root).

---

### 4. Result Pattern and Global Error Handling Integration

- **Decision**: Adhere strictly to Constitution Principle V using `Result<MakePqrsdfResponse>` in Application and `ApiControllerBase.HandleResult()` in API.
- **Rationale**:
  - Invalid inputs (e.g. invalid document number, malformed email, text too short) return typed domain/application validation errors (`Error.Validation(...)`), resulting in HTTP 400 with ProblemDetails.
  - Business rule violations (e.g. non-active destination area selected, anonymous attempted on a Petition) return `Error.Failure(...)`, resulting in HTTP 422 with Spanish explanation.
  - All unexpected infrastructure errors (database down, unexpected pánicos) are intercepted by `GlobalExceptionHandler` and rendered as HTTP 500 RFC 7807 ProblemDetails without leaking internals.
