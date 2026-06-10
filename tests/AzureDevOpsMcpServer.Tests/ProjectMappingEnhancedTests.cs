using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

/// <summary>
/// ProjectMapping 增强功能测试
/// </summary>
public class ProjectMappingEnhancedTests
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
    public async Task CreateMapping_WithWorkingDirectory_SavesCorrectly()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new MappingService(dbContext);

        // Act
        var result = await service.CreateOrUpdateMappingAsync(
            localProjectName: "TestProject",
            azureProjectId: "azure-proj-1",
            azureProjectName: "AzureProject1",
            organization: "testorg",
            workingDirectory: "E:\\projects\\TestProject",
            isDefault: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestProject", result.LocalProjectName);
        Assert.Equal("E:\\projects\\TestProject", result.WorkingDirectory);
        Assert.True(result.IsDefault);
    }

    [Fact]
    public async Task GetMappingByWorkingDirectory_ExistingMapping_ReturnsMapping()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new MappingService(dbContext);
        
        var workingDir = "E:\\projects\\MyProject";
        
        await service.CreateOrUpdateMappingAsync(
            localProjectName: "MyProject",
            azureProjectId: "azure-proj-1",
            azureProjectName: "AzureProject1",
            organization: "testorg",
            workingDirectory: workingDir,
            isDefault: true);

        // Act
        var result = await service.GetMappingByWorkingDirectoryAsync(workingDir);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyProject", result.LocalProjectName);
    }

    [Fact]
    public async Task GetMappingByWorkingDirectory_NonExisting_ReturnsNull()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new MappingService(dbContext);

        // Act
        var result = await service.GetMappingByWorkingDirectoryAsync("E:\\non\\existing");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetDefaultMapping_WhenExists_ReturnsDefault()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new MappingService(dbContext);
        
        await service.CreateOrUpdateMappingAsync(
            localProjectName: "Project1",
            azureProjectId: "azure-proj-1",
            azureProjectName: "AzureProject1",
            organization: "testorg",
            isDefault: false);
        
        await service.CreateOrUpdateMappingAsync(
            localProjectName: "Project2",
            azureProjectId: "azure-proj-2",
            azureProjectName: "AzureProject2",
            organization: "testorg",
            isDefault: true);

        // Act
        var result = await service.GetDefaultMappingAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Project2", result.LocalProjectName);
        Assert.True(result.IsDefault);
    }

    [Fact]
    public async Task ValidateMapping_ValidMapping_ReturnsTrue()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new MappingService(dbContext);
        
        await service.CreateOrUpdateMappingAsync(
            localProjectName: "ValidProject",
            azureProjectId: "azure-proj-1",
            azureProjectName: "AzureProject1",
            organization: "testorg");

        // Act
        var result = await service.ValidateMappingAsync("ValidProject");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateMapping_InvalidMapping_ReturnsFalse()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new MappingService(dbContext);

        // Act
        var result = await service.ValidateMappingAsync("NonExistentProject");

        // Assert
        Assert.False(result);
    }
}
