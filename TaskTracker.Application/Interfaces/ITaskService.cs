using TaskTracker.Application.DTO;
using TaskTracker.Application.Models;

namespace TaskTracker.Application.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllAsync();
    Task<TaskDto?> GetByIdAsync(Guid id);
    Task<TaskDto> CreateAsync(CreateTaskModel model);
    Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(Guid userId);
    Task<bool> AssignToUserAsync(Guid taskId, Guid newUserId);
    Task<bool> UpdateStatusAsync(Guid taskId, Domain.Enums.TaskStatus newStatus);
    Task<bool> DeleteAsync(Guid taskId);
    Task<IEnumerable<TaskDto>> GetOverdueTasksAsync();
}
