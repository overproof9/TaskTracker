using TaskTracker.Domain.Enums;

namespace TaskTracker.Presentation.Models.Requests;

public record class CreateTaskRequest(
    string Title,
    string Description,
    TaskPriority Priority,
    DateTime Deadline,
    Guid UserId
);
