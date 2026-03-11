using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TaskFlow.API.Services;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Analytics controller - provides dashboard metrics.
/// Uses Redis cache to improve performance for frequently accessed data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IDistributedCache _cache;

    public AnalyticsController(IAnalyticsService analyticsService, IDistributedCache cache)
    {
        _analyticsService = analyticsService;
        _cache = cache; // Optional - Redis may not be configured
    }

    /// <summary>
    /// GET /api/analytics/dashboard - Get productivity metrics for the dashboard.
    /// Cache key: dashboard:{userId} - cached for 5 minutes when Redis is available.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetUserId();
        var cacheKey = $"dashboard:{userId}";

        // Try cache first (Redis or in-memory fallback)
        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            var metrics = JsonSerializer.Deserialize<DashboardMetricsDto>(cached);
            if (metrics != null)
                return Ok(metrics);
        }

        var result = await _analyticsService.GetDashboardMetricsAsync(userId);

        // Store in cache for 5 minutes
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

        return Ok(result);
    }

    /// <summary>
    /// GET /api/analytics/weekly - Tasks completed this week.
    /// </summary>
    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyStats()
    {
        var userId = GetUserId();
        var result = await _analyticsService.GetWeeklyStatsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/analytics/monthly - Tasks completed this month.
    /// </summary>
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyStats()
    {
        var userId = GetUserId();
        var result = await _analyticsService.GetMonthlyStatsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/analytics/streak - Consecutive days completing tasks.
    /// </summary>
    [HttpGet("streak")]
    public async Task<IActionResult> GetStreak()
    {
        var userId = GetUserId();
        var result = await _analyticsService.GetStreakAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/analytics/goal - Progress toward daily goal (e.g. ?target=10).
    /// </summary>
    [HttpGet("goal")]
    public async Task<IActionResult> GetGoalProgress([FromQuery] int target = 10)
    {
        var userId = GetUserId();
        var result = await _analyticsService.GetGoalProgressAsync(userId, target);
        return Ok(result ?? new GoalProgressDto { TargetPerDay = target, CompletedToday = 0, GoalMet = false, Message = $"0/{target} tasks completed today." });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found"));
    }
}
