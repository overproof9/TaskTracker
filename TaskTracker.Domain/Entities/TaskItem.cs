using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.New;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
}
