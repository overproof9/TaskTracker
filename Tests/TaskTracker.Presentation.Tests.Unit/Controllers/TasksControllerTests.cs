using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskTracker.Application.DTO;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Models;
using TaskTracker.Domain.Enums;
using TaskTracker.Presentation.Controllers;
using TaskTracker.Presentation.Models.Requests;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.Presentation.Tests.Unit.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _mockService = new();
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _controller = new TasksController(_mockService.Object);
    }

    [Fact]
    public async Task Create_Returns_CreatedAndCallsService()
    {
        var request = new CreateTaskRequest("Title", "Desc", TaskPriority.Medium, DateTime.UtcNow.AddDays(1), Guid.NewGuid());
        var dto = new TaskDto(Guid.NewGuid(), request.Title, request.Description, TaskStatus.New, request.Priority, request.Deadline, DateTime.UtcNow, request.UserId);

        _mockService
            .Setup(s => s.CreateAsync(It.IsAny<CreateTaskModel>()))
            .ReturnsAsync(dto);

        var result = await _controller.Create(request);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.IsType<TaskDto>(created.Value);

        _mockService.Verify(s => s.CreateAsync(It.Is<CreateTaskModel>(m =>
            m.Title == request.Title &&
            m.Description == request.Description &&
            m.Priority == request.Priority &&
            m.Deadline == request.Deadline &&
            m.UserId == request.UserId)), Times.Once);
    }

    [Fact]
    public async Task GetAll_Returns_OkAndCallsService()
    {
        var dtoList = new List<TaskDto> { new(Guid.NewGuid(), "T", "D", TaskStatus.New, TaskPriority.Low, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid()) };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(dtoList);

        var result = await _controller.GetAll();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(dtoList, ok.Value);

        _mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetById_Found_Returns_Task()
    {
        var id = Guid.NewGuid();
        var dto = new TaskDto(id, "Title", "Desc", TaskStatus.New, TaskPriority.High, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid());

        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(dto);

        var result = await _controller.GetById(id);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(dto, ok.Value);

        _mockService.Verify(s => s.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetById_NotFound_Returns_404()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((TaskDto?)null);

        var result = await _controller.GetById(id);
        Assert.IsType<NotFoundResult>(result.Result);

        _mockService.Verify(s => s.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_CallsService_And_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var status = TaskStatus.Completed;

        _mockService.Setup(s => s.UpdateStatusAsync(id, status)).ReturnsAsync(true);

        var result = await _controller.UpdateStatus(id, new(status));
        Assert.IsType<NoContentResult>(result);

        _mockService.Verify(s => s.UpdateStatusAsync(id, status), Times.Once);
    }

    [Fact]
    public async Task Delete_CallsService_And_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

        var result = await _controller.Delete(id);
        Assert.IsType<NoContentResult>(result);

        _mockService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetOverdue_Returns_Ok_And_CallsService()
    {
        var list = new List<TaskDto> { new(Guid.NewGuid(), "Overdue", "desc", TaskStatus.Overdue, TaskPriority.Low, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid()) };
        _mockService.Setup(s => s.GetOverdueTasksAsync()).ReturnsAsync(list);

        var result = await _controller.GetOverdue();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(list, ok.Value);

        _mockService.Verify(s => s.GetOverdueTasksAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByUserId_Returns_Ok_And_Filters()
    {
        var userId = Guid.NewGuid();
        var list = new List<TaskDto> { new(Guid.NewGuid(), "Task", "desc", TaskStatus.New, TaskPriority.Medium, DateTime.UtcNow, DateTime.UtcNow, userId) };

        _mockService.Setup(s => s.GetTasksByUserIdAsync(userId)).ReturnsAsync(list);

        var result = await _controller.GetByUserId(userId);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(list, ok.Value);

        _mockService.Verify(s => s.GetTasksByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task AssignToUser_CallsService_And_ReturnsNoContent()
    {
        var taskId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();

        _mockService.Setup(s => s.AssignToUserAsync(taskId, newUserId)).ReturnsAsync(true);

        var result = await _controller.AssignToUser(taskId, new(newUserId));
        Assert.IsType<NoContentResult>(result);

        _mockService.Verify(s => s.AssignToUserAsync(taskId, newUserId), Times.Once);
    }
}
