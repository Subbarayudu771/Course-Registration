using System.ComponentModel.DataAnnotations;

namespace CourseRegistration.Api.DTOs;

public sealed class EnrollRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string LearnerId { get; init; } = string.Empty;
    public bool HasPrerequisite { get; init; }
}
