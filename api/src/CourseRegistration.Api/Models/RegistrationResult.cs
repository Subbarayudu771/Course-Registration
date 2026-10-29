namespace CourseRegistration.Api.Models;

public enum RegistrationStatus
{
    Enrolled,
    Waitlisted,
    Promoted,
    AlreadyRegistered,
    PrerequisiteNotMet,
    NotEnrolled,
    InvalidRequest
}

public sealed record RegistrationResult(
    RegistrationStatus Status,
    string Message,
    string? LearnerId = null,
    string? PromotedLearnerId = null);
