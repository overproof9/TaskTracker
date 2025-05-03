using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskTracker.Application.Interfaces;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Infrastructure.BackgroundServices;

public class OverdueTaskUpdater(
    IServiceScopeFactory scopeFactory,
    ILogger<OverdueTaskUpdater> logger
) : BackgroundService
{
    private const int DELAY_MINUTES = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

                var tasks = await repository.GetTasksToMarkOverdueAsync();
                var now = DateTime.UtcNow;

                int updated = 0;

                foreach (var task in tasks)
                {

                    task.Status = TaskStatus.Overdue;
                    updated++;
                }

                if (updated > 0)
                {
                    await repository.SaveChangesAsync();
                    logger.LogInformation("Updated {Count} tasks to Overdue status.", updated);
                }
                else
                {
                    logger.LogInformation("No new overdue tasks");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while updating overdue tasks");
            }

            await Task.Delay(TimeSpan.FromMinutes(DELAY_MINUTES), stoppingToken);
        }
    }
}
