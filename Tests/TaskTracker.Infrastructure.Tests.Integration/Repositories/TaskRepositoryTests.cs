using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Repositories;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Infrastructure.Tests.Integration.Repositories;

public class TaskRepositoryTests
{
    private User user;
    private async Task<AppDbContext> CreateDbContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "TestEmail@test.com"
        };
        context.Users.Add(user);

        return context;
    }

    [Fact]
    public async Task AddAndGetByIdAsync_ShouldReturnSameEntity()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var context = await CreateDbContextAsync(connection);

        var repo = new TaskRepository(context);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Integration Test",
            Description = "Repo test",
            Status = TaskStatus.New,
            Deadline = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        };

        await repo.AddAsync(task);
        await repo.SaveChangesAsync();

        var result = await repo.GetByIdAsync(task.Id);

        Assert.NotNull(result);
        Assert.Equal(task.Title, result!.Title);

        await connection.CloseAsync();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var context = await CreateDbContextAsync(connection);

        var repo = new TaskRepository(context);

        await repo.AddAsync(new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task 1",
            Description = "Test",
            Deadline = DateTime.UtcNow.AddDays(1),
            Status = TaskStatus.New,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        });

        await repo.AddAsync(new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Task 2",
            Description = "Test 2",
            Deadline = DateTime.UtcNow.AddDays(2),
            Status = TaskStatus.New,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        });

        await repo.SaveChangesAsync();

        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count());

        await connection.CloseAsync();
    }

    [Fact]
    public async Task GetOverdueTasksAsync_ShouldReturnOnlyOverdue()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var context = await CreateDbContextAsync(connection);

        var repo = new TaskRepository(context);

        var overdue = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Overdue",
            Description = "Too late",
            Status = TaskStatus.New,
            Deadline = DateTime.UtcNow.AddMinutes(-30),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UserId = user.Id
        };

        var upcoming = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Not yet",
            Description = "Still time",
            Status = TaskStatus.New,
            Deadline = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        };

        await repo.AddAsync(overdue);
        await repo.AddAsync(upcoming);
        await repo.SaveChangesAsync();

        var result = await repo.GetOverdueTasksAsync();

        Assert.Single(result);
        Assert.Equal(overdue.Id, result.ElementAt(0).Id);

        await connection.CloseAsync();
    }
}
