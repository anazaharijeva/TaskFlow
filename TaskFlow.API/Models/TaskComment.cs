namespace TaskFlow.API.Models;

/// <summary>
/// Comment on a task.
/// </summary>
public class TaskComment
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }

    public TaskItem Task { get; set; } = null!;
    public User User { get; set; } = null!;
}
