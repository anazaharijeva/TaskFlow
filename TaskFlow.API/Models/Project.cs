namespace TaskFlow.API.Models;

/// <summary>
/// Project entity - a container for related tasks.
/// Each project belongs to one user (Owner) and contains many tasks.
/// </summary>
public class Project
{
    /// <summary>Unique identifier (GUID) for the project</summary>
    public Guid Id { get; set; }

    /// <summary>Project display name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional detailed description of the project</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Foreign key - ID of the user who owns this project</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Navigation property - the user who owns this project</summary>
    public User Owner { get; set; } = null!;

    /// <summary>Navigation property - all tasks in this project</summary>
    public List<TaskItem> Tasks { get; set; } = new();

    /// <summary>Members with access to this project</summary>
    public List<ProjectMember> Members { get; set; } = new();

    /// <summary>Archived projects are hidden from main list</summary>
    public bool IsArchived { get; set; }
}
