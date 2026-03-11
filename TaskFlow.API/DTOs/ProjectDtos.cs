using System.ComponentModel.DataAnnotations;

namespace TaskFlow.API.DTOs;

/// <summary>
/// DTO for creating a new project.
/// </summary>
public record CreateProjectDto
{
    [Required, MinLength(1)]
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing project.
/// </summary>
public record UpdateProjectDto
{
    [Required, MinLength(1)]
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
    public bool? IsArchived { get; init; }
}

public record ProjectResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public int TaskCount { get; init; }
    public bool IsArchived { get; init; }
}

public record AddProjectMemberDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = "Member";
}

public record ProjectMemberDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
