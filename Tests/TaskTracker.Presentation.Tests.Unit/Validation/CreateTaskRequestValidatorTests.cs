using FluentValidation.TestHelper;
using TaskTracker.Presentation.Models.Requests;
using TaskTracker.Presentation.Validation;
using TaskPriority = TaskTracker.Domain.Enums.TaskPriority;

namespace TaskTracker.Presentation.Tests.Unit.Validation;

public class CreateTaskRequestValidatorTests
{
    private readonly CreateTaskRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        var model = new CreateTaskRequest("", "desc", TaskPriority.Medium, DateTime.UtcNow.AddDays(1), Guid.NewGuid());
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Empty()
    {
        var model = new CreateTaskRequest("Title", "", TaskPriority.Medium, DateTime.UtcNow.AddDays(1), Guid.NewGuid());
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Deadline_Is_Past()
    {
        var model = new CreateTaskRequest("T", "D", TaskPriority.Medium, DateTime.UtcNow.AddMinutes(-1), Guid.NewGuid());
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Deadline);
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var model = new CreateTaskRequest("T", "D", TaskPriority.Medium, DateTime.UtcNow.AddMinutes(10), Guid.Empty);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = new CreateTaskRequest("Valid", "Valid description", TaskPriority.High, DateTime.UtcNow.AddHours(1), Guid.NewGuid());
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Title_Too_Long()
    {
        var title = new string('a', 101);
        var model = new CreateTaskRequest(title, "desc", TaskPriority.Low, DateTime.UtcNow.AddDays(1), Guid.NewGuid());
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Too_Long()
    {
        var desc = new string('b', 1001);
        var model = new CreateTaskRequest("Title", desc, TaskPriority.Low, DateTime.UtcNow.AddDays(1), Guid.NewGuid());
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
