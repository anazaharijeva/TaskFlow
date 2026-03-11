using System.ComponentModel.DataAnnotations;

namespace TaskFlow.API.DTOs;

public record CreateTaskCommentDto
{
    [Required, MinLength(1)]
    public string Content { get; init; } = string.Empty;
}

public record TaskCommentDto
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
}
