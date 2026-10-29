using CourseRegistration.Api.Configuration;
using CourseRegistration.Api.Repositories;
using CourseRegistration.Api.Services;
using Microsoft.Extensions.Options;
using Xunit;
using CourseRegistration.Api.Models;

namespace CourseRegistration.UnitTests;

public sealed class CourseRegistrationServiceTests
{
    private static CourseRegistrationService CreateService(int capacity = 2) =>
        new(new InMemoryCourseRegistrationRepository(Options.Create(new CourseOptions { Capacity = capacity })));

    [Fact]
    public async Task Enroll_WithAvailableCapacity_EnrollsLearner()
    {
        var service = CreateService();
        var result = await service.EnrollAsync("learner-1", true);
        Assert.Equal(RegistrationStatus.Enrolled, result.Status);
        Assert.Contains("learner-1", service.GetSnapshot().EnrolledLearnerIds);
        Assert.Equal(1, service.GetSnapshot().AvailableSeats);
    }

    [Fact]
    public async Task Enroll_WhenFull_AddsEligibleLearnerToWaitlist()
    {
        var service = CreateService(1);
        await service.EnrollAsync("learner-1", true);
        var result = await service.EnrollAsync("learner-2", true);
        Assert.Equal(RegistrationStatus.Waitlisted, result.Status);
        Assert.Equal(new[] { "learner-2" }, service.GetSnapshot().WaitlistedLearnerIds);
    }

    [Fact]
    public async Task Enroll_DuplicateRequest_DoesNotDuplicateEnrollmentOrWaitlist()
    {
        var service = CreateService(1);
        await service.EnrollAsync("learner-1", true);
        await service.EnrollAsync("learner-2", true);
        var duplicate = await service.EnrollAsync("LEARNER-2", true);
        Assert.Equal(RegistrationStatus.AlreadyRegistered, duplicate.Status);
        Assert.Single(service.GetSnapshot().WaitlistedLearnerIds);
    }

    [Fact]
    public async Task Drop_EnrolledLearner_PromotesEarliestWaitlistedLearner()
    {
        var service = CreateService(1);
        await service.EnrollAsync("learner-1", true);
        await service.EnrollAsync("learner-2", true);
        await service.EnrollAsync("learner-3", true);
        var result = await service.DropAsync("learner-1");
        Assert.Equal(RegistrationStatus.Promoted, result.Status);
        Assert.Equal("learner-2", result.PromotedLearnerId);
        Assert.Equal(new[] { "learner-2" }, service.GetSnapshot().EnrolledLearnerIds);
        Assert.Equal(new[] { "learner-3" }, service.GetSnapshot().WaitlistedLearnerIds);
    }

    [Fact]
    public async Task Enroll_WithoutPrerequisite_IsRejectedAndNotQueued()
    {
        var service = CreateService(1);
        var result = await service.EnrollAsync("learner-1", false);
        Assert.Equal(RegistrationStatus.PrerequisiteNotMet, result.Status);
        Assert.Empty(service.GetSnapshot().EnrolledLearnerIds);
        Assert.Empty(service.GetSnapshot().WaitlistedLearnerIds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Enroll_InvalidLearnerId_ReturnsInvalidRequest(string? learnerId)
    {
        var result = await CreateService().EnrollAsync(learnerId, true);
        Assert.Equal(RegistrationStatus.InvalidRequest, result.Status);
    }

    [Fact]
    public async Task Drop_LearnerNotEnrolled_ReturnsNotEnrolledWithoutChangingState()
    {
        var service = CreateService(1);
        await service.EnrollAsync("learner-1", true);
        var before = service.GetSnapshot();
        var result = await service.DropAsync("learner-2");
        Assert.Equal(RegistrationStatus.NotEnrolled, result.Status);
        var after = service.GetSnapshot();
        Assert.Equal(before.Capacity, after.Capacity);
        Assert.Equal(before.AvailableSeats, after.AvailableSeats);
        Assert.Equal(before.EnrolledLearnerIds, after.EnrolledLearnerIds);
        Assert.Equal(before.WaitlistedLearnerIds, after.WaitlistedLearnerIds);
    }

    [Fact]
    public async Task Drop_RepeatedDrop_ReturnsNotEnrolled()
    {
        var service = CreateService(1);
        await service.EnrollAsync("learner-1", true);
        Assert.Equal(RegistrationStatus.Enrolled, (await service.DropAsync("learner-1")).Status);
        Assert.Equal(RegistrationStatus.NotEnrolled, (await service.DropAsync("learner-1")).Status);
    }

    [Fact]
    public async Task Enroll_ConcurrentRequests_NeverExceedsCapacity()
    {
        var service = CreateService(5);
        var tasks = Enumerable.Range(1, 100).Select(i => service.EnrollAsync($"learner-{i}", true));
        await Task.WhenAll(tasks);
        Assert.Equal(5, service.GetSnapshot().EnrolledLearnerIds.Count);
        Assert.Equal(95, service.GetSnapshot().WaitlistedLearnerIds.Count);
        Assert.Equal(0, service.GetSnapshot().AvailableSeats);
    }

    [Fact]
    public void Repository_InvalidCapacity_ThrowsConfigurationError()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new InMemoryCourseRegistrationRepository(Options.Create(new CourseOptions { Capacity = 0 })));
    }
}
