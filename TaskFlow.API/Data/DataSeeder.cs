using Microsoft.EntityFrameworkCore;
using TaskFlow.API.Models;
using TaskStatus = TaskFlow.API.Models.TaskStatus;

namespace TaskFlow.API.Data;

/// <summary>
/// Seeds demo data for users who have no projects.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var users = await context.Users.ToListAsync();
        foreach (var user in users)
        {
            var hasProjects = await context.Projects.AnyAsync(p => p.OwnerId == user.Id);
            if (hasProjects) continue;

            var now = DateTime.UtcNow;
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = "My First Project",
                Description = "Demo project with sample tasks",
                OwnerId = user.Id,
                IsArchived = false
            };
            context.Projects.Add(project);

            var tasks = new[]
            {
                new TaskItem { Id = Guid.NewGuid(), Title = "Review documentation", Description = "Read through the project docs", Status = TaskStatus.Todo, Priority = TaskPriority.Medium, Tags = "Work", ProjectId = project.Id, CreatedAt = now, UpdatedAt = now },
                new TaskItem { Id = Guid.NewGuid(), Title = "Set up development environment", Description = "Install required tools", Status = TaskStatus.InProgress, Priority = TaskPriority.High, Tags = "Work,Urgent", ProjectId = project.Id, CreatedAt = now, UpdatedAt = now },
                new TaskItem { Id = Guid.NewGuid(), Title = "Complete onboarding", Description = "Finish team onboarding process", Status = TaskStatus.Completed, Priority = TaskPriority.Medium, Tags = "Personal", ProjectId = project.Id, CreatedAt = now, UpdatedAt = now, CompletionNote = "All steps completed successfully" },
                new TaskItem { Id = Guid.NewGuid(), Title = "Schedule team meeting", Description = "Book a slot for weekly sync", Status = TaskStatus.Todo, Priority = TaskPriority.Low, Tags = "Work", ProjectId = project.Id, CreatedAt = now, UpdatedAt = now },
                new TaskItem { Id = Guid.NewGuid(), Title = "Update project roadmap", Description = "Add Q2 milestones", Status = TaskStatus.Todo, Priority = TaskPriority.High, Tags = "Work,Urgent", ProjectId = project.Id, CreatedAt = now, UpdatedAt = now },
            };

            foreach (var t in tasks)
                context.Tasks.Add(t);
        }

        await context.SaveChangesAsync();
    }
}
