namespace TaskFlow.API.Models;

/// <summary>
/// TaskItem entity - represents a single task within a project.
/// Tasks have status (Todo, InProgress, Completed) and optional due dates.
/// </summary>
public class TaskItem
{
    /// <summary>Unique identifier (GUID) for the task</summary>
    public Guid Id { get; set; }

    /// <summary>Short title describing the task</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional detailed description</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>When the task was created (UTC)</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the task was last updated (UTC) - for analytics (streak, weekly/monthly)</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Optional deadline for the task</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Current status in the workflow</summary>
    public TaskStatus Status { get; set; }

    /// <summary>Foreign key - ID of the project this task belongs to</summary>
    public Guid ProjectId { get; set; }

    /// <summary>Priority: Low, Medium, High, Urgent</summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>Tags: comma-separated e.g. "Work,Urgent"</summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>Assigned user (nullable)</summary>
    public Guid? AssigneeId { get; set; }
    public User? Assignee { get; set; }

    /// <summary>User who moved task to In Progress (nullable)</summary>
    public Guid? StartedById { get; set; }
    public DateTime? StartedAt { get; set; }
    public User? StartedBy { get; set; }

    /// <summary>User who completed the task (nullable)</summary>
    public Guid? CompletedById { get; set; }
    public DateTime? CompletedAt { get; set; }
    public User? CompletedBy { get; set; }

    /// <summary>Navigation property - the project containing this task</summary>
    public Project Project { get; set; } = null!;

    /// <summary>Comments on this task</summary>
    public List<TaskComment> Comments { get; set; } = new();

    /// <summary>Optional feedback/note when task was completed</summary>
    public string? CompletionNote { get; set; }
}
