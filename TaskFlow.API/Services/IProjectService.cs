using TaskFlow.API.DTOs;
using TaskFlow.API.Models;

namespace TaskFlow.API.Services;

/// <summary>
/// Project service - business logic for project CRUD operations.
/// </summary>
public interface IProjectService
{
    Task<IEnumerable<ProjectResponseDto>> GetProjectsByUserAsync(Guid userId, bool includeArchived = false);
    Task<ProjectResponseDto?> GetByIdAsync(Guid id, Guid userId);
    Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, Guid userId);
    Task<ProjectResponseDto?> UpdateAsync(Guid id, UpdateProjectDto dto, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(Guid projectId, Guid userId);
    Task<IEnumerable<ProjectMemberDto>> GetAssignableUsersAsync(Guid projectId, Guid userId);
    Task<bool> AddMemberAsync(Guid projectId, AddProjectMemberDto dto, Guid userId);
    Task<bool> RemoveMemberAsync(Guid projectId, Guid memberUserId, Guid userId);
}
