using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TaskFlow.API.Data;
using TaskFlow.API.DTOs;
using TaskFlow.API.Models;

namespace TaskFlow.API.Services;

/// <summary>
/// Project service implementation.
/// Invalidates dashboard cache when projects change.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache;

    public ProjectService(AppDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private Task InvalidateDashboardCacheAsync(Guid userId) =>
        _cache.RemoveAsync($"dashboard:{userId}");

    public async Task<IEnumerable<ProjectResponseDto>> GetProjectsByUserAsync(Guid userId, bool includeArchived = false)
    {
        var query = _context.Projects
            .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId));
        if (!includeArchived)
            query = query.Where(p => !p.IsArchived);
        return await query.Select(p => new ProjectResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            OwnerId = p.OwnerId,
            TaskCount = p.Tasks.Count,
            IsArchived = p.IsArchived
        }).ToListAsync();
    }

    public async Task<ProjectResponseDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var project = await _context.Projects
            .Where(p => p.Id == id && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)))
            .Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                OwnerId = p.OwnerId,
                TaskCount = p.Tasks.Count,
                IsArchived = p.IsArchived
            })
            .FirstOrDefaultAsync();
        return project;
    }

    public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, Guid userId)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = userId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        await InvalidateDashboardCacheAsync(userId);

        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            TaskCount = 0,
            IsArchived = false
        };
    }

    public async Task<ProjectResponseDto?> UpdateAsync(Guid id, UpdateProjectDto dto, Guid userId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (project == null)
            return null;

        project.Name = dto.Name;
        project.Description = dto.Description;
        if (dto.IsArchived.HasValue)
            project.IsArchived = dto.IsArchived.Value;
        await _context.SaveChangesAsync();
        await InvalidateDashboardCacheAsync(userId);

        return new ProjectResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            TaskCount = project.Tasks.Count,
            IsArchived = project.IsArchived
        };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (project == null)
            return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        await InvalidateDashboardCacheAsync(userId);
        return true;
    }

    public async Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(Guid projectId, Guid userId)
    {
        var project = await _context.Projects
            .Include(p => p.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == projectId && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));
        if (project == null) return Array.Empty<ProjectMemberDto>();
        return project.Members.Select(m => new ProjectMemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            UserName = m.User.Name,
            UserEmail = m.User.Email,
            Role = m.Role
        }).ToList();
    }

    public async Task<IEnumerable<ProjectMemberDto>> GetAssignableUsersAsync(Guid projectId, Guid userId)
    {
        var project = await _context.Projects
            .Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == projectId && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));
        if (project == null) return Array.Empty<ProjectMemberDto>();
        var list = new List<ProjectMemberDto>();
        list.Add(new ProjectMemberDto { Id = Guid.Empty, UserId = project.OwnerId, UserName = project.Owner.Name, UserEmail = project.Owner.Email, Role = "Owner" });
        foreach (var m in project.Members)
            list.Add(new ProjectMemberDto { Id = m.Id, UserId = m.UserId, UserName = m.User.Name, UserEmail = m.User.Email, Role = m.Role });
        return list;
    }

    public async Task<bool> AddMemberAsync(Guid projectId, AddProjectMemberDto dto, Guid userId)
    {
        var project = await _context.Projects.Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);
        if (project == null) return false;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return false;
        if (project.OwnerId == user.Id) return false; // owner is implicit
        if (project.Members.Any(m => m.UserId == user.Id)) return false; // already member
        _context.ProjectMembers.Add(new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = user.Id,
            Role = dto.Role
        });
        await _context.SaveChangesAsync();
        await InvalidateDashboardCacheAsync(userId);
        return true;
    }

    public async Task<bool> RemoveMemberAsync(Guid projectId, Guid memberUserId, Guid userId)
    {
        var project = await _context.Projects.Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);
        if (project == null) return false;
        var member = project.Members.FirstOrDefault(m => m.UserId == memberUserId);
        if (member == null) return false;
        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync();
        await InvalidateDashboardCacheAsync(userId);
        return true;
    }
}
