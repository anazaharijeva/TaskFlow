using System.ComponentModel.DataAnnotations;

namespace TaskFlow.API.DTOs;

/// <summary>
/// DTO for creating a new task.
/// </summary>
public record CreateTaskDto
{
    [Required, MinLength(1)]
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public DateTime? DueDate { get; init; }

    public Guid ProjectId { get; init; }
    public Models.TaskPriority Priority { get; init; } = Models.TaskPriority.Medium;
    public string Tags { get; init; } = string.Empty;
    public Guid? AssigneeId { get; init; }
}

/// <summary>
/// DTO for updating an existing task.
/// </summary>
public record UpdateTaskDto
{
    [Required, MinLength(1)]
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public DateTime? DueDate { get; init; }

    public Models.TaskStatus Status { get; init; }
    public Models.TaskPriority Priority { get; init; }
    public string Tags { get; init; } = string.Empty;
    public Guid? AssigneeId { get; init; }
    /// <summary>Optional feedback when marking task as Completed</summary>
    public string? CompletionNote { get; init; }
}

/// <summary>
/// DTO for returning task data to the client.
/// </summary>
public record TaskResponseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? DueDate { get; init; }
    public Models.TaskStatus Status { get; init; }
    public Models.TaskPriority Priority { get; init; }
    public string Tags { get; init; } = string.Empty;
    public Guid? AssigneeId { get; init; }
    public string? AssigneeName { get; init; }
    public Guid ProjectId { get; init; }
    public string? CompletionNote { get; init; }
    public Guid? StartedById { get; init; }
    public string? StartedByName { get; init; }
    public DateTime? StartedAt { get; init; }
    public Guid? CompletedById { get; init; }
    public string? CompletedByName { get; init; }
    public DateTime? CompletedAt { get; init; }
}

/// <summary>
/// DTO for importing tasks from JSON or CSV content.
/// </summary>
public record ImportTasksDto
{
    public string Format { get; init; } = "json"; // json | csv
    public string Content { get; init; } = string.Empty;
}

public record ImportTaskItem
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public string? Priority { get; init; }
    public string? Tags { get; init; }
}
