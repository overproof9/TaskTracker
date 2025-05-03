using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskTracker.Application.DTO;
using TaskTracker.Domain.Enums;
using TaskTracker.Presentation;
using TaskTracker.Presentation.Models.Requests;
using TaskTracker.Presentation.Tests.Integration.Controllers;
using Xunit;
using TaskStatus = TaskTracker.Domain.Enums.TaskStatus;

namespace TaskTracker.PresentationTests.Integration.Controllers;

public class TasksControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TasksControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateUserAsync()
    {
        var user = new CreateUserRequest($"Test User", $"test{Guid.NewGuid()}@mail.com");
        var response = await _client.PostAsJsonAsync("/api/users", user);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<UserDto>();
        return created!.Id;
    }

    private async Task<Guid> CreateTaskAsync(Guid? userId = null)
    {
        var uid = userId ?? await CreateUserAsync();
        var request = new CreateTaskRequest(
            "Integration Test Task",
            "Created during integration test",
            TaskPriority.Medium,
            DateTime.UtcNow.AddHours(1),
            uid
        );

        var response = await _client.PostAsJsonAsync("/api/tasks", request);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<TaskDto>();
        return created!.Id;
    }

    [Fact]
    public async Task Create_Returns_Created_Task()
    {
        var id = await CreateTaskAsync();
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task GetAll_Returns_Ok()
    {
        var response = await _client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Returns_Task_IfExists()
    {
        var id = await CreateTaskAsync();
        var response = await _client.GetAsync($"/api/tasks/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.Equal(id, task!.Id);
    }

    [Fact]
    public async Task UpdateStatus_Returns_NoContent()
    {
        var id = await CreateTaskAsync();
        var request = new UpdateTaskStatusRequest(TaskStatus.InProgress);

        var response = await _client.PutAsJsonAsync($"/api/tasks/{id}/status", request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Removes_Task()
    {
        var id = await CreateTaskAsync();
        var response = await _client.DeleteAsync($"/api/tasks/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetOverdue_Returns_Ok()
    {
        var response = await _client.GetAsync("/api/tasks/overdue");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetByUserId_Returns_Tasks()
    {
        var userId = await CreateUserAsync();
        await CreateTaskAsync(userId);

        var response = await _client.GetAsync($"/api/tasks/user/{userId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tasks = await response.Content.ReadFromJsonAsync<IEnumerable<TaskDto>>();
        Assert.All(tasks!, t => Assert.Equal(userId, t.UserId));
    }

    [Fact]
    public async Task AssignToUser_ChangesUser()
    {
        var taskId = await CreateTaskAsync();
        var newUser = await CreateUserAsync();

        var request = new AssignTaskRequest(newUser);
        var response = await _client.PutAsJsonAsync($"/api/tasks/{taskId}/assign", request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
