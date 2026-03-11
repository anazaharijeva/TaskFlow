using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.DTOs;
using TaskFlow.API.Services;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Projects controller - CRUD operations for projects.
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ITaskService _taskService;

    public ProjectsController(IProjectService projectService, ITaskService taskService)
    {
        _projectService = projectService;
        _taskService = taskService;
    }

    /// <summary>
    /// GET /api/projects - Get all projects for the authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProjects([FromQuery] bool includeArchived = false)
    {
        var userId = GetUserId();
        var projects = await _projectService.GetProjectsByUserAsync(userId, includeArchived);
        return Ok(projects);
    }

    /// <summary>
    /// GET /api/projects/{id} - Get a single project by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProject(Guid id)
    {
        var userId = GetUserId();
        var project = await _projectService.GetByIdAsync(id, userId);

        if (project == null)
            return NotFound();

        return Ok(project);
    }

    /// <summary>
    /// POST /api/projects - Create a new project.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
    {
        var userId = GetUserId();
        var project = await _projectService.CreateAsync(dto, userId);
        return Ok(project);
    }

    /// <summary>
    /// PUT /api/projects/{id} - Update an existing project.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectDto dto)
    {
        var userId = GetUserId();
        var project = await _projectService.UpdateAsync(id, dto, userId);

        if (project == null)
            return NotFound();

        return Ok(project);
    }

    /// <summary>
    /// GET /api/projects/{projectId}/tasks - Get all tasks for a project.
    /// </summary>
    [HttpGet("{projectId:guid}/tasks")]
    public async Task<IActionResult> GetProjectTasks(Guid projectId, [FromQuery][Bind(Prefix = "")] TaskFilterDto? filter)
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetTasksByProjectAsync(projectId, userId, filter);
        return Ok(tasks);
    }

    /// <summary>
    /// GET /api/projects/{id}/assignable - Get users that can be assigned to tasks (owner + members).
    /// </summary>
    [HttpGet("{id:guid}/assignable")]
    public async Task<IActionResult> GetAssignableUsers(Guid id)
    {
        var userId = GetUserId();
        var users = await _projectService.GetAssignableUsersAsync(id, userId);
        return Ok(users);
    }

    /// <summary>
    /// GET /api/projects/{id}/members - Get project members.
    /// </summary>
    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetProjectMembers(Guid id)
    {
        var userId = GetUserId();
        var members = await _projectService.GetMembersAsync(id, userId);
        return Ok(members);
    }

    /// <summary>
    /// POST /api/projects/{id}/members - Add a member by email (owner only).
    /// </summary>
    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddProjectMember(Guid id, [FromBody] AddProjectMemberDto dto)
    {
        var userId = GetUserId();
        var ok = await _projectService.AddMemberAsync(id, dto, userId);
        if (!ok) return BadRequest(new { message = "Project not found, user not found, or already a member" });
        return NoContent();
    }

    /// <summary>
    /// DELETE /api/projects/{id}/members/{memberId} - Remove a member (owner only).
    /// </summary>
    [HttpDelete("{id:guid}/members/{memberUserId:guid}")]
    public async Task<IActionResult> RemoveProjectMember(Guid id, Guid memberUserId)
    {
        var userId = GetUserId();
        var ok = await _projectService.RemoveMemberAsync(id, memberUserId, userId);
        if (!ok) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// DELETE /api/projects/{id} - Delete a project and all its tasks.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var userId = GetUserId();
        var deleted = await _projectService.DeleteAsync(id, userId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Extracts user ID from JWT claims (NameIdentifier claim).
    /// </summary>
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found"));
    }
}
