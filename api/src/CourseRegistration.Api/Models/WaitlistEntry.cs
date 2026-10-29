namespace CourseRegistration.Api.Models;

public sealed record WaitlistEntry(string LearnerId, DateTimeOffset CreatedAt, bool HasPrerequisite);
