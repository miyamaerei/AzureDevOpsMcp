using AzureDevOpsMcpServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureDevOpsMcpServer.Services;

public interface IAzureDevOpsService
{
    Task<IEnumerable<TaskItem>> GetAssignedTasksAsync(string userId, string projectId);
    Task<TaskItem?> UpdateTaskStatusAsync(string taskId, Models.TaskStatus status);
    Task<TaskItem?> GetTaskDetailsAsync(string taskId);
    Task<IEnumerable<Project>> GetProjectsAsync(string userId);
    Task<IEnumerable<TaskHistory>> GetTaskHistoryAsync(string taskId);
}

public class TaskHistory
{
    public DateTime ChangedAt { get; set; }
    public Models.TaskStatus OldStatus { get; set; }
    public Models.TaskStatus NewStatus { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}

public class AzureDevOpsService : IAzureDevOpsService
{
    private readonly AppDbContext _dbContext;

    public AzureDevOpsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<TaskItem>> GetAssignedTasksAsync(string userId, string projectId)
    {
        return await _dbContext.Tasks
            .Where(t => t.AssignedTo == userId && t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<TaskItem?> UpdateTaskStatusAsync(string taskId, Models.TaskStatus status)
    {
        var task = await _dbContext.Tasks.FindAsync(Guid.Parse(taskId));
        if (task == null) return null;

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        
        if (status == Models.TaskStatus.Archived && task.CompletedAt == null)
        {
            task.CompletedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem?> GetTaskDetailsAsync(string taskId)
    {
        return await _dbContext.Tasks.FindAsync(Guid.Parse(taskId));
    }

    public async Task<IEnumerable<Project>> GetProjectsAsync(string userId)
    {
        return await _dbContext.Projects.ToListAsync();
    }

    public async Task<IEnumerable<TaskHistory>> GetTaskHistoryAsync(string taskId)
    {
        var task = await _dbContext.Tasks.FindAsync(Guid.Parse(taskId));
        if (task == null) return Enumerable.Empty<TaskHistory>();

        return new List<TaskHistory>
        {
            new TaskHistory
            {
                ChangedAt = task.CreatedAt,
                OldStatus = Models.TaskStatus.NotImplemented,
                NewStatus = task.Status,
                ChangedBy = "System"
            }
        };
    }

    public async Task InitializeTestDataAsync()
    {
        if (await _dbContext.Projects.AnyAsync()) return;

        var projects = new List<Project>
        {
            new Project
            {
                Id = Guid.NewGuid(),
                AzureDevOpsId = "proj-1",
                Name = "Sample Project",
                Description = "A sample project for testing",
                Organization = "TestOrg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        _dbContext.Projects.AddRange(projects);

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                AzureDevOpsId = "task-1",
                Title = "Implement Feature A",
                Description = "Implement the first feature",
                AssignedTo = "user@example.com",
                ProjectId = "proj-1",
                ProjectName = "Sample Project",
                Status = Models.TaskStatus.NotImplemented,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                AzureDevOpsId = "task-2",
                Title = "Fix Bug B",
                Description = "Fix the bug in module B",
                AssignedTo = "user@example.com",
                ProjectId = "proj-1",
                ProjectName = "Sample Project",
                Status = Models.TaskStatus.Current,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        _dbContext.Tasks.AddRange(tasks);

        await _dbContext.SaveChangesAsync();
    }
}