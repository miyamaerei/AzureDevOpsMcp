using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using TaskStatus = AzureDevOpsMcpServer.Models.TaskStatus;

namespace AzureDevOpsMcpServer.Tests.IntegrationTests;

/// <summary>
/// 任务同步集成测试
/// </summary>
public class TaskSyncIntegrationTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IAzureDevOpsApiService> _apiServiceMock;

    public TaskSyncIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _apiServiceMock = new Mock<IAzureDevOpsApiService>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task TaskSyncService_SyncTaskToAzureDevOps_Integration()
    {
        // Arrange
        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            AzureDevOpsId = "12345",
            Title = "Test Task",
            Status = TaskStatus.Archived
        };

        await _dbContext.Tasks.AddAsync(taskItem);
        await _dbContext.SaveChangesAsync();

        var syncService = new TaskSyncService(_dbContext, _apiServiceMock.Object);

        _apiServiceMock.Setup(x => x.UpdateWorkItemStateAsync(12345, "Closed"))
            .ReturnsAsync(taskItem);

        // Act
        var result = await syncService.SyncTaskToAzureDevOpsAsync(taskItem.Id);

        // Assert
        Assert.True(result);
        _apiServiceMock.Verify(x => x.UpdateWorkItemStateAsync(12345, "Closed"), Times.Once);
    }

    [Fact]
    public async Task TaskSyncService_SyncTaskByWorkItemId_Integration()
    {
        // Arrange
        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            AzureDevOpsId = "67890",
            Title = "Test Task 2",
            Status = TaskStatus.Archived
        };

        await _dbContext.Tasks.AddAsync(taskItem);
        await _dbContext.SaveChangesAsync();

        var syncService = new TaskSyncService(_dbContext, _apiServiceMock.Object);

        _apiServiceMock.Setup(x => x.UpdateWorkItemStateAsync(67890, "Closed"))
            .ReturnsAsync(taskItem);

        // Act
        var result = await syncService.SyncTaskByWorkItemIdAsync("67890");

        // Assert
        Assert.True(result);
        _apiServiceMock.Verify(x => x.UpdateWorkItemStateAsync(67890, "Closed"), Times.Once);
    }

    [Fact]
    public async Task TaskSyncService_SyncAllPendingTasks_Integration()
    {
        // Arrange
        var task1 = new TaskItem { Id = Guid.NewGuid(), AzureDevOpsId = "task1", Status = TaskStatus.Archived };
        var task2 = new TaskItem { Id = Guid.NewGuid(), AzureDevOpsId = "task2", Status = TaskStatus.Archived };
        var task3 = new TaskItem { Id = Guid.NewGuid(), AzureDevOpsId = "task3", Status = TaskStatus.Current }; // Not archived

        await _dbContext.Tasks.AddRangeAsync(task1, task2, task3);
        await _dbContext.SaveChangesAsync();

        var syncService = new TaskSyncService(_dbContext, _apiServiceMock.Object);

        _apiServiceMock.Setup(x => x.UpdateWorkItemStateAsync(It.IsAny<int>(), "Closed"))
            .ReturnsAsync((int id, string state) => task1);

        // Act
        var result = await syncService.SyncAllPendingTasksAsync();

        // Assert
        Assert.True(result);
        _apiServiceMock.Verify(x => x.UpdateWorkItemStateAsync(It.IsAny<int>(), "Closed"), Times.Exactly(2));
    }

    [Fact]
    public async Task TaskSyncService_GetSyncHistory_Integration()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var syncRecords = new List<TaskSyncRecord>
        {
            new TaskSyncRecord { Id = Guid.NewGuid(), TaskId = taskId, WorkItemId = "123", SyncSuccess = true },
            new TaskSyncRecord { Id = Guid.NewGuid(), TaskId = taskId, WorkItemId = "456", SyncSuccess = false }
        };

        await _dbContext.TaskSyncRecords.AddRangeAsync(syncRecords);
        await _dbContext.SaveChangesAsync();

        var syncService = new TaskSyncService(_dbContext, _apiServiceMock.Object);

        // Act
        var result = await syncService.GetSyncHistoryAsync(taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task TaskSyncService_TaskNotFound_Integration()
    {
        // Arrange
        var syncService = new TaskSyncService(_dbContext, _apiServiceMock.Object);

        // Act
        var result = await syncService.SyncTaskToAzureDevOpsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
        _apiServiceMock.Verify(x => x.UpdateWorkItemStateAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }
}