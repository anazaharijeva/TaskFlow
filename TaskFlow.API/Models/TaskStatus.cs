namespace TaskFlow.API.Models;

/// <summary>
/// Represents the lifecycle state of a task in the system.
/// Tasks flow from Todo -> InProgress -> Completed.
/// </summary>
public enum TaskStatus
{
    /// <summary>Task has been created but not yet started</summary>
    Todo,

    /// <summary>Task is currently being worked on</summary>
    InProgress,

    /// <summary>Task has been finished</summary>
    Completed
}
