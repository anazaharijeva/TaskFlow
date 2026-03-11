using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Data;
using TaskFlow.API.Models;
using TaskStatus = TaskFlow.API.Models.TaskStatus;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Seed controller - creates demo data for the current user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _context;

    public SeedController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// POST /api/seed/demo - Create a demo project with tasks for the current user.
    /// </summary>
    [HttpPost("demo")]
    public async Task<IActionResult> SeedDemo()
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var now = DateTime.UtcNow;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "My First Project",
            Description = "Demo project with sample tasks",
            OwnerId = userId,
            IsArchived = false
        };
        _context.Projects.Add(project);

        var tasks = new[]
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Review documentation", Description = "Read through the project docs", Status = TaskStatus.Todo, Priority = TaskPriority.Medium, Tags = "Work", ProjectId = project.Id, CreatedAt = now, UpdatedAt = now },
            new TaskItem { Id = Guid.NewGuid(), Title = "Set up development environment", Description = "Install required tools", Status = TaskStatus.InProgress, Priority = TaskPriority.High, Tags = "Work,Urgent", ProjectId = project.Id, CreatedAt = now, UpdatedAt = now },
            new TaskItem { Id = Guid.NewGuid(), Title = "Complete onboarding", Description = "Finish team onboarding process", Status = TaskStatus.Completed, Priority = TaskPriority.Medium, Tags = "Personal", ProjectId = project.Id, CreatedAt = now, UpdatedAt = now, CompletionNote = "All steps completed successfully" },
            new TaskItem { Id = Guid.NewGuid(), Title = "Schedule team meeting", Description = "Book a slot for weekly sync", Status = TaskStatus.Todo, Priority = TaskPriority.Low, Tags = "Work", ProjectId = project.Id, CreatedAt = now, UpdatedAt = now },
            new TaskItem { Id = Guid.NewGuid(), Title = "Update project roadmap", Description = "Add Q2 milestones", Status = TaskStatus.Todo, Priority = TaskPriority.High, Tags = "Work,Urgent", ProjectId = project.Id, CreatedAt = now, UpdatedAt = now },
        };

        foreach (var t in tasks)
            _context.Tasks.Add(t);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Demo project created successfully",
            projectId = project.Id,
            projectName = project.Name,
            tasksCreated = tasks.Length
        });
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim ?? throw new UnauthorizedAccessException("User ID not found"));
    }
}
