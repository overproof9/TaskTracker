using FluentValidation;
using TaskTracker.Presentation.Models.Requests;

namespace TaskTracker.Presentation.Validation;

public class AssignTaskRequestValidator : AbstractValidator<AssignTaskRequest>
{
    public AssignTaskRequestValidator()
    {
        RuleFor(x => x.NewUserId)
            .NotEqual(Guid.Empty);
    }
}
