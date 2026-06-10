using AzureDevOpsMcpServer.Models;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

/// <summary>
/// TaskStateHistory 模型单元测试
/// </summary>
public class TaskHistoryModelTests
{
    [Fact]
    public void TaskStateHistory_NewInstance_HasDefaultValues()
    {
        // Arrange & Act
        var history = new TaskStateHistory();

        // Assert
        Assert.Equal(Guid.Empty, history.Id);
        Assert.Equal(string.Empty, history.WorkItemId);
        Assert.Equal(Models.TaskStatus.NotImplemented, history.OldStatus);
        Assert.Equal(Models.TaskStatus.NotImplemented, history.NewStatus);
        Assert.Equal(string.Empty, history.ChangedBy);
        Assert.Equal(default, history.ChangedAt);
    }

    [Fact]
    public void TaskStateHistory_SetProperties_ReturnsCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var workItemId = "12345";
        var oldStatus = Models.TaskStatus.Current;
        var newStatus = Models.TaskStatus.Archived;
        var changedBy = "user@company.com";
        var changedAt = DateTime.UtcNow;

        // Act
        var history = new TaskStateHistory
        {
            Id = id,
            WorkItemId = workItemId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedBy = changedBy,
            ChangedAt = changedAt
        };

        // Assert
        Assert.Equal(id, history.Id);
        Assert.Equal(workItemId, history.WorkItemId);
        Assert.Equal(oldStatus, history.OldStatus);
        Assert.Equal(newStatus, history.NewStatus);
        Assert.Equal(changedBy, history.ChangedBy);
        Assert.Equal(changedAt, history.ChangedAt);
    }
}
