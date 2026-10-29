using CourseRegistration.Api.Interfaces;
using CourseRegistration.Api.Models;

namespace CourseRegistration.Api.Services;

/// <summary>Coordinates enrollment and promotion as one critical section within this process.</summary>
public sealed class CourseRegistrationService : ICourseRegistrationService
{
    private readonly ICourseRegistrationRepository _repository;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public CourseRegistrationService(ICourseRegistrationRepository repository) => _repository = repository;

    public async Task<RegistrationResult> EnrollAsync(string? learnerId, bool hasPrerequisite, CancellationToken cancellationToken = default)
    {
        var normalizedId = learnerId?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedId))
            return new(RegistrationStatus.InvalidRequest, "LearnerId is required.");
        if (!hasPrerequisite)
            return new(RegistrationStatus.PrerequisiteNotMet, "Learner does not meet the course prerequisite.", normalizedId);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_repository.IsRegistered(normalizedId))
                return new(RegistrationStatus.AlreadyRegistered, "Learner is already enrolled or waitlisted.", normalizedId);

            if (_repository.EnrolledCount < _repository.Capacity)
            {
                _repository.AddEnrollment(normalizedId, DateTimeOffset.UtcNow);
                return new(RegistrationStatus.Enrolled, "Enrollment successful.", normalizedId);
            }

            _repository.AddToWaitlist(normalizedId, DateTimeOffset.UtcNow, hasPrerequisite);
            return new(RegistrationStatus.Waitlisted, "Course is full; learner added to the waitlist.", normalizedId);
        }
        finally { _gate.Release(); }
    }

    public async Task<RegistrationResult> DropAsync(string? learnerId, CancellationToken cancellationToken = default)
    {
        var normalizedId = learnerId?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedId))
            return new(RegistrationStatus.InvalidRequest, "LearnerId is required.");

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!_repository.RemoveEnrollment(normalizedId))
                return new(RegistrationStatus.NotEnrolled, "Learner is not currently enrolled.", normalizedId);

            // Policy: learners failing prerequisites are rejected before entering the queue. Promotion
            // rechecks the stored prerequisite flag and skips any ineligible legacy/stale queue entries.
            while (true)
            {
                var next = _repository.RemoveNextWaitlisted();
                if (next is null)
                    return new(RegistrationStatus.Enrolled, "Enrollment dropped; seat is now available.", normalizedId);
                if (!next.HasPrerequisite) continue;
                _repository.AddEnrollment(next.LearnerId, DateTimeOffset.UtcNow);
                return new(RegistrationStatus.Promoted, "Enrollment dropped and the earliest eligible waitlisted learner was promoted.", normalizedId, next.LearnerId);
            }
        }
        finally { _gate.Release(); }
    }

    public RegistrationSnapshot GetSnapshot() => _repository.GetSnapshot();
}
