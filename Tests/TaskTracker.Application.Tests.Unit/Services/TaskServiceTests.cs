using AutoMapper;
using Moq;
using TaskTracker.Application.DTO;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Models;
using TaskTracker.Application.Services;
using TaskTracker.Domain.Entities;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Tests.Unit.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _mockRepo = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _service = new TaskService(_mockRepo.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedTasks()
    {
        var tasks = new List<TaskItem> { new() { Id = Guid.NewGuid(), Title = "Test" } };
        var dtos = new List<TaskDto> { new(Guid.NewGuid(), "T", "D", TaskStatus.New, TaskPriority.Medium, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid()) };

        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);
        _mockMapper.Setup(m => m.Map<IEnumerable<TaskDto>>(tasks)).Returns(dtos);

        var result = await _service.GetAllAsync();

        Assert.Equal(dtos, result);
        _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsDto()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Title = "X" };
        var dto = new TaskDto(task.Id, "X", "D", TaskStatus.New, TaskPriority.Low, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid());

        _mockRepo.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);
        _mockMapper.Setup(m => m.Map<TaskDto>(task)).Returns(dto);

        var result = await _service.GetByIdAsync(task.Id);

        Assert.Equal(dto, result);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((TaskItem?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_AddsTaskAndReturnsDto()
    {
        var model = new CreateTaskModel("T", "D", TaskPriority.High, DateTime.UtcNow.AddHours(2), Guid.NewGuid());
        var entity = new TaskItem { Title = model.Title, Description = model.Description, Deadline = model.Deadline, Priority = model.Priority, UserId = model.UserId };
        var dto = new TaskDto(Guid.NewGuid(), model.Title, model.Description, TaskStatus.New, model.Priority, model.Deadline, DateTime.UtcNow, model.UserId);

        _mockMapper.Setup(m => m.Map<TaskItem>(model)).Returns(entity);
        _mockMapper.Setup(m => m.Map<TaskDto>(It.IsAny<TaskItem>())).Returns(dto);

        var result = await _service.CreateAsync(model);

        _mockRepo.Verify(r => r.AddAsync(It.Is<TaskItem>(t => t.Title == model.Title)), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.Equal(dto, result);
    }

    [Fact]
    public async Task GetTasksByUserIdAsync_ReturnsFilteredMapped()
    {
        var userId = Guid.NewGuid();
        var all = new List<TaskItem>
        {
            new() { Id = Guid.NewGuid(), Title = "1", UserId = userId },
            new() { Id = Guid.NewGuid(), Title = "2", UserId = Guid.NewGuid() }
        };
        var expected = new List<TaskDto> { new(all[0].Id, "1", "D", TaskStatus.New, TaskPriority.Low, DateTime.UtcNow, DateTime.UtcNow, userId) };

        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(all);
        _mockMapper.Setup(m => m.Map<IEnumerable<TaskDto>>(It.Is<IEnumerable<TaskItem>>(t => t.All(x => x.UserId == userId)))).Returns(expected);

        var result = await _service.GetTasksByUserIdAsync(userId);

        Assert.Single(result);
        Assert.Equal(userId, result.First().UserId);
    }

    [Fact]
    public async Task AssignToUserAsync_UpdatesUserId()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        var newUser = Guid.NewGuid();

        _mockRepo.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

        var result = await _service.AssignToUserAsync(task.Id, newUser);

        Assert.True(result);
        Assert.Equal(newUser, task.UserId);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ChangesStatus()
    {
        var task = new TaskItem { Id = Guid.NewGuid(), Status = TaskStatus.New };
        _mockRepo.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

        var result = await _service.UpdateStatusAsync(task.Id, TaskStatus.Completed);

        Assert.True(result);
        Assert.Equal(TaskStatus.Completed, task.Status);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTask()
    {
        var task = new TaskItem { Id = Guid.NewGuid() };
        _mockRepo.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

        var result = await _service.DeleteAsync(task.Id);

        Assert.True(result);
        _mockRepo.Verify(r => r.Delete(task), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetOverdueTasksAsync_ReturnsMapped()
    {
        var raw = new List<TaskItem> { new() { Id = Guid.NewGuid(), Deadline = DateTime.UtcNow.AddHours(-1) } };
        var dto = new List<TaskDto> { new(Guid.NewGuid(), "Late", "X", TaskStatus.Overdue, TaskPriority.Low, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-2), Guid.NewGuid()) };

        _mockRepo.Setup(r => r.GetOverdueTasksAsync()).ReturnsAsync(raw);
        _mockMapper.Setup(m => m.Map<IEnumerable<TaskDto>>(raw)).Returns(dto);

        var result = await _service.GetOverdueTasksAsync();

        Assert.Equal(dto, result);
    }
}
