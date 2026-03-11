namespace TaskFlow.API.Models;

/// <summary>
/// Project sharing - users who have access to a project.
/// </summary>
public class ProjectMember
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member"; // Owner, Member, Viewer

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}
