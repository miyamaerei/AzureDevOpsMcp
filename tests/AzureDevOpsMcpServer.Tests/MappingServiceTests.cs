using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

public class MappingServiceTests
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
    public async Task CreateOrUpdateMapping_NewMapping_CreatesNewRecord()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new MappingService(dbContext);

        var result = await service.CreateOrUpdateMappingAsync(
            "LocalProject1", 
            "azure-proj-1", 
            "AzureProject1", 
            "TestOrg");

        Assert.NotNull(result);
        Assert.Equal("LocalProject1", result.LocalProjectName);
        Assert.Equal("azure-proj-1", result.AzureDevOpsProjectId);
        Assert.Equal("AzureProject1", result.AzureDevOpsProjectName);
        Assert.Equal("TestOrg", result.Organization);
        
        var savedMapping = await dbContext.ProjectMappings.FirstOrDefaultAsync();
        Assert.NotNull(savedMapping);
        Assert.Equal("LocalProject1", savedMapping.LocalProjectName);
    }

    [Fact]
    public async Task CreateOrUpdateMapping_ExistingMapping_UpdatesRecord()
    {
        using var dbContext = CreateInMemoryDbContext();
        
        var existingMapping = new ProjectMapping
        {
            Id = Guid.NewGuid(),
            LocalProjectName = "LocalProject1",
            AzureDevOpsProjectId = "old-id",
            AzureDevOpsProjectName = "OldName",
            Organization = "OldOrg"
        };
        await dbContext.ProjectMappings.AddAsync(existingMapping);
        await dbContext.SaveChangesAsync();

        var service = new MappingService(dbContext);

        var result = await service.CreateOrUpdateMappingAsync(
            "LocalProject1", 
            "new-id", 
            "NewName", 
            "NewOrg");

        Assert.NotNull(result);
        Assert.Equal("new-id", result.AzureDevOpsProjectId);
        Assert.Equal("NewName", result.AzureDevOpsProjectName);
        Assert.Equal("NewOrg", result.Organization);
    }

    [Fact]
    public async Task GetMappingByLocalProject_ExistingProject_ReturnsMapping()
    {
        using var dbContext = CreateInMemoryDbContext();
        
        var mapping = new ProjectMapping
        {
            Id = Guid.NewGuid(),
            LocalProjectName = "MyProject",
            AzureDevOpsProjectId = "azure-123",
            AzureDevOpsProjectName = "AzureProject",
            Organization = "MyOrg"
        };
        await dbContext.ProjectMappings.AddAsync(mapping);
        await dbContext.SaveChangesAsync();

        var service = new MappingService(dbContext);
        var result = await service.GetMappingByLocalProjectAsync("MyProject");

        Assert.NotNull(result);
        Assert.Equal("azure-123", result.AzureDevOpsProjectId);
    }

    [Fact]
    public async Task GetMappingByLocalProject_NonExistingProject_ReturnsNull()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new MappingService(dbContext);

        var result = await service.GetMappingByLocalProjectAsync("NonExisting");

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteMapping_ExistingProject_DeletesSuccessfully()
    {
        using var dbContext = CreateInMemoryDbContext();
        
        var mapping = new ProjectMapping
        {
            Id = Guid.NewGuid(),
            LocalProjectName = "ToDelete",
            AzureDevOpsProjectId = "azure-123"
        };
        await dbContext.ProjectMappings.AddAsync(mapping);
        await dbContext.SaveChangesAsync();

        var service = new MappingService(dbContext);
        var result = await service.DeleteMappingAsync("ToDelete");

        Assert.True(result);
        Assert.Empty(await dbContext.ProjectMappings.ToListAsync());
    }

    [Fact]
    public async Task DeleteMapping_NonExistingProject_ReturnsFalse()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new MappingService(dbContext);

        var result = await service.DeleteMappingAsync("NonExisting");

        Assert.False(result);
    }
}