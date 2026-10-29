namespace CourseRegistration.Api.Models;

public sealed record RegistrationSnapshot(
    int Capacity,
    IReadOnlyCollection<string> EnrolledLearnerIds,
    IReadOnlyCollection<string> WaitlistedLearnerIds,
    int AvailableSeats);
