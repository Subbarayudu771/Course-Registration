namespace CourseRegistration.Api.Configuration;

public sealed class CourseOptions
{
    public const string SectionName = "Course";
    public int Capacity { get; init; } = 3;
}
