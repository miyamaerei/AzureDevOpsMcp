using AzureDevOpsMcpServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureDevOpsMcpServer.Services;

public class AppDbContext : DbContext
{
    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectMapping> ProjectMappings { get; set; } = null!;
    public DbSet<RepositoryMapping> RepositoryMappings { get; set; } = null!;
    public DbSet<TaskStateHistory> TaskHistories { get; set; } = null!;
    public DbSet<UserMapping> UserMappings { get; set; } = null!;
    public DbSet<TaskSyncRecord> TaskSyncRecords { get; set; } = null!;

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectMapping>()
            .HasIndex(pm => pm.LocalProjectName)
            .IsUnique();
        
        modelBuilder.Entity<ProjectMapping>()
            .HasIndex(pm => pm.AzureDevOpsProjectId);

        modelBuilder.Entity<RepositoryMapping>()
            .HasIndex(rm => new { rm.WindowsUsername, rm.LocalProjectName })
            .IsUnique();

        modelBuilder.Entity<RepositoryMapping>()
            .HasIndex(rm => new { rm.WindowsUsername, rm.WorkingDirectory });

        modelBuilder.Entity<RepositoryMapping>()
            .HasIndex(rm => new { rm.WindowsUsername, rm.RepositoryId });
        
        modelBuilder.Entity<UserMapping>()
            .HasIndex(um => um.WindowsUsername)
            .IsUnique();
    }
}