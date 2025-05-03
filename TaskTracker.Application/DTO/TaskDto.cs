using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.DTO;

public record class TaskDto(
    Guid Id,
    string Title,
    string Description,
    Domain.Enums.TaskStatus Status,
    TaskPriority Priority,
    DateTime Deadline,
    DateTime CreatedAt,
    Guid UserId
);
