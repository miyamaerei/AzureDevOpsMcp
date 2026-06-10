using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

/// <summary>
/// 任务同步服务测试
/// </summary>
public class TaskSyncServiceTests
{
    private async Task<AppDbContext> CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        return dbContext;
    }

    [Fact]
    public async Task SyncTaskToAzureDevOpsAsync_TaskExists_SyncsSuccessfully()
    {
        // Arrange
        var dbContext = await CreateTestDbContext();
        var taskId = Guid.NewGuid();
        
        var task = new TaskItem
        {
            Id = taskId,
            AzureDevOpsId = "1",
            Title = "Test Task",
            Status = Models.TaskStatus.Archived
        };
        await dbContext.Tasks.AddAsync(task);
        await dbContext.SaveChangesAsync();

        var mockApiService = new Mock<IAzureDevOpsApiService>();
        mockApiService.Setup(x => x.UpdateWorkItemStateAsync(1, "Closed"))
            .ReturnsAsync(new TaskItem { Id = taskId, AzureDevOpsId = "1", Status = Models.TaskStatus.Archived });

        var syncService = new TaskSyncService(dbContext, mockApiService.Object);

        // Act
        bool result = await syncService.SyncTaskToAzureDevOpsAsync(taskId);

        // Assert
        Assert.True(result);
        
        var syncRecords = await dbContext.TaskSyncRecords.Where(r => r.TaskId == taskId).ToListAsync();
        Assert.Single(syncRecords);
        Assert.True(syncRecords[0].SyncSuccess);
        Assert.Equal("Closed", syncRecords[0].AzureDevOpsState);
    }

    [Fact]
    public async Task SyncTaskToAzureDevOpsAsync_TaskNotFound_ReturnsFalse()
    {
        // Arrange
        var dbContext = await CreateTestDbContext();
        var mockApiService = new Mock<IAzureDevOpsApiService>();
        var syncService = new TaskSyncService(dbContext, mockApiService.Object);

        // Act
        bool result = await syncService.SyncTaskToAzureDevOpsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SyncTaskToAzureDevOpsAsync_ApiFails_RecordsFailure()
    {
        // Arrange
        var dbContext = await CreateTestDbContext();
        var taskId = Guid.NewGuid();
        
        var task = new TaskItem
        {
            Id = taskId,
            AzureDevOpsId = "1",
            Title = "Test Task",
            Status = Models.TaskStatus.Archived
        };
        await dbContext.Tasks.AddAsync(task);
        await dbContext.SaveChangesAsync();

        var mockApiService = new Mock<IAzureDevOpsApiService>();
        mockApiService.Setup(x => x.UpdateWorkItemStateAsync(1, "Closed"))
            .ThrowsAsync(new InvalidOperationException("API Error"));

        var syncService = new TaskSyncService(dbContext, mockApiService.Object);

        // Act
        bool result = await syncService.SyncTaskToAzureDevOpsAsync(taskId);

        // Assert
        Assert.False(result);
        
        var syncRecords = await dbContext.TaskSyncRecords.Where(r => r.TaskId == taskId).ToListAsync();
        Assert.Single(syncRecords);
        Assert.False(syncRecords[0].SyncSuccess);
        Assert.Contains("API Error", syncRecords[0].ErrorMessage);
    }

    [Fact]
    public async Task GetSyncHistoryAsync_ReturnsRecordsInOrder()
    {
        // Arrange
        var dbContext = await CreateTestDbContext();
        var taskId = Guid.NewGuid();
        
        var records = new List<TaskSyncRecord>
        {
            new TaskSyncRecord { Id = Guid.NewGuid(), TaskId = taskId, SyncedAt = DateTime.UtcNow.AddHours(-2), SyncSuccess = true },
            new TaskSyncRecord { Id = Guid.NewGuid(), TaskId = taskId, SyncedAt = DateTime.UtcNow.AddHours(-1), SyncSuccess = false },
            new TaskSyncRecord { Id = Guid.NewGuid(), TaskId = taskId, SyncedAt = DateTime.UtcNow, SyncSuccess = true }
        };
        
        await dbContext.TaskSyncRecords.AddRangeAsync(records);
        await dbContext.SaveChangesAsync();

        var mockApiService = new Mock<IAzureDevOpsApiService>();
        var syncService = new TaskSyncService(dbContext, mockApiService.Object);

        // Act
        var result = await syncService.GetSyncHistoryAsync(taskId);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Equal(DateTime.UtcNow.Date, result.First().SyncedAt.Date);
    }

    [Fact]
    public async Task SyncAllPendingTasksAsync_SyncsArchivedTasks()
    {
        // Arrange
        var dbContext = await CreateTestDbContext();
        
        var task1 = new TaskItem { Id = Guid.NewGuid(), AzureDevOpsId = "1", Title = "Task 1", Status = Models.TaskStatus.Archived };
        var task2 = new TaskItem { Id = Guid.NewGuid(), AzureDevOpsId = "2", Title = "Task 2", Status = Models.TaskStatus.Current };
        var task3 = new TaskItem { Id = Guid.NewGuid(), AzureDevOpsId = "3", Title = "Task 3", Status = Models.TaskStatus.Archived };
        
        await dbContext.Tasks.AddRangeAsync(task1, task2, task3);
        await dbContext.SaveChangesAsync();

        var mockApiService = new Mock<IAzureDevOpsApiService>();
        mockApiService.Setup(x => x.UpdateWorkItemStateAsync(1, "Closed"))
            .ReturnsAsync(new TaskItem { Id = task1.Id, Status = Models.TaskStatus.Archived });
        mockApiService.Setup(x => x.UpdateWorkItemStateAsync(3, "Closed"))
            .ReturnsAsync(new TaskItem { Id = task3.Id, Status = Models.TaskStatus.Archived });

        var syncService = new TaskSyncService(dbContext, mockApiService.Object);

        // Act
        bool result = await syncService.SyncAllPendingTasksAsync();

        // Assert
        Assert.True(result);
        
        var syncRecords = await dbContext.TaskSyncRecords.ToListAsync();
        Assert.Equal(2, syncRecords.Count);
    }

    [Fact]
    public async Task SyncTaskByWorkItemIdAsync_WorkItemExists_SyncsSuccessfully()
    {
        // Arrange
        var dbContext = await CreateTestDbContext();
        
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            AzureDevOpsId = "123",
            Title = "Test Task",
            Status = Models.TaskStatus.Archived
        };
        await dbContext.Tasks.AddAsync(task);
        await dbContext.SaveChangesAsync();

        var mockApiService = new Mock<IAzureDevOpsApiService>();
        mockApiService.Setup(x => x.UpdateWorkItemStateAsync(123, "Closed"))
            .ReturnsAsync(new TaskItem { Id = task.Id, Status = Models.TaskStatus.Archived });

        var syncService = new TaskSyncService(dbContext, mockApiService.Object);

        // Act
        bool result = await syncService.SyncTaskByWorkItemIdAsync("123");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SyncTaskByWorkItemIdAsync_WorkItemNotFound_ReturnsFalse()
    {
        // Arrange
        var dbContext = await CreateTestDbContext();
        var mockApiService = new Mock<IAzureDevOpsApiService>();
        var syncService = new TaskSyncService(dbContext, mockApiService.Object);

        // Act
        bool result = await syncService.SyncTaskByWorkItemIdAsync("999");

        // Assert
        Assert.False(result);
    }
}