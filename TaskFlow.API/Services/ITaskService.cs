using TaskFlow.API.DTOs;
using TaskFlow.API.Models;
using TaskStatus = TaskFlow.API.Models.TaskStatus;

namespace TaskFlow.API.Services;

/// <summary>
/// Task service - business logic for task CRUD operations.
/// </summary>
public interface ITaskService
{
    Task<IEnumerable<TaskResponseDto>> GetTasksByProjectAsync(Guid projectId, Guid userId, TaskFilterDto? filter = null);
    Task<TaskResponseDto?> GetByIdAsync(Guid id, Guid userId);
    Task<TaskResponseDto?> CreateAsync(CreateTaskDto dto, Guid userId);
    Task<TaskResponseDto?> UpdateAsync(Guid id, UpdateTaskDto dto, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<IEnumerable<TaskCommentDto>> GetCommentsAsync(Guid taskId, Guid userId);
    Task<TaskCommentDto?> AddCommentAsync(Guid taskId, CreateTaskCommentDto dto, Guid userId);
    Task<bool> DeleteCommentAsync(Guid taskId, Guid commentId, Guid userId);
    Task<int> ImportTasksAsync(Guid projectId, Guid userId, ImportTasksDto dto);
}

public record TaskFilterDto
{
    public Models.TaskStatus? Status { get; init; }
    public Models.TaskPriority? Priority { get; init; }
    public string? Tag { get; init; }
    public string SortBy { get; init; } = "CreatedAt"; // CreatedAt, DueDate, Priority, Title
    public bool SortDesc { get; init; } = true;
}
