using FluentValidation;
using TaskTracker.Presentation.Models.Requests;

namespace TaskTracker.Presentation.Validation;

public class UpdateTaskStatusRequestValidator : AbstractValidator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}
