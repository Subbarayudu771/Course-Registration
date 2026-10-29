# Course Registration Waitlist

A bounded .NET 8 service for one course offering. It supports enrollment, dropping, FIFO waitlist promotion, prerequisite validation, and an in-memory repository.

## Prerequisite and waitlist policy

- A learner must satisfy the prerequisite to enroll.
- An ineligible learner is rejected and is not added to the waitlist.
- If an ineligible stale entry is found in the queue, it is skipped during promotion.
- Eligible waitlisted learners are promoted in FIFO order.
- Learner IDs are trimmed and compared case-insensitively.
- A learner may have one active enrollment or waitlist entry. After dropping, the learner may register again.

## Requirements

- .NET 8 SDK

## Build and test

From the `api` directory:

```powershell
dotnet restore
dotnet build
dotnet test
```

The test suite covers available capacity, full capacity, duplicate requests, FIFO promotion, prerequisite failure, invalid and repeated drops, invalid configuration, and concurrent enrollment requests.

## Run the API

```powershell
dotnet run --project src\CourseRegistration.Api\CourseRegistration.Api.csproj
```

In Development, Swagger is available at `/swagger` using the URL printed by the application.

## API endpoints

- `POST /api/registrations/enroll`
- `POST /api/registrations/drop`
- `GET /api/registrations/status`

The default capacity is configured in `src\CourseRegistration.Api\appsettings.json` and can be overridden with the `Course:Capacity` configuration value.

## Assessment documents

- [Design specification](../docs/DESIGN.md)
- [AI use record](../docs/AI-USE-RECORD.md)
