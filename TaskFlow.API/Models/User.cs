namespace TaskFlow.API.Models;

/// <summary>
/// User entity - represents a registered user in the system.
/// Users own projects and can manage tasks within those projects.
/// </summary>
public class User
{
    /// <summary>Unique identifier (GUID) for the user</summary>
    public Guid Id { get; set; }

    /// <summary>User's email - used for login and must be unique</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Hashed password - never store plain text passwords!</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Display name of the user</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Navigation property - projects owned by this user</summary>
    public List<Project> Projects { get; set; } = new();

    /// <summary>Projects shared with this user</summary>
    public List<ProjectMember> ProjectMemberships { get; set; } = new();
}
