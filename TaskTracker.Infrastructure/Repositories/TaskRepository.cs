using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Infrastructure.Repositories;

public class TaskRepository(AppDbContext context) : GenericRepository<TaskItem>(context), ITaskRepository
{
    public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync()
    {
        return await _dbSet
            .Where(t => t.Deadline < DateTime.UtcNow && t.Status != Domain.Enums.TaskStatus.Completed)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetTasksToMarkOverdueAsync()
    {
        return await _dbSet
            .Where(t => t.Status != Domain.Enums.TaskStatus.Completed &&
                        t.Status != Domain.Enums.TaskStatus.Overdue &&
                        t.Deadline < DateTime.UtcNow)
            .ToListAsync();
    }
}
