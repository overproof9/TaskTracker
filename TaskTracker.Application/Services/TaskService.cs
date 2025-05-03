using TaskTracker.Application.DTO;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Models;
using TaskTracker.Domain.Entities;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;
using AutoMapper;

namespace TaskTracker.Application.Services;

public class TaskService(ITaskRepository taskRepository, IMapper mapper) : ITaskService
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<TaskDto>> GetAllAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<TaskDto>>(tasks);
    }

    public async Task<TaskDto?> GetByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return null;

        return _mapper.Map<TaskDto>(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskModel model)
    {
        var task = _mapper.Map<TaskItem>(model);
        task.Id = Guid.NewGuid();
        task.Status = TaskStatus.New;
        task.CreatedAt = DateTime.UtcNow;

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        return _mapper.Map<TaskDto>(task);
    }

    public async Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(Guid userId)
    {
        var tasks = await _taskRepository.GetAllAsync();
        var userTasks = tasks.Where(t => t.UserId == userId);
        return _mapper.Map<IEnumerable<TaskDto>>(userTasks);
    }

    public async Task<bool> AssignToUserAsync(Guid taskId, Guid newUserId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            return false;

        task.UserId = newUserId;
        await _taskRepository.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UpdateStatusAsync(Guid taskId, TaskStatus newStatus)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            return false;

        if (task.Status != newStatus)
        {
            task.Status = newStatus;
            await _taskRepository.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> DeleteAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            return false;

        _taskRepository.Delete(task);
        await _taskRepository.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<TaskDto>> GetOverdueTasksAsync()
    {
        var overdue = await _taskRepository.GetOverdueTasksAsync();

        return _mapper.Map<IEnumerable<TaskDto>>(overdue);
    }
}
