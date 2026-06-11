using AzureDevOpsMcpServer.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

public class RepositoryMappingServiceTests
{
    private static AppDbContext CreateInMemoryDbContext()
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
    public async Task CreateOrUpdateRepositoryMapping_AllowsLocalProjectToResolveAzureRepo()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new RepositoryMappingService(dbContext);

        await service.CreateOrUpdateRepositoryMappingAsync(
            windowsUsername: "DOMAIN\\alice",
            azureDevOpsUser: "alice@example.com",
            localProjectName: "AzureDevOpsMcp",
            workingDirectory: "c:\\gitrepos\\AzureDevOpsMcp",
            azureDevOpsProjectId: "project-1",
            azureDevOpsProjectName: "Platform Project",
            repositoryId: "repo-1",
            repositoryName: "AzureDevOpsMcp",
            remoteUrl: "https://dev.azure.com/org/project/_git/AzureDevOpsMcp",
            organization: "org",
            isDefault: true);

        var result = await service.GetRepositoryMappingByLocalProjectAsync("DOMAIN\\alice", "AzureDevOpsMcp");

        Assert.NotNull(result);
        Assert.Equal("DOMAIN\\alice", result!.WindowsUsername);
        Assert.Equal("alice@example.com", result.AzureDevOpsUser);
        Assert.Equal("AzureDevOpsMcp", result.LocalProjectName);
        Assert.Equal("repo-1", result.RepositoryId);
        Assert.Equal("AzureDevOpsMcp", result.RepositoryName);
        Assert.Equal("project-1", result.AzureDevOpsProjectId);
        Assert.True(result.IsDefault);
    }

    [Fact]
    public async Task CreateOrUpdateRepositoryMapping_WhenAnotherDefaultExists_OnlyNewOneIsDefault()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new RepositoryMappingService(dbContext);

        await service.CreateOrUpdateRepositoryMappingAsync(
            windowsUsername: "DOMAIN\\alice",
            azureDevOpsUser: "alice@example.com",
            localProjectName: "ProjectA",
            workingDirectory: "c:\\repos\\ProjectA",
            azureDevOpsProjectId: "project-1",
            azureDevOpsProjectName: "Platform Project",
            repositoryId: "repo-a",
            repositoryName: "RepoA",
            remoteUrl: "https://dev.azure.com/org/project/_git/RepoA",
            organization: "org",
            isDefault: true);

        await service.CreateOrUpdateRepositoryMappingAsync(
            windowsUsername: "DOMAIN\\alice",
            azureDevOpsUser: "alice@example.com",
            localProjectName: "ProjectB",
            workingDirectory: "c:\\repos\\ProjectB",
            azureDevOpsProjectId: "project-1",
            azureDevOpsProjectName: "Platform Project",
            repositoryId: "repo-b",
            repositoryName: "RepoB",
            remoteUrl: "https://dev.azure.com/org/project/_git/RepoB",
            organization: "org",
            isDefault: true);

        await service.CreateOrUpdateRepositoryMappingAsync(
            windowsUsername: "DOMAIN\\bob",
            azureDevOpsUser: "bob@example.com",
            localProjectName: "ProjectA",
            workingDirectory: "d:\\repos\\ProjectA",
            azureDevOpsProjectId: "project-1",
            azureDevOpsProjectName: "Platform Project",
            repositoryId: "repo-a",
            repositoryName: "RepoA",
            remoteUrl: "https://dev.azure.com/org/project/_git/RepoA",
            organization: "org",
            isDefault: true);

        var aliceDefaultMapping = await service.GetDefaultRepositoryMappingAsync("DOMAIN\\alice");
        var bobDefaultMapping = await service.GetDefaultRepositoryMappingAsync("DOMAIN\\bob");
        var aliceMappings = await service.GetAllRepositoryMappingsAsync("DOMAIN\\alice");

        Assert.NotNull(aliceDefaultMapping);
        Assert.Equal("ProjectB", aliceDefaultMapping!.LocalProjectName);
        Assert.NotNull(bobDefaultMapping);
        Assert.Equal("ProjectA", bobDefaultMapping!.LocalProjectName);
        Assert.Single(aliceMappings, mapping => mapping.IsDefault);
    }
}
