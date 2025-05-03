using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Presentation.Models.Requests;

public record class UpdateTaskStatusRequest(TaskStatus NewStatus);
