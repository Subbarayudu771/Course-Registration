using CourseRegistration.Api.Configuration;
using CourseRegistration.Api.Interfaces;
using CourseRegistration.Api.Models;
using Microsoft.Extensions.Options;

namespace CourseRegistration.Api.Repositories;

/// <summary>Single-process in-memory store for the assessment's one-offering prototype.</summary>
public sealed class InMemoryCourseRegistrationRepository : ICourseRegistrationRepository
{
    private readonly object _sync = new();
    private readonly Dictionary<string, EnrollmentRecord> _enrollments = new(StringComparer.OrdinalIgnoreCase);
    private readonly Queue<WaitlistEntry> _waitlist = new();
    private readonly HashSet<string> _waitlistedIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly int _capacity;

    public InMemoryCourseRegistrationRepository(IOptions<CourseOptions> options)
    {
        _capacity = options.Value.Capacity > 0
            ? options.Value.Capacity
            : throw new InvalidOperationException("Course:Capacity must be greater than zero.");
    }

    public int Capacity => _capacity;
    public int EnrolledCount { get { lock (_sync) return _enrollments.Count; } }

    public bool IsRegistered(string learnerId)
    {
        lock (_sync) return _enrollments.ContainsKey(learnerId) || _waitlistedIds.Contains(learnerId);
    }

    public bool IsEnrolled(string learnerId) { lock (_sync) return _enrollments.ContainsKey(learnerId); }

    public void AddEnrollment(string learnerId, DateTimeOffset createdAt)
    {
        lock (_sync)
        {
            if (_enrollments.Count >= _capacity) throw new InvalidOperationException("Course capacity has been reached.");
            if (_enrollments.ContainsKey(learnerId)) throw new InvalidOperationException("Learner is already enrolled.");
            _enrollments.Add(learnerId, new EnrollmentRecord(learnerId, createdAt));
        }
    }

    public bool RemoveEnrollment(string learnerId) { lock (_sync) return _enrollments.Remove(learnerId); }

    public void AddToWaitlist(string learnerId, DateTimeOffset createdAt, bool hasPrerequisite)
    {
        lock (_sync)
        {
            if (_enrollments.ContainsKey(learnerId) || !_waitlistedIds.Add(learnerId))
                throw new InvalidOperationException("Learner is already registered.");
            _waitlist.Enqueue(new WaitlistEntry(learnerId, createdAt, hasPrerequisite));
        }
    }

    public WaitlistEntry? RemoveNextWaitlisted()
    {
        lock (_sync)
        {
            if (_waitlist.Count == 0) return null;
            var entry = _waitlist.Dequeue();
            _waitlistedIds.Remove(entry.LearnerId);
            return entry;
        }
    }

    public bool IsWaitlisted(string learnerId) { lock (_sync) return _waitlistedIds.Contains(learnerId); }

    public RegistrationSnapshot GetSnapshot()
    {
        lock (_sync)
            return new RegistrationSnapshot(_capacity, _enrollments.Keys.ToArray(), _waitlist.Select(x => x.LearnerId).ToArray(), _capacity - _enrollments.Count);
    }
}
