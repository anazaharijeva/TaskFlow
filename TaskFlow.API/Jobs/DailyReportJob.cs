using Microsoft.EntityFrameworkCore;
using TaskFlow.API.Data;
using TaskFlow.API.Models;

namespace TaskFlow.API.Jobs;

/// <summary>
/// Hangfire background job - generates daily productivity report.
/// Scheduled to run daily (e.g., at 9 AM).
/// In production, this could send emails or push notifications.
/// </summary>
public class DailyReportJob
{
    private readonly AppDbContext _context;

    public DailyReportJob(AppDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync()
    {
        var users = await _context.Users.ToListAsync();

        foreach (var user in users)
        {
            var projects = await _context.Projects
                .Where(p => p.OwnerId == user.Id)
                .Include(p => p.Tasks)
                .ToListAsync();

            var totalTasks = projects.SelectMany(p => p.Tasks).Count();
            var completed = projects.SelectMany(p => p.Tasks).Count(t => t.Status == TaskFlow.API.Models.TaskStatus.Completed);
            var inProgress = projects.SelectMany(p => p.Tasks).Count(t => t.Status == TaskFlow.API.Models.TaskStatus.InProgress);

            // Log the report (in production: send email, push notification, etc.)
            Serilog.Log.Information(
                "Daily Report for {Email}: Projects={Projects}, Total Tasks={Total}, Completed={Completed}, In Progress={InProgress}",
                user.Email, projects.Count, totalTasks, completed, inProgress);
        }
    }
}
