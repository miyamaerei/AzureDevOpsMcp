using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Moq;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

/// <summary>
/// 状态同步到 Azure DevOps 测试
/// </summary>
public class WorkItemStateSyncTests
{
    [Theory]
    [InlineData("New", Models.TaskStatus.NotImplemented)]
    [InlineData("Active", Models.TaskStatus.Current)]
    [InlineData("Resolved", Models.TaskStatus.Blocked)]
    [InlineData("Closed", Models.TaskStatus.Archived)]
    public async Task UpdateWorkItemStateAsync_ValidState_UpdatesInAzureDevOps(string inputState, Models.TaskStatus expectedStatus)
    {
        // Arrange
        var mockApiService = new Mock<IAzureDevOpsApiService>();
        
        var updatedWorkItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            AzureDevOpsId = "1",
            Title = "Test Task",
            Status = expectedStatus
        };
        
        mockApiService.Setup(x => x.UpdateWorkItemStateAsync(1, inputState))
            .ReturnsAsync(updatedWorkItem);
        
        // Act
        var result = await mockApiService.Object.UpdateWorkItemStateAsync(1, inputState);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStatus, result.Status);
    }

    [Fact]
    public async Task UpdateWorkItemStateAsync_InvalidState_ThrowsException()
    {
        // Arrange
        var mockApiService = new Mock<IAzureDevOpsApiService>();
        
        mockApiService.Setup(x => x.UpdateWorkItemStateAsync(1, "InvalidState"))
            .ThrowsAsync(new InvalidOperationException("Invalid state"));
        
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mockApiService.Object.UpdateWorkItemStateAsync(1, "InvalidState"));
    }

    [Fact]
    public async Task UpdateWorkItemStateAsync_WorkItemNotFound_ReturnsNull()
    {
        // Arrange
        var mockApiService = new Mock<IAzureDevOpsApiService>();
        
        mockApiService.Setup(x => x.UpdateWorkItemStateAsync(999, "Active"))
            .ReturnsAsync((TaskItem?)null);
        
        // Act
        var result = await mockApiService.Object.UpdateWorkItemStateAsync(999, "Active");
        
        // Assert
        Assert.Null(result);
    }
}
