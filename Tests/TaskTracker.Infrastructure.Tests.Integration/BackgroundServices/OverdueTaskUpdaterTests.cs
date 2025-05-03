using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.BackgroundServices;
using TaskTracker.Infrastructure.Repositories;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Infrastructure.Tests.Integration.BackgroundServices;

public class OverdueTaskUpdaterTests
{
    private async Task<AppDbContext> CreateDbContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    [Fact]
    public async Task BackgroundService_UpdatesOverdueTasks_InRealDb()
    {
        // Arrange
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var context = await CreateDbContextAsync(connection);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "TestEmail@test.com"
        };
        context.Users.Add(user);

        var oldTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Overdue",
            Description = "This is overdue",
            Status = TaskStatus.New,
            Deadline = DateTime.UtcNow.AddMinutes(-30),
            UserId = user.Id
        };

        var recentTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Future",
            Description = "This is in the future",
            Status = TaskStatus.New,
            Deadline = DateTime.UtcNow.AddMinutes(30),
            UserId = user.Id
        };

        context.Tasks.AddRange(oldTask, recentTask);
        await context.SaveChangesAsync();

        var repo = new TaskRepository(context);
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<OverdueTaskUpdater>();
        var serviceProvider = new ServiceCollection()
            .AddScoped<ITaskRepository>(_ => repo)
            .BuildServiceProvider();

        var scopeFactory = new TestScopeFactory(serviceProvider);
        var service = new OverdueTaskUpdater(scopeFactory, logger);

        // Act
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));
        await service.StartAsync(cts.Token);

        // Assert
        var updated = await context.Tasks.FirstAsync(t => t.Id == oldTask.Id);
        Assert.Equal(TaskStatus.Overdue, updated.Status);

        var untouched = await context.Tasks.FirstAsync(t => t.Id == recentTask.Id);
        Assert.Equal(TaskStatus.New, untouched.Status);

        await connection.CloseAsync();
    }

    private class TestScopeFactory(IServiceProvider provider) : IServiceScopeFactory
    {
        private readonly IServiceProvider _provider = provider;

        public IServiceScope CreateScope() => new TestScope(_provider);

        private class TestScope(IServiceProvider provider) : IServiceScope
        {
            public IServiceProvider ServiceProvider { get; } = provider;

            public void Dispose() { }
        }
    }
}
