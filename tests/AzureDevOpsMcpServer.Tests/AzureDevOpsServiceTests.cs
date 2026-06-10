using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

public class AzureDevOpsServiceTests
{
    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        
        var dbContext = new AppDbContext(options);
        dbContext.Database.OpenConnection();
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    [Fact]
    public async Task GetAssignedTasks_WithTasks_ReturnsFilteredTasks()
    {
        using var dbContext = CreateInMemoryDbContext();
        
        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Id = Guid.NewGuid(),
                AzureDevOpsId = "task-1",
                Title = "Task 1",
                AssignedTo = "user@test.com",
                ProjectId = "proj-1"
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                AzureDevOpsId = "task-2",
                Title = "Task 2",
                AssignedTo = "user@test.com",
                ProjectId = "proj-2"
            },
            new TaskItem
            {
                Id = Guid.NewGuid(),
                AzureDevOpsId = "task-3",
                Title = "Task 3",
                AssignedTo = "other@test.com",
                ProjectId = "proj-1"
            }
        };
        await dbContext.Tasks.AddRangeAsync(tasks);
        await dbContext.SaveChangesAsync();

        var service = new AzureDevOpsService(dbContext);
        var result = await service.GetAssignedTasksAsync("user@test.com", "proj-1");

        Assert.Single(result);
        Assert.Equal("task-1", result.First().AzureDevOpsId);
    }

    [Fact]
    public async Task UpdateTaskStatus_ExistingTask_UpdatesStatus()
    {
        using var dbContext = CreateInMemoryDbContext();
        
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            AzureDevOpsId = "task-1",
            Title = "Test Task",
            Status = Models.TaskStatus.NotImplemented
        };
        await dbContext.Tasks.AddAsync(task);
        await dbContext.SaveChangesAsync();

        var service = new AzureDevOpsService(dbContext);
        var result = await service.UpdateTaskStatusAsync(task.Id.ToString(), Models.TaskStatus.Current);

        Assert.NotNull(result);
        Assert.Equal(Models.TaskStatus.Current, result.Status);
    }

    [Fact]
    public async Task UpdateTaskStatus_Archived_SetsCompletedAt()
    {
        using var dbContext = CreateInMemoryDbContext();
        
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            AzureDevOpsId = "task-1",
            Title = "Test Task",
            Status = Models.TaskStatus.Current,
            CompletedAt = null
        };
        await dbContext.Tasks.AddAsync(task);
        await dbContext.SaveChangesAsync();

        var service = new AzureDevOpsService(dbContext);
        var result = await service.UpdateTaskStatusAsync(task.Id.ToString(), Models.TaskStatus.Archived);

        Assert.NotNull(result);
        Assert.Equal(Models.TaskStatus.Archived, result.Status);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task UpdateTaskStatus_NonExistingTask_ReturnsNull()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new AzureDevOpsService(dbContext);

        var result = await service.UpdateTaskStatusAsync(Guid.NewGuid().ToString(), Models.TaskStatus.Current);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTaskDetails_ExistingTask_ReturnsTask()
    {
        using var dbContext = CreateInMemoryDbContext();
        
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            AzureDevOpsId = "task-1",
            Title = "Test Task",
            Description = "Test Description"
        };
        await dbContext.Tasks.AddAsync(task);
        await dbContext.SaveChangesAsync();

        var service = new AzureDevOpsService(dbContext);
        var result = await service.GetTaskDetailsAsync(task.Id.ToString());

        Assert.NotNull(result);
        Assert.Equal("task-1", result.AzureDevOpsId);
        Assert.Equal("Test Task", result.Title);
    }

    [Fact]
    public async Task GetProjects_ReturnsAllProjects()
    {
        using var dbContext = CreateInMemoryDbContext();
        
        var projects = new List<Project>
        {
            new Project { Id = Guid.NewGuid(), AzureDevOpsId = "proj-1", Name = "Project 1" },
            new Project { Id = Guid.NewGuid(), AzureDevOpsId = "proj-2", Name = "Project 2" }
        };
        await dbContext.Projects.AddRangeAsync(projects);
        await dbContext.SaveChangesAsync();

        var service = new AzureDevOpsService(dbContext);
        var result = await service.GetProjectsAsync("user@test.com");

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetTaskHistory_ExistingTask_ReturnsHistory()
    {
        using var dbContext = CreateInMemoryDbContext();
        
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            AzureDevOpsId = "task-1",
            Title = "Test Task",
            Status = Models.TaskStatus.Current,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };
        await dbContext.Tasks.AddAsync(task);
        await dbContext.SaveChangesAsync();

        var service = new AzureDevOpsService(dbContext);
        var result = await service.GetTaskHistoryAsync(task.Id.ToString());

        Assert.NotNull(result);
        Assert.Single(result);
    }
}