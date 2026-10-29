# AI prompt and use record

This record summarizes the AI-assisted activities, human review, and verification performed for the final implementation.

## Prompts and activities

1. Review the current code against the supplied course-waitlist assessment, including acceptance criteria, scope, deliverables, and overengineering.
2. Replace `SemaphoreSlim` with a more familiar concurrency approach and complete the required assessment changes.

## AI contribution summary
- Proposed a controller/service/repository separation and DTOs.
- Proposed a process-local serialization gate to protect the enrollment/drop workflows; this was reviewed and changed from `SemaphoreSlim` to the standard C# `lock` approach.
- Drafted focused xUnit tests for capacity, duplicate requests, FIFO promotion, prerequisite failure, invalid IDs, repeated drops, and concurrent enrollment requests.
- Drafted design/setup documentation.

## Review status
- **Accepted as a starting implementation:** bounded service slice, interfaces, DTO separation, explicit prerequisite/waitlist assumptions, and focused tests.
- **Accepted and verified:** the final implementation uses `lock` around the synchronous multi-step workflows, while repository collection locks were retained.
- **Accepted and verified:** the design, setup, AI-use, and limitations documentation was added or updated.
- **Potential follow-up:** add API integration tests and validate HTTP response mapping in a larger production system.

## Evidence to add before submission
1. Paste the exact prompts and relevant AI response excerpts you actually used.
2. Record each material suggestion as accepted, edited, or rejected with a short reason.
3. Run `dotnet test` and paste the actual output (including test count).
4. Record any code edits made after reviewing AI output.
