using CourseRegistration.Api.Interfaces;
using CourseRegistration.Api.Models;

namespace CourseRegistration.Api.Services;

/// <summary>Coordinates enrollment and promotion as one locked critical section within this process.</summary>
public sealed class CourseRegistrationService : ICourseRegistrationService
{
    private readonly ICourseRegistrationRepository _repository;
    private readonly object _sync = new();

    public CourseRegistrationService(ICourseRegistrationRepository repository) => _repository = repository;

    public Task<RegistrationResult> EnrollAsync(string? learnerId, bool hasPrerequisite, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedId = learnerId?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedId))
            return Task.FromResult(new RegistrationResult(RegistrationStatus.InvalidRequest, "LearnerId is required."));
        if (!hasPrerequisite)
            return Task.FromResult(new RegistrationResult(RegistrationStatus.PrerequisiteNotMet, "Learner does not meet the course prerequisite.", normalizedId));

        lock (_sync)
        {
            if (_repository.IsRegistered(normalizedId))
                return Task.FromResult(new RegistrationResult(RegistrationStatus.AlreadyRegistered, "Learner is already enrolled or waitlisted.", normalizedId));

            if (_repository.EnrolledCount < _repository.Capacity)
            {
                _repository.AddEnrollment(normalizedId, DateTimeOffset.UtcNow);
                return Task.FromResult(new RegistrationResult(RegistrationStatus.Enrolled, "Enrollment successful.", normalizedId));
            }

            _repository.AddToWaitlist(normalizedId, DateTimeOffset.UtcNow, hasPrerequisite);
            return Task.FromResult(new RegistrationResult(RegistrationStatus.Waitlisted, "Course is full; learner added to the waitlist.", normalizedId));
        }
    }

    public Task<RegistrationResult> DropAsync(string? learnerId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedId = learnerId?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedId))
            return Task.FromResult(new RegistrationResult(RegistrationStatus.InvalidRequest, "LearnerId is required."));

        lock (_sync)
        {
            if (!_repository.RemoveEnrollment(normalizedId))
                return Task.FromResult(new RegistrationResult(RegistrationStatus.NotEnrolled, "Learner is not currently enrolled.", normalizedId));

            // Policy: learners failing prerequisites are rejected before entering the queue. Promotion
            // rechecks the stored prerequisite flag and skips any ineligible legacy/stale queue entries.
            while (true)
            {
                var next = _repository.RemoveNextWaitlisted();
                if (next is null)
                    return Task.FromResult(new RegistrationResult(RegistrationStatus.Enrolled, "Enrollment dropped; seat is now available.", normalizedId));
                if (!next.HasPrerequisite) continue;
                _repository.AddEnrollment(next.LearnerId, DateTimeOffset.UtcNow);
                return Task.FromResult(new RegistrationResult(RegistrationStatus.Promoted, "Enrollment dropped and the earliest eligible waitlisted learner was promoted.", normalizedId, next.LearnerId));
            }
        }
    }

    public RegistrationSnapshot GetSnapshot() => _repository.GetSnapshot();
}
