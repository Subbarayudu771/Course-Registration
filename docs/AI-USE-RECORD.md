# AI prompt and use record

> Assessment requirement: replace or supplement this record with your own exact prompts, review notes, and evidence before submission. This file is a truthful starter record, not a claim that automated test execution occurred in this environment.

## Prompt used
“Implement the supplied course registration use case in .NET 8: enroll, drop, FIFO waitlist promotion, prerequisite checks, duplicate protection, negative cases, focused unit tests, API documentation, and setup instructions. Keep the scope bounded and explain assumptions and tradeoffs.”

## AI contribution summary
- Proposed a controller/service/repository separation and DTOs.
- Proposed a process-local serialization gate to protect the enrollment/drop workflows.
- Drafted focused xUnit tests for capacity, duplicate requests, FIFO promotion, prerequisite failure, invalid IDs, repeated drops, and concurrent enrollment requests.
- Drafted design/setup documentation.

## Review status
- **Accepted as a starting implementation:** bounded service slice, interfaces, DTO separation, explicit prerequisite/waitlist assumptions, and focused tests.
- **Requires independent verification:** compile/test with the .NET 8 SDK, inspect generated Swagger contract, and review all tests locally. This runtime did not have the `dotnet` CLI available, so no build/test execution is claimed here.
- **Potential follow-up:** add API integration tests and validate HTTP response mapping once the project is run locally.

## Evidence to add before submission
1. Paste the exact prompts and relevant AI response excerpts you actually used.
2. Record each material suggestion as accepted, edited, or rejected with a short reason.
3. Run `dotnet test` and paste the actual output (including test count).
4. Record any code edits made after reviewing AI output.
