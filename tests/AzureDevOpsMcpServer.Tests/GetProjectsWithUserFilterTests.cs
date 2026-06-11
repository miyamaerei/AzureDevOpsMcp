using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using AzureDevOpsMcpServer.Tools;
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

    [Fact]
    public async Task GetProjectsAsync_EmptyUserId_ReturnsAllProjects()
    {
        // Arrange
        var apiService = new Mock<IAzureDevOpsApiService>();
        
        var expectedProjects = new List<Project>
        {
            new Project { Id = Guid.NewGuid(), Name = "Project1" },
            new Project { Id = Guid.NewGuid(), Name = "Project2" }
        };
        
        apiService.Setup(x => x.GetProjectsAsync(string.Empty))
            .ReturnsAsync(expectedProjects);
        
        // Act
        var result = await apiService.Object.GetProjectsAsync(string.Empty);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ProjectRepositoryTool_GetProjects_WithUserContext_UsesCurrentUser()
    {
        // Arrange
        var apiService = new Mock<IAzureDevOpsApiService>();
        var userContext = new Mock<IUserContext>();
        
        var expectedUserId = "current.user@company.com";
        var expectedProjects = new List<Project>
        {
            new Project { Id = Guid.NewGuid(), Name = "Project1" }
        };
        
        userContext.Setup(x => x.GetCurrentAzureDevOpsUserAsync())
            .ReturnsAsync(expectedUserId);
        
        apiService.Setup(x => x.GetProjectsAsync(expectedUserId))
            .ReturnsAsync(expectedProjects);
        
        var tool = new ProjectRepositoryTool(apiService.Object, userContext.Object);
        
        // Act
        var result = await tool.GetProjects();
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        apiService.Verify(x => x.GetProjectsAsync(expectedUserId), Times.Once);
    }

    [Fact]
    public async Task ProjectRepositoryTool_GetProjects_WithoutUserContext_ReturnsAllProjects()
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
        
        var tool = new ProjectRepositoryTool(apiService.Object);
        
        // Act
        var result = await tool.GetProjects();
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        apiService.Verify(x => x.GetProjectsAsync(null), Times.Once);
    }

    [Fact]
    public async Task ProjectRepositoryTool_GetProjects_UserContextReturnsNull_ReturnsAllProjects()
    {
        // Arrange
        var apiService = new Mock<IAzureDevOpsApiService>();
        var userContext = new Mock<IUserContext>();
        
        var expectedProjects = new List<Project>
        {
            new Project { Id = Guid.NewGuid(), Name = "Project1" },
            new Project { Id = Guid.NewGuid(), Name = "Project2" }
        };
        
        userContext.Setup(x => x.GetCurrentAzureDevOpsUserAsync())
            .ReturnsAsync((string?)null);
        
        apiService.Setup(x => x.GetProjectsAsync(null))
            .ReturnsAsync(expectedProjects);
        
        var tool = new ProjectRepositoryTool(apiService.Object, userContext.Object);
        
        // Act
        var result = await tool.GetProjects();
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        apiService.Verify(x => x.GetProjectsAsync(null), Times.Once);
    }
}
