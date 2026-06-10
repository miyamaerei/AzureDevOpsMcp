using AzureDevOpsMcpServer.Services;
using AzureDevOpsMcpServer.Tools;
using Moq;
using Xunit;

namespace AzureDevOpsMcpServer.Tests.IntegrationTests;

/// <summary>
/// Azure DevOps API 集成测试
/// </summary>
public class AzureDevOpsApiIntegrationTests
{
    private readonly Mock<IAzureDevOpsApiService> _apiServiceMock;
    private readonly Mock<IUserContext> _userContextMock;

    public AzureDevOpsApiIntegrationTests()
    {
        _apiServiceMock = new Mock<IAzureDevOpsApiService>();
        _userContextMock = new Mock<IUserContext>();
    }

    [Fact]
    public async Task ProjectRepositoryTool_GetProjects_Integration()
    {
        // Arrange
        var expectedProjects = new List<Models.Project>
        {
            new Models.Project { Id = Guid.NewGuid(), Name = "Project1" },
            new Models.Project { Id = Guid.NewGuid(), Name = "Project2" }
        };

        _apiServiceMock.Setup(x => x.GetProjectsAsync(null))
            .ReturnsAsync(expectedProjects);

        var tool = new ProjectRepositoryTool(_apiServiceMock.Object);

        // Act
        var result = await tool.GetProjects();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _apiServiceMock.Verify(x => x.GetProjectsAsync(null), Times.Once);
    }

    [Fact]
    public async Task ProjectRepositoryTool_GetProject_Integration()
    {
        // Arrange
        var projectId = "test-project-id";
        var expectedProject = new Models.Project 
        { 
            Id = Guid.NewGuid(), 
            Name = "TestProject",
            AzureDevOpsId = projectId 
        };

        _apiServiceMock.Setup(x => x.GetProjectAsync(projectId))
            .ReturnsAsync(expectedProject);

        var tool = new ProjectRepositoryTool(_apiServiceMock.Object);

        // Act
        var result = await tool.GetProject(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectId, result.AzureDevOpsId);
        Assert.Equal("TestProject", result.Name);
        _apiServiceMock.Verify(x => x.GetProjectAsync(projectId), Times.Once);
    }

    [Fact]
    public async Task ProjectRepositoryTool_GetRepositories_Integration()
    {
        // Arrange
        var projectId = "test-project-id";
        var expectedRepos = new List<RepositoryInfo>
        {
            new RepositoryInfo { Id = "repo1", Name = "Repo1", ProjectId = projectId },
            new RepositoryInfo { Id = "repo2", Name = "Repo2", ProjectId = projectId }
        };

        _apiServiceMock.Setup(x => x.GetRepositoriesAsync(projectId))
            .ReturnsAsync(expectedRepos);

        var tool = new ProjectRepositoryTool(_apiServiceMock.Object);

        // Act
        var result = await tool.GetRepositories(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _apiServiceMock.Verify(x => x.GetRepositoriesAsync(projectId), Times.Once);
    }

    [Fact]
    public async Task ProjectRepositoryTool_GetRepository_Integration()
    {
        // Arrange
        var projectId = "test-project-id";
        var repoId = "test-repo-id";
        var expectedRepo = new RepositoryInfo 
        { 
            Id = repoId, 
            Name = "TestRepo", 
            ProjectId = projectId 
        };

        _apiServiceMock.Setup(x => x.GetRepositoryAsync(projectId, repoId))
            .ReturnsAsync(expectedRepo);

        var tool = new ProjectRepositoryTool(_apiServiceMock.Object);

        // Act
        var result = await tool.GetRepository(projectId, repoId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(repoId, result.Id);
        Assert.Equal("TestRepo", result.Name);
        _apiServiceMock.Verify(x => x.GetRepositoryAsync(projectId, repoId), Times.Once);
    }

    [Fact]
    public async Task ProjectRepositoryTool_WithUserContext_GetProjects_Integration()
    {
        // Arrange
        var userId = "user@company.com";
        var expectedProjects = new List<Models.Project>
        {
            new Models.Project { Id = Guid.NewGuid(), Name = "UserProject" }
        };

        _userContextMock.Setup(x => x.GetCurrentAzureDevOpsUserAsync())
            .ReturnsAsync(userId);

        _apiServiceMock.Setup(x => x.GetProjectsAsync(userId))
            .ReturnsAsync(expectedProjects);

        var tool = new ProjectRepositoryTool(_apiServiceMock.Object, _userContextMock.Object);

        // Act
        var result = await tool.GetProjects();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _userContextMock.Verify(x => x.GetCurrentAzureDevOpsUserAsync(), Times.Once);
        _apiServiceMock.Verify(x => x.GetProjectsAsync(userId), Times.Once);
    }
}