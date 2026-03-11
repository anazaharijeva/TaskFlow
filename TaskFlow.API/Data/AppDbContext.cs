using Microsoft.EntityFrameworkCore;
using TaskFlow.API.Models;

namespace TaskFlow.API.Data;

/// <summary>
/// Entity Framework Core DbContext - the main database access layer.
/// Configures entity mappings and provides DbSets for each entity.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>Users table - stores registered users</summary>
    public DbSet<User> Users { get; set; }

    /// <summary>Projects table - stores project metadata</summary>
    public DbSet<Project> Projects { get; set; }

    /// <summary>Tasks table - stores task items (named Tasks to avoid conflict with System.Threading.Tasks)</summary>
    public DbSet<TaskItem> Tasks { get; set; }

    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<TaskComment> TaskComments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User: Email must be unique for login
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Project: One-to-many relationship with User
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.Projects)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade); // Delete projects when user is deleted

        // TaskItem: One-to-many relationship with Project
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // TaskItem: Optional assignee
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        // TaskItem: Who started (moved to In Progress) and who completed
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.StartedBy)
            .WithMany()
            .HasForeignKey(t => t.StartedById)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.CompletedBy)
            .WithMany()
            .HasForeignKey(t => t.CompletedById)
            .OnDelete(DeleteBehavior.SetNull);

        // ProjectMember: unique user per project
        modelBuilder.Entity<ProjectMember>()
            .HasIndex(pm => new { pm.ProjectId, pm.UserId })
            .IsUnique();
        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.User)
            .WithMany(u => u.ProjectMemberships)
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // TaskComment
        modelBuilder.Entity<TaskComment>()
            .HasOne(c => c.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TaskComment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
