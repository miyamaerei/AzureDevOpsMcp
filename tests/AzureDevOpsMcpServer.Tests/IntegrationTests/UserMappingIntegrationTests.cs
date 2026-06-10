using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AzureDevOpsMcpServer.Tests.IntegrationTests;

/// <summary>
/// 用户映射集成测试
/// </summary>
public class UserMappingIntegrationTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly UserMappingService _userMappingService;

    public UserMappingIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _userMappingService = new UserMappingService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task UserMappingService_CreateAndGetMapping_Integration()
    {
        // Arrange
        var windowsUser = "DOMAIN\\user1";
        var azureUser = "user1@company.com";

        // Act
        var result = await _userMappingService.CreateOrUpdateUserMappingAsync(windowsUser, azureUser);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(windowsUser, result.WindowsUsername);
        Assert.Equal(azureUser, result.AzureDevOpsUser);
    }

    [Fact]
    public async Task UserMappingService_GetAzureDevOpsUserFromWindowsUser_Integration()
    {
        // Arrange
        var windowsUser = "DOMAIN\\user2";
        var azureUser = "user2@company.com";

        await _userMappingService.CreateOrUpdateUserMappingAsync(windowsUser, azureUser);

        // Act
        var result = await _userMappingService.GetAzureDevOpsUserFromWindowsUserAsync(windowsUser);

        // Assert
        Assert.Equal(azureUser, result);
    }

    [Fact]
    public async Task UserMappingService_GetAzureDevOpsUserFromWindowsUser_NotFound_Integration()
    {
        // Act
        var result = await _userMappingService.GetAzureDevOpsUserFromWindowsUserAsync("DOMAIN\\nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UserMappingService_GetAllUserMappings_Integration()
    {
        // Arrange
        await _userMappingService.CreateOrUpdateUserMappingAsync("DOMAIN\\user1", "user1@company.com");
        await _userMappingService.CreateOrUpdateUserMappingAsync("DOMAIN\\user2", "user2@company.com");

        // Act
        var result = await _userMappingService.GetAllUserMappingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UserMappingService_DeleteUserMapping_Integration()
    {
        // Arrange
        var windowsUser = "DOMAIN\\todelete";
        await _userMappingService.CreateOrUpdateUserMappingAsync(windowsUser, "todelete@company.com");

        // Act
        var deleteResult = await _userMappingService.DeleteUserMappingAsync(windowsUser);
        var getResult = await _userMappingService.GetAzureDevOpsUserFromWindowsUserAsync(windowsUser);

        // Assert
        Assert.True(deleteResult);
        Assert.Null(getResult);
    }

    [Fact]
    public async Task UserMappingService_DeleteUserMapping_NotFound_Integration()
    {
        // Act
        var result = await _userMappingService.DeleteUserMappingAsync("DOMAIN\\nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UserMappingService_UpdateExistingMapping_Integration()
    {
        // Arrange
        var windowsUser = "DOMAIN\\user3";
        await _userMappingService.CreateOrUpdateUserMappingAsync(windowsUser, "old@company.com");

        // Act
        var result = await _userMappingService.CreateOrUpdateUserMappingAsync(windowsUser, "new@company.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new@company.com", result.AzureDevOpsUser);
        Assert.NotEqual(result.CreatedAt, result.UpdatedAt);
    }
}