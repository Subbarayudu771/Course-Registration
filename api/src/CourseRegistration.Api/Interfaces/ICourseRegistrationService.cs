using CourseRegistration.Api.Models;

namespace CourseRegistration.Api.Interfaces;

public interface ICourseRegistrationService
{
    Task<RegistrationResult> EnrollAsync(string? learnerId, bool hasPrerequisite, CancellationToken cancellationToken = default);
    Task<RegistrationResult> DropAsync(string? learnerId, CancellationToken cancellationToken = default);
    RegistrationSnapshot GetSnapshot();
}
