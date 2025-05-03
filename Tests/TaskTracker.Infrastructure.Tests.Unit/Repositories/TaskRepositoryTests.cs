using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Repositories;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Infrastructure.Tests.Unit.Repositories;

public class TaskRepositoryTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddAsync_AddsEntityToDatabase()
    {
        using var context = CreateInMemoryContext();
        var repo = new TaskRepository(context);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Test Add",
            Description = "Unit test",
            CreatedAt = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddHours(1),
            Status = TaskStatus.New
        };

        await repo.AddAsync(task);
        await repo.SaveChangesAsync();

        var fromDb = await repo.GetByIdAsync(task.Id);

        Assert.NotNull(fromDb);
        Assert.Equal("Test Add", fromDb!.Title);
    }

    [Fact]
    public async Task Update_ChangesPersisted()
    {
        using var context = CreateInMemoryContext();
        var repo = new TaskRepository(context);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Before Update",
            Description = "Old",
            CreatedAt = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(1),
            Status = TaskStatus.New
        };

        await repo.AddAsync(task);
        await repo.SaveChangesAsync();

        task.Title = "Updated Title";
        repo.Update(task);
        await repo.SaveChangesAsync();

        var updated = await repo.GetByIdAsync(task.Id);
        Assert.Equal("Updated Title", updated!.Title);
    }

    [Fact]
    public async Task Delete_RemovesEntity()
    {
        using var context = CreateInMemoryContext();
        var repo = new TaskRepository(context);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "To delete",
            Description = "Delete me",
            CreatedAt = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(2),
            Status = TaskStatus.New
        };

        await repo.AddAsync(task);
        await repo.SaveChangesAsync();

        repo.Delete(task);
        await repo.SaveChangesAsync();

        var found = await repo.GetByIdAsync(task.Id);
        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllItems()
    {
        using var context = CreateInMemoryContext();
        var repo = new TaskRepository(context);

        var task1 = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task 1",
            Description = "First",
            CreatedAt = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(1),
            Status = TaskStatus.New
        };

        var task2 = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task 2",
            Description = "Second",
            CreatedAt = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(1),
            Status = TaskStatus.New
        };

        await repo.AddAsync(task1);
        await repo.AddAsync(task2);
        await repo.SaveChangesAsync();

        var all = await repo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    [Fact]
    public async Task GetOverdueTasksAsync_ReturnsOnlyOverdue()
    {
        using var context = CreateInMemoryContext();
        var repo = new TaskRepository(context);

        var overdue = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Overdue",
            Description = "Missed",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            Deadline = DateTime.UtcNow.AddMinutes(-10),
            Status = TaskStatus.New
        };

        var onTime = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Upcoming",
            Description = "Still good",
            CreatedAt = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddMinutes(10),
            Status = TaskStatus.New
        };

        var completed = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Completed",
            Description = "Already done",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Deadline = DateTime.UtcNow.AddMinutes(-60),
            Status = TaskStatus.Completed
        };

        await repo.AddAsync(overdue);
        await repo.AddAsync(onTime);
        await repo.AddAsync(completed);
        await repo.SaveChangesAsync();

        var result = await repo.GetOverdueTasksAsync();
        Assert.Single(result);
        Assert.Equal("Overdue", result.First().Title);
    }
}
