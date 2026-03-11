using Microsoft.EntityFrameworkCore;
using TaskFlow.API.Data;
using TaskFlow.API.Models;

namespace TaskFlow.API.Services;

/// <summary>
/// Analytics service implementation.
/// Calculates productivity metrics from user's projects and tasks.
/// Productivity score = (completed / total) * 100 when total > 0.
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _context;

    public AnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync(Guid userId)
    {
        var projects = await _context.Projects
            .Where(p => (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)) && !p.IsArchived)
            .Include(p => p.Tasks)
            .ToListAsync();

        var totalProjects = projects.Count;
        var tasksCompleted = projects.SelectMany(p => p.Tasks).Count(t => t.Status == TaskFlow.API.Models.TaskStatus.Completed);
        var tasksInProgress = projects.SelectMany(p => p.Tasks).Count(t => t.Status == TaskFlow.API.Models.TaskStatus.InProgress);
        var tasksTodo = projects.SelectMany(p => p.Tasks).Count(t => t.Status == TaskFlow.API.Models.TaskStatus.Todo);
        var totalTasks = tasksCompleted + tasksInProgress + tasksTodo;

        // Productivity score: percentage of completed tasks (0-100%)
        var productivityScore = totalTasks > 0
            ? Math.Round((double)tasksCompleted / totalTasks * 100, 0).ToString() + "%"
            : "0%";

        // Per-project breakdown for chart: which project has how many Todo/InProgress/Completed
        var projectStats = projects.Select(p => new ProjectTaskStatsDto
        {
            ProjectName = p.Name,
            Todo = p.Tasks.Count(t => t.Status == TaskFlow.API.Models.TaskStatus.Todo),
            InProgress = p.Tasks.Count(t => t.Status == TaskFlow.API.Models.TaskStatus.InProgress),
            Completed = p.Tasks.Count(t => t.Status == TaskFlow.API.Models.TaskStatus.Completed)
        }).ToList();

        return new DashboardMetricsDto
        {
            TotalProjects = totalProjects,
            TasksCompleted = tasksCompleted,
            TasksInProgress = tasksInProgress,
            TasksTodo = tasksTodo,
            ProductivityScore = productivityScore,
            ProjectStats = projectStats
        };
    }

    public async Task<PeriodStatsDto> GetWeeklyStatsAsync(Guid userId)
    {
        var startOfWeek = DateTime.UtcNow.Date;
        while (startOfWeek.DayOfWeek != DayOfWeek.Monday) startOfWeek = startOfWeek.AddDays(-1);
        var endOfWeek = startOfWeek.AddDays(7);
        var completed = await _context.Tasks
            .Where(t => (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId))
                && !t.Project.IsArchived
                && t.Status == TaskFlow.API.Models.TaskStatus.Completed)
            .CountAsync(t => t.UpdatedAt >= startOfWeek && t.UpdatedAt < endOfWeek);
        var total = await _context.Tasks
            .Where(t => (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId))
                && !t.Project.IsArchived)
            .CountAsync(t => t.CreatedAt < endOfWeek);
        return new PeriodStatsDto { Completed = completed, Total = total, PeriodLabel = "This week" };
    }

    public async Task<PeriodStatsDto> GetMonthlyStatsAsync(Guid userId)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1);
        var completed = await _context.Tasks
            .Where(t => (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId))
                && !t.Project.IsArchived
                && t.Status == TaskFlow.API.Models.TaskStatus.Completed)
            .CountAsync(t => t.UpdatedAt >= startOfMonth && t.UpdatedAt < endOfMonth);
        var total = await _context.Tasks
            .Where(t => (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId))
                && !t.Project.IsArchived)
            .CountAsync(t => t.CreatedAt < endOfMonth);
        return new PeriodStatsDto { Completed = completed, Total = total, PeriodLabel = "This month" };
    }

    public async Task<StreakDto> GetStreakAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        var streak = 0;
        for (var d = today; ; d = d.AddDays(-1))
        {
            var next = d.AddDays(1);
            var completedToday = await _context.Tasks
                .Where(t => (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId))
                    && !t.Project.IsArchived
                    && t.Status == TaskFlow.API.Models.TaskStatus.Completed)
                .CountAsync(t => t.UpdatedAt >= d && t.UpdatedAt < next);
            if (completedToday == 0) break;
            streak++;
        }
        var msg = streak == 0 ? "Complete a task today to start your streak!" : $"X days in a row completing tasks".Replace("X", streak.ToString());
        return new StreakDto { CurrentStreakDays = streak, Message = msg };
    }

    public async Task<GoalProgressDto?> GetGoalProgressAsync(Guid userId, int targetTasksPerDay)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var completedToday = await _context.Tasks
            .Where(t => (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId))
                && !t.Project.IsArchived
                && t.Status == TaskFlow.API.Models.TaskStatus.Completed)
            .CountAsync(t => t.UpdatedAt >= today && t.UpdatedAt < tomorrow);
        var goalMet = completedToday >= targetTasksPerDay;
        var msg = goalMet
            ? $"Goal met! {completedToday}/{targetTasksPerDay} tasks today."
            : $"{completedToday}/{targetTasksPerDay} tasks completed today.";
        return new GoalProgressDto { TargetPerDay = targetTasksPerDay, CompletedToday = completedToday, GoalMet = goalMet, Message = msg };
    }
}
