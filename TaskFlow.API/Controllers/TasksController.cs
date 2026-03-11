using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.DTOs;
using TaskFlow.API.Services;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Tasks controller - CRUD operations for tasks.
/// Tasks can be accessed via project or directly by ID.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// GET /api/tasks/project/{projectId}/export - Export tasks as JSON or CSV.
    /// </summary>
    [HttpGet("project/{projectId:guid}/export")]
    public async Task<IActionResult> ExportTasks(Guid projectId, [FromQuery] string format = "json")
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetTasksByProjectAsync(projectId, userId);
        var list = tasks.ToList();
        if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = "Title,Description,Status,Priority,Tags,DueDate,Assignee,CreatedAt\n" +
                string.Join("\n", list.Select(t =>
                    $"\"{EscapeCsv(t.Title)}\",\"{EscapeCsv(t.Description)}\",{t.Status},{t.Priority},\"{EscapeCsv(t.Tags)}\",{t.DueDate?.ToString("yyyy-MM-dd") ?? ""},\"{EscapeCsv(t.AssigneeName ?? "")}\",{t.CreatedAt:yyyy-MM-dd}"));
            return Content(csv, "text/csv; charset=utf-8");
        }
        return Ok(list);
    }

    /// <summary>
    /// GET /api/tasks/project/{projectId} - Get all tasks for a project with optional filters.
    /// </summary>
    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetTasksByProject(Guid projectId, [FromQuery][Bind(Prefix = "")] TaskFilterDto? filter)
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetTasksByProjectAsync(projectId, userId, filter);
        return Ok(tasks);
    }

    /// <summary>
    /// GET /api/tasks/{id} - Get a single task by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTask(Guid id)
    {
        var userId = GetUserId();
        var task = await _taskService.GetByIdAsync(id, userId);

        if (task == null)
            return NotFound();

        return Ok(task);
    }

    /// <summary>
    /// POST /api/tasks - Create a new task.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        var userId = GetUserId();
        var task = await _taskService.CreateAsync(dto, userId);

        if (task == null)
            return BadRequest(new { message = "Project not found or access denied" });

        return Ok(task);
    }

    /// <summary>
    /// PUT /api/tasks/{id} - Update an existing task.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto dto)
    {
        var userId = GetUserId();
        var task = await _taskService.UpdateAsync(id, dto, userId);

        if (task == null)
            return NotFound();

        return Ok(task);
    }

    /// <summary>
    /// POST /api/tasks/project/{projectId}/import - Import tasks from JSON or CSV.
    /// </summary>
    [HttpPost("project/{projectId:guid}/import")]
    public async Task<IActionResult> ImportTasks(Guid projectId, [FromBody] ImportTasksDto dto)
    {
        var userId = GetUserId();
        var count = await _taskService.ImportTasksAsync(projectId, userId, dto);
        if (count < 0) return BadRequest(new { message = "Invalid format or project not found" });
        return Ok(new { imported = count });
    }

    /// <summary>
    /// DELETE /api/tasks/{id} - Delete a task.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var userId = GetUserId();
        var deleted = await _taskService.DeleteAsync(id, userId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private static string EscapeCsv(string s) => s.Replace("\"", "\"\"");

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found"));
    }
}
