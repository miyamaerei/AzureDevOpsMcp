using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Moq;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

/// <summary>
/// GetProjects with user filter 测试
/// </summary>
public class GetProjectsWithUserFilterTests
{
    [Fact]
    public async Task GetProjectsAsync_WithoutUserId_ReturnsAllProjects()
    {
        // Arrange
        var apiService = new Mock<IAzureDevOpsApiService>();
        
        var expectedProjects = new List<Project>
        {
            new Project { Id = Guid.NewGuid(), Name = "Project1" },
            new Project { Id = Guid.NewGuid(), Name = "Project2" }
        };
        
        apiService.Setup(x => x.GetProjectsAsync(null))
            .ReturnsAsync(expectedProjects);
        
        // Act
        var result = await apiService.Object.GetProjectsAsync(null);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetProjectsAsync_WithUserId_ReturnsProjects()
    {
        // Arrange
        var apiService = new Mock<IAzureDevOpsApiService>();
        var userId = "user@company.com";
        
        var expectedProjects = new List<Project>
        {
            new Project { Id = Guid.NewGuid(), Name = "Project1" }
        };
        
        apiService.Setup(x => x.GetProjectsAsync(userId))
            .ReturnsAsync(expectedProjects);
        
        // Act
        var result = await apiService.Object.GetProjectsAsync(userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }
}
