# Course Registration Waitlist API (.NET 8)

A focused AI-assisted engineering assessment implementation for one course offering. It supports enrollment, dropping, prerequisite validation, duplicate protection, and FIFO waitlist promotion.

## Requirements
- .NET 8 SDK
- Internet access on first restore to download NuGet packages
- Visual Studio 2022 (17.8 or later) or another .NET-compatible editor (optional)

## Open in Visual Studio
1. Extract the ZIP.
2. Open `api/CourseRegistrationAssessment.sln`.
3. Set `CourseRegistration.Api` as the startup project if Visual Studio has not selected it automatically.
4. Build the solution and run the API project. The launch profile starts the app in Development and opens Swagger at `http://localhost:5080/swagger`.

## Run from the repository root
Restore packages:
```bash
dotnet restore api/CourseRegistrationAssessment.sln
```

Run unit tests:
```bash
dotnet test api/CourseRegistrationAssessment.sln
```

Run the API:
```bash
dotnet run --project api/src/CourseRegistration.Api/CourseRegistration.Api.csproj
```

Swagger UI is available at `http://localhost:5080/swagger` when using the included launch profile. If a port is already in use, change `applicationUrl` in `api/src/CourseRegistration.Api/Properties/launchSettings.json`.

## Configuration
The default course capacity is `3` in `api/src/CourseRegistration.Api/appsettings.json`. Change `Course:Capacity` to test different capacities. Only non-sensitive configuration is stored in appsettings; no Terraform `environments/` folder is included because deployment is outside the assessment scope.

## API endpoints
### Enroll or join waitlist
`POST /api/registrations/enroll`
```json
{
  "learnerId": "learner-1",
  "hasPrerequisite": true
}
```
### Drop enrollment
`POST /api/registrations/drop`
```json
{
  "learnerId": "learner-1"
}
```
### View registration status
`GET /api/registrations/status`

## Project layout
```text
CourseRegistrationAssessment/
  api/
    CourseRegistrationAssessment.sln
    src/CourseRegistration.Api/
      Configuration/   Strongly typed settings
      Controllers/     HTTP endpoints
      DTOs/            Request contracts
      Interfaces/      Service and repository contracts
      Models/          Registration models and results
      Repositories/    In-memory repository
      Services/        Enrollment/drop business workflows
      Properties/      Launch profile for local Swagger startup
      Program.cs       Dependency injection and middleware
      appsettings*.json
    tests/CourseRegistration.UnitTests/
  docs/
    DESIGN.md
    AI-USE-RECORD.md
```

## Assumptions and limitations
- One course offering is modeled.
- Learner IDs are trimmed and compared case-insensitively.
- Learners who fail the prerequisite check are rejected and not queued; promotion skips any ineligible legacy queue entries.
- The in-memory store resets when the application restarts and concurrency protection applies to one process only.
- `HasPrerequisite` is a demonstration input; a production system should determine eligibility from trusted records.

See `docs/DESIGN.md` and `docs/AI-USE-RECORD.md` for the design decisions and AI-use record. Update the AI-use record with the prompts actually used and verification you personally perform before submitting the assessment.
