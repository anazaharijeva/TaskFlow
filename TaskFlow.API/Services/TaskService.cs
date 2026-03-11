using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TaskFlow.API.Data;
using TaskFlow.API.DTOs;
using TaskFlow.API.Models;

namespace TaskFlow.API.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ITaskNotificationService _notifications;

    public TaskService(AppDbContext context, IDistributedCache cache, ITaskNotificationService notifications)
    {
        _context = context;
        _cache = cache;
        _notifications = notifications;
    }

    private Task InvalidateDashboardCacheAsync(Guid userId) =>
        _cache.RemoveAsync($"dashboard:{userId}");

    public async Task<IEnumerable<TaskResponseDto>> GetTasksByProjectAsync(Guid projectId, Guid userId, TaskFilterDto? filter = null)
    {
        var query = _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.StartedBy)
            .Include(t => t.CompletedBy)
            .Where(t => t.ProjectId == projectId &&
                (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId)));

        filter ??= new TaskFilterDto();

        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);
        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority.Value);
        if (!string.IsNullOrWhiteSpace(filter.Tag))
            query = query.Where(t => t.Tags.Contains(filter.Tag!));

        query = filter.SortBy switch
        {
            "DueDate" => filter.SortDesc ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "Priority" => filter.SortDesc ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "Title" => filter.SortDesc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            _ => filter.SortDesc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        return await query.Select(t => new TaskResponseDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            CreatedAt = t.CreatedAt,
            DueDate = t.DueDate,
            Status = t.Status,
            Priority = t.Priority,
            Tags = t.Tags,
            AssigneeId = t.AssigneeId,
            AssigneeName = t.Assignee != null ? t.Assignee.Name : null,
            ProjectId = t.ProjectId,
            CompletionNote = t.CompletionNote,
            StartedById = t.StartedById,
            StartedByName = t.StartedBy != null ? t.StartedBy.Name : null,
            StartedAt = t.StartedAt,
            CompletedById = t.CompletedById,
            CompletedByName = t.CompletedBy != null ? t.CompletedBy.Name : null,
            CompletedAt = t.CompletedAt
        }).ToListAsync();
    }

    public async Task<TaskResponseDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.StartedBy)
            .Include(t => t.CompletedBy)
            .FirstOrDefaultAsync(t => t.Id == id &&
                (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId)));

        return task == null ? null : MapToDto(task);
    }

    public async Task<TaskResponseDto?> CreateAsync(CreateTaskDto dto, Guid userId)
    {
        var project = await _context.Projects.Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == dto.ProjectId && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));
        if (project == null) return null;

        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            CreatedAt = now,
            UpdatedAt = now,
            DueDate = dto.DueDate,
            Status = Models.TaskStatus.Todo,
            Priority = dto.Priority,
            Tags = dto.Tags ?? string.Empty,
            AssigneeId = dto.AssigneeId,
            ProjectId = dto.ProjectId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        await InvalidateDashboardCacheAsync(userId);

        task = await _context.Tasks.Include(t => t.Assignee).FirstAsync(t => t.Id == task.Id);
        return MapToDto(task);
    }

    public async Task<TaskResponseDto?> UpdateAsync(Guid id, UpdateTaskDto dto, Guid userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id &&
                (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId)));
        if (task == null) return null;

        var oldStatus = task.Status;
        task.Title = dto.Title;
        task.Description = dto.Description;
        task.DueDate = dto.DueDate;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.Tags = dto.Tags ?? string.Empty;
        task.AssigneeId = dto.AssigneeId;
        if (!string.IsNullOrWhiteSpace(dto.CompletionNote))
            task.CompletionNote = dto.CompletionNote;
        var now = DateTime.UtcNow;
        task.UpdatedAt = now;

        if (dto.Status == Models.TaskStatus.InProgress && oldStatus != Models.TaskStatus.InProgress && task.StartedById == null)
        {
            task.StartedById = userId;
            task.StartedAt = now;
        }
        if (dto.Status == Models.TaskStatus.Completed)
        {
            task.CompletedById = userId;
            task.CompletedAt = now;
        }
        await _context.SaveChangesAsync();
        await InvalidateDashboardCacheAsync(userId);

        await _context.Entry(task).Reference(t => t.Assignee).LoadAsync();
        await _context.Entry(task).Reference(t => t.StartedBy).LoadAsync();
        await _context.Entry(task).Reference(t => t.CompletedBy).LoadAsync();
        var result = MapToDto(task);

        var user = await _context.Users.FindAsync(userId);
        var changeType = dto.Status.ToString();
        await _notifications.NotifyTaskUpdatedAsync(task.ProjectId, task.Id, task.Title, user?.Name ?? "Someone", changeType, result);

        return result;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id &&
                (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId)));
        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        await InvalidateDashboardCacheAsync(userId);
        return true;
    }

    public async Task<IEnumerable<TaskCommentDto>> GetCommentsAsync(Guid taskId, Guid userId)
    {
        var task = await _context.Tasks
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.Id == taskId &&
                (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId)));
        if (task == null) return Array.Empty<TaskCommentDto>();
        return task.Comments.OrderBy(c => c.CreatedAt).Select(c => new TaskCommentDto
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UserId = c.UserId,
            UserName = c.User.Name
        }).ToList();
    }

    public async Task<TaskCommentDto?> AddCommentAsync(Guid taskId, CreateTaskCommentDto dto, Guid userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId &&
                (t.Project.OwnerId == userId || t.Project.Members.Any(m => m.UserId == userId)));
        if (task == null) return null;
        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            TaskId = taskId,
            UserId = userId
        };
        _context.TaskComments.Add(comment);
        await _context.SaveChangesAsync();
        var user = await _context.Users.FindAsync(userId);
        return new TaskCommentDto { Id = comment.Id, Content = comment.Content, CreatedAt = comment.CreatedAt, UserId = userId, UserName = user?.Name ?? "" };
    }

    public async Task<bool> DeleteCommentAsync(Guid taskId, Guid commentId, Guid userId)
    {
        var comment = await _context.TaskComments
            .Include(c => c.Task!).ThenInclude(t => t.Project!).ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == taskId);
        if (comment == null) return false;
        var hasAccess = comment.Task.Project.OwnerId == userId || comment.Task.Project.Members.Any(m => m.UserId == userId);
        if (!hasAccess) return false;
        if (comment.UserId != userId && comment.Task.Project.OwnerId != userId) return false; // only author or owner can delete
        _context.TaskComments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> ImportTasksAsync(Guid projectId, Guid userId, ImportTasksDto dto)
    {
        var project = await _context.Projects.Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));
        if (project == null) return -1;

        var tasksToCreate = new List<CreateTaskDto>();
        if (dto.Format.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var type = typeof(List<DTOs.ImportTaskItem>);
                var items = (List<DTOs.ImportTaskItem>?)System.Text.Json.JsonSerializer.Deserialize(dto.Content, type);
                if (items == null) return -1;
                foreach (var item in items)
                {
                    if (string.IsNullOrWhiteSpace(item.Title)) continue;
                    tasksToCreate.Add(new CreateTaskDto
                    {
                        Title = item.Title,
                        Description = item.Description ?? "",
                        DueDate = item.DueDate,
                        ProjectId = projectId,
                        Priority = ParsePriority(item.Priority),
                        Tags = item.Tags ?? "",
                        AssigneeId = null
                    });
                }
            }
            catch { return -1; }
        }
        else if (dto.Format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            var lines = dto.Content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) return 0;
            var headers = ParseCsvLine(lines[0]);
            var titleIdx = Array.IndexOf(headers, "Title");
            if (titleIdx < 0) titleIdx = 0;
            var descIdx = Array.IndexOf(headers, "Description");
            var statusIdx = Array.IndexOf(headers, "Status");
            var priorityIdx = Array.IndexOf(headers, "Priority");
            var tagsIdx = Array.IndexOf(headers, "Tags");
            var dueIdx = Array.IndexOf(headers, "DueDate");
            for (var i = 1; i < lines.Length; i++)
            {
                var cols = ParseCsvLine(lines[i]);
                if (cols.Length <= titleIdx || string.IsNullOrWhiteSpace(cols[titleIdx])) continue;
                var title = cols[titleIdx];
                var desc = descIdx >= 0 && descIdx < cols.Length ? cols[descIdx] : "";
                var priority = priorityIdx >= 0 && priorityIdx < cols.Length ? ParsePriority(cols[priorityIdx]) : Models.TaskPriority.Medium;
                var tags = tagsIdx >= 0 && tagsIdx < cols.Length ? cols[tagsIdx] : "";
                DateTime? due = null;
                if (dueIdx >= 0 && dueIdx < cols.Length && DateTime.TryParse(cols[dueIdx], out var d))
                    due = d;
                tasksToCreate.Add(new CreateTaskDto { Title = title, Description = desc, DueDate = due, ProjectId = projectId, Priority = priority, Tags = tags });
            }
        }
        else return -1;

        var count = 0;
        foreach (var create in tasksToCreate)
        {
            var task = await CreateAsync(create, userId);
            if (task != null) count++;
        }
        return count;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var current = "";
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') { current += '"'; i++; }
                else inQuotes = !inQuotes;
            }
            else if ((c == ',' && !inQuotes) || c == '\r')
            {
                result.Add(current);
                current = "";
            }
            else if (c != '\n') current += c;
        }
        result.Add(current);
        return result.ToArray();
    }

    private static Models.TaskPriority ParsePriority(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return Models.TaskPriority.Medium;
        if (int.TryParse(s, out var n) && n >= 0 && n <= 3) return (Models.TaskPriority)n;
        return s.ToLowerInvariant() switch { "low" => Models.TaskPriority.Low, "high" => Models.TaskPriority.High, "urgent" => Models.TaskPriority.Urgent, _ => Models.TaskPriority.Medium };
    }

    private static TaskResponseDto MapToDto(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        CreatedAt = t.CreatedAt,
        DueDate = t.DueDate,
        Status = t.Status,
        Priority = t.Priority,
        Tags = t.Tags,
        AssigneeId = t.AssigneeId,
        AssigneeName = t.Assignee?.Name,
        ProjectId = t.ProjectId,
        CompletionNote = t.CompletionNote,
        StartedById = t.StartedById,
        StartedByName = t.StartedBy?.Name,
        StartedAt = t.StartedAt,
        CompletedById = t.CompletedById,
        CompletedByName = t.CompletedBy?.Name,
        CompletedAt = t.CompletedAt
    };
}
