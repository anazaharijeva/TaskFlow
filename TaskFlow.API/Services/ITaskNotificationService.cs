namespace TaskFlow.API.Services;

/// <summary>
/// Sends real-time notifications when tasks change.
/// </summary>
public interface ITaskNotificationService
{
    Task NotifyTaskUpdatedAsync(Guid projectId, Guid taskId, string taskTitle, string changedByUserName, string changeType, object? taskData = null);
}
