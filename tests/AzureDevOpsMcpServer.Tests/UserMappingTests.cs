using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

/// <summary>
/// UserMapping Windows 用户认证集成测试
/// </summary>
public class UserMappingTests
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
    public async Task CreateUserMapping_SavesCorrectly()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new UserMappingService(dbContext);

        // Act
        var result = await service.CreateOrUpdateUserMappingAsync(
            windowsUsername: "DOMAIN\\testuser",
            azureDevOpsUser: "testuser@company.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("DOMAIN\\testuser", result.WindowsUsername);
        Assert.Equal("testuser@company.com", result.AzureDevOpsUser);
    }

    [Fact]
    public async Task GetAzureDevOpsUserFromWindowsUser_ExistingMapping_ReturnsUser()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new UserMappingService(dbContext);
        
        await service.CreateOrUpdateUserMappingAsync(
            windowsUsername: "DOMAIN\\developer1",
            azureDevOpsUser: "developer1@company.com");

        // Act
        var result = await service.GetAzureDevOpsUserFromWindowsUserAsync("DOMAIN\\developer1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("developer1@company.com", result);
    }

    [Fact]
    public async Task GetAzureDevOpsUserFromWindowsUser_NonExisting_ReturnsNull()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new UserMappingService(dbContext);

        // Act
        var result = await service.GetAzureDevOpsUserFromWindowsUserAsync("DOMAIN\\unknown");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateExistingUserMapping_UpdatesCorrectly()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new UserMappingService(dbContext);
        
        await service.CreateOrUpdateUserMappingAsync(
            windowsUsername: "DOMAIN\\user1",
            azureDevOpsUser: "user1@company.com");

        // Act
        var result = await service.CreateOrUpdateUserMappingAsync(
            windowsUsername: "DOMAIN\\user1",
            azureDevOpsUser: "newuser@company.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newuser@company.com", result.AzureDevOpsUser);
        
        // Should still be only one record
        var allMappings = await dbContext.UserMappings.ToListAsync();
        Assert.Single(allMappings);
    }

    [Fact]
    public async Task GetAllUserMappings_ReturnsAllMappings()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new UserMappingService(dbContext);
        
        await service.CreateOrUpdateUserMappingAsync("DOMAIN\\user1", "user1@company.com");
        await service.CreateOrUpdateUserMappingAsync("DOMAIN\\user2", "user2@company.com");

        // Act
        var result = await service.GetAllUserMappingsAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeleteUserMapping_RemovesMapping()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new UserMappingService(dbContext);
        
        await service.CreateOrUpdateUserMappingAsync("DOMAIN\\user1", "user1@company.com");

        // Act
        var deleted = await service.DeleteUserMappingAsync("DOMAIN\\user1");
        var allMappings = await service.GetAllUserMappingsAsync();

        // Assert
        Assert.True(deleted);
        Assert.Empty(allMappings);
    }

    [Fact]
    public async Task DeleteUserMapping_NonExisting_ReturnsFalse()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var service = new UserMappingService(dbContext);

        // Act
        var deleted = await service.DeleteUserMappingAsync("DOMAIN\\unknown");

        // Assert
        Assert.False(deleted);
    }
}