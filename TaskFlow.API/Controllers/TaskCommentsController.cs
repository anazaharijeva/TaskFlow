using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.DTOs;
using TaskFlow.API.Services;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/comments")]
[Authorize]
public class TaskCommentsController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskCommentsController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetComments(Guid taskId)
    {
        var userId = GetUserId();
        var comments = await _taskService.GetCommentsAsync(taskId, userId);
        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(Guid taskId, [FromBody] CreateTaskCommentDto dto)
    {
        var userId = GetUserId();
        var comment = await _taskService.AddCommentAsync(taskId, dto, userId);
        if (comment == null) return NotFound();
        return Ok(comment);
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid taskId, Guid commentId)
    {
        var userId = GetUserId();
        var ok = await _taskService.DeleteCommentAsync(taskId, commentId, userId);
        if (!ok) return NotFound();
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found"));
    }
}
