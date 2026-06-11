using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using AzureDevOpsMcpServer.PublicApi;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

/// <summary>
/// RepositoryMapping 增强功能测试
/// 测试 SetRepositoryMapping 应该具备的完整功能
/// </summary>
public class RepositoryMappingEnhancedTests
{
    private readonly Mock<IAzureDevOpsApiService> _mockApiService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly Mock<ITaskSyncService> _mockTaskSyncService;

    public RepositoryMappingEnhancedTests()
    {
        _mockApiService = new Mock<IAzureDevOpsApiService>();
        _mockUserContext = new Mock<IUserContext>();
        _mockTaskSyncService = new Mock<ITaskSyncService>();
        
        _mockUserContext.Setup(u => u.CurrentWindowsUsername).Returns("DOMAIN\\alice");
        _mockUserContext.Setup(u => u.GetCurrentAzureDevOpsUserAsync()).ReturnsAsync("alice@example.com");
    }

    [Fact]
    public async Task SetRepositoryMapping_ShouldValidateAzureProjectExists()
    {
        // Arrange
        _mockApiService.Setup(a => a.GetProjectAsync("invalid-project"))
            .ReturnsAsync((Project?)null);

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.SetRepositoryMapping(
                localProject: "MyProject",
                azureProjectId: "invalid-project",
                azureProjectName: "My Azure Project",
                repositoryId: "repo-1",
                repositoryName: "MyRepo",
                remoteUrl: "https://dev.azure.com/org/project/_git/MyRepo",
                organization: "org",
                workingDirectory: "c:\\git\\MyProject"));
    }

    [Fact]
    public async Task SetRepositoryMapping_ShouldValidateAzureRepoExists()
    {
        // Arrange
        _mockApiService.Setup(a => a.GetProjectAsync("project-1"))
            .ReturnsAsync(new Project { Id = Guid.NewGuid(), AzureDevOpsId = "project-1", Name = "My Project" });
        
        _mockApiService.Setup(a => a.GetRepositoryAsync("project-1", "invalid-repo"))
            .ReturnsAsync((RepositoryInfo?)null);

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.SetRepositoryMapping(
                localProject: "MyProject",
                azureProjectId: "project-1",
                azureProjectName: "My Azure Project",
                repositoryId: "invalid-repo",
                repositoryName: "MyRepo",
                remoteUrl: "https://dev.azure.com/org/project/_git/MyRepo",
                organization: "org",
                workingDirectory: "c:\\git\\MyProject"));
    }

    [Fact]
    public async Task SetRepositoryMapping_ShouldAutoLinkWorkItemsWhenBranchMatches()
    {
        // Arrange
        SetupValidAzureResources();

        var service = CreateService();

        // Act - 使用包含 WorkItem ID 的路径（如 feature/task-123）
        await service.SetRepositoryMapping(
            localProject: "MyProject",
            azureProjectId: "project-1",
            azureProjectName: "My Azure Project",
            repositoryId: "repo-1",
            repositoryName: "MyRepo",
            remoteUrl: "https://dev.azure.com/org/project/_git/MyRepo",
            organization: "org",
            workingDirectory: "c:\\git\\feature\\task-123-MyProject");

        // Assert - 验证是否尝试创建 ArtifactLink
        _mockApiService.Verify(a => a.CreateArtifactLinkAsync(
            123, 
            It.IsAny<string>(), 
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SetRepositoryMapping_ShouldSaveMappingSuccessfully()
    {
        // Arrange
        SetupValidAzureResources();

        var service = CreateService();

        // Act
        var result = await service.SetRepositoryMapping(
            localProject: "MyProject",
            azureProjectId: "project-1",
            azureProjectName: "My Azure Project",
            repositoryId: "repo-1",
            repositoryName: "MyRepo",
            remoteUrl: "https://dev.azure.com/org/project/_git/MyRepo",
            organization: "org",
            workingDirectory: "c:\\git\\MyProject");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyProject", result.LocalProjectName);
        Assert.Equal("project-1", result.AzureDevOpsProjectId);
        Assert.Equal("repo-1", result.RepositoryId);
    }

    private void SetupValidAzureResources()
    {
        _mockApiService.Setup(a => a.GetProjectAsync("project-1"))
            .ReturnsAsync(new Project { Id = Guid.NewGuid(), AzureDevOpsId = "project-1", Name = "My Project" });
        
        _mockApiService.Setup(a => a.GetRepositoryAsync("project-1", "repo-1"))
            .ReturnsAsync(new RepositoryInfo { 
                Id = "repo-1", 
                Name = "MyRepo", 
                ProjectId = "project-1" 
            });
    }

    private AzureDevOpsPublicApi CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        var dbContext = new AppDbContext(options);
        dbContext.Database.OpenConnection();
        dbContext.Database.EnsureCreated();

        var mappingService = new RepositoryMappingService(dbContext);
        
        return new AzureDevOpsPublicApi(
            mappingService,
            _mockUserContext.Object,
            _mockApiService.Object,
            _mockTaskSyncService.Object);
    }
}
