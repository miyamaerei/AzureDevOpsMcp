using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AzureDevOpsMcpServer.Tests.IntegrationTests;

/// <summary>
/// 项目映射集成测试
/// </summary>
public class ProjectMappingIntegrationTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly MappingService _mappingService;

    public ProjectMappingIntegrationTests()
    {
        // 使用内存数据库进行集成测试
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _mappingService = new MappingService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task MappingService_CreateAndGetMapping_Integration()
    {
        // Arrange & Act
        var result = await _mappingService.CreateOrUpdateMappingAsync(
            "TestProject",
            "azure-project-id",
            "AzureProject",
            "https://dev.azure.com/myorg",
            "/workspace/test",
            true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestProject", result.LocalProjectName);
        Assert.Equal("azure-project-id", result.AzureDevOpsProjectId);
        Assert.True(result.IsDefault);
    }

    [Fact]
    public async Task MappingService_GetAllMappings_Integration()
    {
        // Arrange
        var mappings = new List<ProjectMapping>
        {
            new ProjectMapping { LocalProjectName = "Project1", AzureDevOpsProjectId = "id1" },
            new ProjectMapping { LocalProjectName = "Project2", AzureDevOpsProjectId = "id2" }
        };

        await _dbContext.ProjectMappings.AddRangeAsync(mappings);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _mappingService.GetAllMappingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task MappingService_GetDefaultMapping_Integration()
    {
        // Arrange
        var mappings = new List<ProjectMapping>
        {
            new ProjectMapping { LocalProjectName = "Project1", AzureDevOpsProjectId = "id1", IsDefault = false },
            new ProjectMapping { LocalProjectName = "Project2", AzureDevOpsProjectId = "id2", IsDefault = true }
        };

        await _dbContext.ProjectMappings.AddRangeAsync(mappings);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _mappingService.GetDefaultMappingAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Project2", result.LocalProjectName);
        Assert.True(result.IsDefault);
    }

    [Fact]
    public async Task MappingService_DeleteMapping_Integration()
    {
        // Arrange
        var mapping = new ProjectMapping
        {
            LocalProjectName = "ToDelete",
            AzureDevOpsProjectId = "delete-id"
        };

        await _dbContext.ProjectMappings.AddAsync(mapping);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _mappingService.DeleteMappingAsync("ToDelete");

        // Assert
        Assert.True(result);
        var deletedMapping = await _mappingService.GetMappingByLocalProjectAsync("ToDelete");
        Assert.Null(deletedMapping);
    }

    [Fact]
    public async Task MappingService_GetMappingByWorkingDirectory_Integration()
    {
        // Arrange
        var mapping = new ProjectMapping
        {
            LocalProjectName = "DirectoryProject",
            AzureDevOpsProjectId = "dir-id",
            WorkingDirectory = "/workspace/myproject"
        };

        await _dbContext.ProjectMappings.AddAsync(mapping);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _mappingService.GetMappingByWorkingDirectoryAsync("/workspace/myproject");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("DirectoryProject", result.LocalProjectName);
    }

    [Fact]
    public async Task MappingService_ValidateMapping_Integration()
    {
        // Arrange
        var mapping = new ProjectMapping
        {
            LocalProjectName = "ValidProject",
            AzureDevOpsProjectId = "valid-id",
            IsDefault = true
        };

        await _dbContext.ProjectMappings.AddAsync(mapping);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _mappingService.ValidateMappingAsync("ValidProject");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task MappingService_GetMappingsByAzureDevOpsProjectId_Integration()
    {
        // Arrange
        var mappings = new List<ProjectMapping>
        {
            new ProjectMapping { LocalProjectName = "ProjectA", AzureDevOpsProjectId = "shared-id" },
            new ProjectMapping { LocalProjectName = "ProjectB", AzureDevOpsProjectId = "shared-id" },
            new ProjectMapping { LocalProjectName = "ProjectC", AzureDevOpsProjectId = "other-id" }
        };

        await _dbContext.ProjectMappings.AddRangeAsync(mappings);
        await _dbContext.SaveChangesAsync();

        // Act
        var allMappings = await _mappingService.GetAllMappingsAsync();
        var result = allMappings.Where(m => m.AzureDevOpsProjectId == "shared-id");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, m => Assert.Equal("shared-id", m.AzureDevOpsProjectId));
    }
}