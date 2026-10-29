using CourseRegistration.Api.Models;

namespace CourseRegistration.Api.Interfaces;

public interface ICourseRegistrationRepository
{
    bool IsRegistered(string learnerId);
    bool IsEnrolled(string learnerId);
    int EnrolledCount { get; }
    int Capacity { get; }
    void AddEnrollment(string learnerId, DateTimeOffset createdAt);
    bool RemoveEnrollment(string learnerId);
    void AddToWaitlist(string learnerId, DateTimeOffset createdAt, bool hasPrerequisite);
    WaitlistEntry? RemoveNextWaitlisted();
    bool IsWaitlisted(string learnerId);
    RegistrationSnapshot GetSnapshot();
}
