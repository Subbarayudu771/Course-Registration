using CourseRegistration.Api.DTOs;
using CourseRegistration.Api.Interfaces;
using CourseRegistration.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseRegistration.Api.Controllers;

[ApiController]
[Route("api/registrations")]
public sealed class RegistrationsController : ControllerBase
{
    private readonly ICourseRegistrationService _service;
    public RegistrationsController(ICourseRegistrationService service) => _service = service;

    /// <summary>Enroll an eligible learner or add them to the FIFO waitlist when full.</summary>
    [HttpPost("enroll")]
    [ProducesResponseType(typeof(RegistrationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RegistrationResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.EnrollAsync(request.LearnerId, request.HasPrerequisite, cancellationToken);
        return result.Status switch
        {
            RegistrationStatus.InvalidRequest or RegistrationStatus.PrerequisiteNotMet => BadRequest(result),
            _ => Ok(result)
        };
    }

    /// <summary>Drop an enrollment and promote the earliest eligible waitlisted learner.</summary>
    [HttpPost("drop")]
    [ProducesResponseType(typeof(RegistrationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RegistrationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RegistrationResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Drop([FromBody] DropRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.DropAsync(request.LearnerId, cancellationToken);
        return result.Status switch
        {
            RegistrationStatus.InvalidRequest => BadRequest(result),
            RegistrationStatus.NotEnrolled => NotFound(result),
            _ => Ok(result)
        };
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(RegistrationSnapshot), StatusCodes.Status200OK)]
    public ActionResult<RegistrationSnapshot> GetStatus() => Ok(_service.GetSnapshot());
}
