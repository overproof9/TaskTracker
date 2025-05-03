using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.BackgroundServices;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Infrastructure.Tests.Unit.BackgroundServices;

public class OverdueTaskUpdaterTests
{
    [Fact]
    public async Task OverdueTask_IsUpdatedAndSaved()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Status = TaskStatus.New,
            Deadline = DateTime.UtcNow.AddMinutes(-10)
        };

        var repoMock = new Mock<ITaskRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem> { task });

        var provider = new Mock<IServiceProvider>();
        provider.Setup(p => p.GetService(typeof(ITaskRepository))).Returns(repoMock.Object);

        var scope = new Mock<IServiceScope>();
        scope.Setup(s => s.ServiceProvider).Returns(provider.Object);

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        var logger = new Mock<ILogger<OverdueTaskUpdater>>();
        var service = new OverdueTaskUpdater(scopeFactory.Object, logger.Object);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cts.Token);

        // Assert
        Assert.Equal(TaskStatus.Overdue, task.Status);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CompletedTask_IsNotModified()
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Status = TaskStatus.Completed,
            Deadline = DateTime.UtcNow.AddMinutes(-10)
        };

        var repoMock = new Mock<ITaskRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskItem> { task });

        var provider = new Mock<IServiceProvider>();
        provider.Setup(p => p.GetService(typeof(ITaskRepository))).Returns(repoMock.Object);

        var scope = new Mock<IServiceScope>();
        scope.Setup(s => s.ServiceProvider).Returns(provider.Object);

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        var logger = new Mock<ILogger<OverdueTaskUpdater>>();
        var service = new OverdueTaskUpdater(scopeFactory.Object, logger.Object);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await service.StartAsync(cts.Token);

        repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        Assert.Equal(TaskStatus.Completed, task.Status);
    }

    [Fact]
    public async Task Exception_IsCaught_AndLogged()
    {
        var repoMock = new Mock<ITaskRepository>();
        repoMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("test"));

        var provider = new Mock<IServiceProvider>();
        provider.Setup(p => p.GetService(typeof(ITaskRepository))).Returns(repoMock.Object);

        var scope = new Mock<IServiceScope>();
        scope.Setup(s => s.ServiceProvider).Returns(provider.Object);

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        var logger = new Mock<ILogger<OverdueTaskUpdater>>();
        var service = new OverdueTaskUpdater(scopeFactory.Object, logger.Object);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await service.StartAsync(cts.Token);

        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce
        );
    }
}
