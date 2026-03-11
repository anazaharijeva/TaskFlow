namespace TaskFlow.API.Services;

/// <summary>
/// Analytics service - provides productivity metrics for the dashboard.
/// </summary>
public interface IAnalyticsService
{
    Task<DashboardMetricsDto> GetDashboardMetricsAsync(Guid userId);
    Task<PeriodStatsDto> GetWeeklyStatsAsync(Guid userId);
    Task<PeriodStatsDto> GetMonthlyStatsAsync(Guid userId);
    Task<StreakDto> GetStreakAsync(Guid userId);
    Task<GoalProgressDto?> GetGoalProgressAsync(Guid userId, int targetTasksPerDay);
}

/// <summary>
/// DTO for dashboard analytics - used by both API and caching.
/// </summary>
public record DashboardMetricsDto
{
    public int TotalProjects { get; init; }
    public int TasksCompleted { get; init; }
    public int TasksInProgress { get; init; }
    public int TasksTodo { get; init; }
    public string ProductivityScore { get; init; } = string.Empty;
    public List<ProjectTaskStatsDto> ProjectStats { get; init; } = new();
}

/// <summary>
/// Task counts per project for chart - which project has how many Todo/InProgress/Completed.
/// </summary>
public record ProjectTaskStatsDto
{
    public string ProjectName { get; init; } = string.Empty;
    public int Todo { get; init; }
    public int InProgress { get; init; }
    public int Completed { get; init; }
}

public record PeriodStatsDto
{
    public int Completed { get; init; }
    public int Total { get; init; }
    public string PeriodLabel { get; init; } = string.Empty;
}

public record StreakDto
{
    public int CurrentStreakDays { get; init; }
    public string Message { get; init; } = string.Empty;
}

public record GoalProgressDto
{
    public int TargetPerDay { get; init; }
    public int CompletedToday { get; init; }
    public bool GoalMet { get; init; }
    public string Message { get; init; } = string.Empty;
}
