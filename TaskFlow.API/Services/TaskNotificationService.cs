using Microsoft.AspNetCore.SignalR;
using TaskFlow.API.Hubs;

namespace TaskFlow.API.Services;

public class TaskNotificationService : ITaskNotificationService
{
    private readonly IHubContext<NotificationHub> _hub;

    public TaskNotificationService(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public async Task NotifyTaskUpdatedAsync(Guid projectId, Guid taskId, string taskTitle, string changedByUserName, string changeType, object? taskData = null)
    {
        var message = changeType switch
        {
            "Completed" => $"{changedByUserName} completed task \"{taskTitle}\"",
            "InProgress" => $"{changedByUserName} started task \"{taskTitle}\"",
            "Todo" => $"{changedByUserName} reset task \"{taskTitle}\" to Todo",
            _ => $"{changedByUserName} updated task \"{taskTitle}\""
        };
        await _hub.Clients.Group($"project_{projectId}").SendAsync("TaskUpdated", new
        {
            taskId,
            projectId,
            message,
            changeType,
            changedBy = changedByUserName,
            task = taskData
        });
    }
}
