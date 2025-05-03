using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Models;

public record class CreateTaskModel(
    string Title,
    string Description,
    TaskPriority Priority,
    DateTime Deadline,
    Guid UserId
);
