# Design specification

## Scope and architecture
This .NET 8 Web API implements only one course offering's enroll, drop, and FIFO waitlist promotion. It intentionally has no frontend, catalog, identity provider, payment workflow, or external database.

- **Controller**: HTTP binding, status-code mapping, and API contract only.
- **Service**: use-case orchestration, input validation, duplicate checks, prerequisite policy, drop/promotion workflow.
- **Repository interface**: persistence boundary so an in-memory implementation can later be replaced without changing the use case.
- **In-memory repository**: stores enrolled learners and an ordered FIFO queue; case-insensitive learner IDs prevent casing-based duplicates.
- **DTOs**: request contracts are separate from domain/result models.

## Rules and assumptions
1. One offering exists; capacity is configured by `Course:Capacity` and must be positive.
2. `HasPrerequisite` is supplied by the caller for this assessment prototype. In a real system it must be derived from trusted learner/course records, not trusted from an arbitrary client.
3. A learner who does not meet the prerequisite is rejected and never added to the waitlist. Promotion also checks the stored prerequisite flag and skips ineligible legacy/stale queue entries. This makes the policy explicit and consistent.
4. Duplicate enroll requests for either enrolled or waitlisted learners do not mutate state.
5. Dropping a non-enrolled learner returns a clear `NotEnrolled` result and does not mutate state.
6. When a seat opens, the first eligible queue entry is promoted. If there is no eligible waitlisted learner, the seat remains available.
7. IDs are trimmed and compared case-insensitively.

## Invariants
- Enrolled count never exceeds capacity.
- A learner is not simultaneously enrolled and waitlisted.
- A learner can appear at most once across the enrolled and waitlisted sets.
- Waitlist order is FIFO; skipped ineligible entries are removed from the queue.
- Failed/invalid operations do not change registration state.

## Concurrency and persistence
A standard C# `lock` in the singleton service serializes enrollment/drop workflows, making check-then-write and promotion atomic within this single application process. The repository also protects its own data structures with a lock. This is adequate for the requested in-memory prototype, but not a distributed deployment: multiple API instances would need a transactional database operation, row/advisory lock or equivalent concurrency control. In-memory data resets on restart.

## API
- `POST /api/registrations/enroll` body: `{ "learnerId": "learner-1", "hasPrerequisite": true }`
- `POST /api/registrations/drop` body: `{ "learnerId": "learner-1" }`
- `GET /api/registrations/status` returns capacity, enrolled IDs, waitlisted IDs, and available seats.
- Swagger UI is enabled in Development at `/swagger`.

## Security and environments
No secrets are needed for this prototype. The `environments/` Terraform layout is intentionally omitted because the assessment explicitly excludes deployment and a new database. If this service later integrates with cloud resources, keep environment-specific Terraform and retrieve secrets from the platform's secret manager rather than committing credentials to `appsettings*.json`. The current appsettings only holds a non-secret capacity setting.

## Tradeoffs and follow-up risks
- In-memory storage is volatile and only single-process safe.
- `HasPrerequisite` is demonstration input, not production-grade authorization/eligibility evidence.
- For production, add persistent transactions, authentication/authorization, structured audit logging, rate limiting, health checks, and integration/API tests.
- API uses simple result statuses to keep the bounded feature understandable; a larger system might use a typed error/result abstraction.
