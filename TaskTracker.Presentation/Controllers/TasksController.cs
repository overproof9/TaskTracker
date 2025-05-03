using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.DTO;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Models;
using TaskTracker.Presentation.Models.Requests;

namespace TaskTracker.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService;

    /// <summary>
    /// Create new task
    /// </summary>
    /// <param name="request"></param>
    /// <returns>TaskDto</returns>
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskRequest request)
    {
        var model = new CreateTaskModel(
            request.Title,
            request.Description,
            request.Priority,
            request.Deadline,
            request.UserId
        );

        var result = await _taskService.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get all tasks
    /// </summary>
    /// <returns>IEnumerable<TaskDto></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll()
    {
        var tasks = await _taskService.GetAllAsync();
        return Ok(tasks);
    }

    /// <summary>
    /// Get task by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns>TaskDto</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        return task == null ? NotFound() : Ok(task);
    }

    /// <summary>
    /// Update task status
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns>No content</returns>
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        var updated = await _taskService.UpdateStatusAsync(id, request.NewStatus);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>
    /// Delete task
    /// </summary>
    /// <param name="id"></param>
    /// <returns>No contnet</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _taskService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Get overdue tasks
    /// </summary>
    /// <returns>IEnumerable<TaskDto></returns>
    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetOverdue()
    {
        var tasks = await _taskService.GetOverdueTasksAsync();
        return Ok(tasks);
    }

    /// <summary>
    /// Get tasks by userId
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>IEnumerable<TaskDto></returns>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetByUserId(Guid userId)
    {
        var tasks = await _taskService.GetTasksByUserIdAsync(userId);
        return Ok(tasks);
    }

    /// <summary>
    /// Assign task to user
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="request"></param>
    /// <returns>No content</returns>
    [HttpPut("{taskId:guid}/assign")]
    public async Task<IActionResult> AssignToUser(Guid taskId, [FromBody] AssignTaskRequest request)
    {
        var result = await _taskService.AssignToUserAsync(taskId, request.NewUserId);
        return result ? NoContent() : NotFound();
    }
}
