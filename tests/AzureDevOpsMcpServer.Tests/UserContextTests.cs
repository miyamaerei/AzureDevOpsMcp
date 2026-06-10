using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Moq;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

/// <summary>
/// UserContext 服务测试
/// </summary>
public class UserContextTests
{
    [Fact]
    public void SetCurrentUser_SetsWindowsUsername()
    {
        // Arrange
        var mockMappingService = new Mock<IUserMappingService>();
        var userContext = new UserContext(mockMappingService.Object);

        // Act
        userContext.SetCurrentUser("DOMAIN\\testuser");

        // Assert
        Assert.Equal("DOMAIN\\testuser", userContext.CurrentWindowsUsername);
    }

    [Fact]
    public async Task GetCurrentAzureDevOpsUserAsync_WithMappedUser_ReturnsAzureDevOpsUser()
    {
        // Arrange
        var mockMappingService = new Mock<IUserMappingService>();
        mockMappingService
            .Setup(s => s.GetAzureDevOpsUserFromWindowsUserAsync("DOMAIN\\testuser"))
            .ReturnsAsync("testuser@company.com");
        
        var userContext = new UserContext(mockMappingService.Object);
        userContext.SetCurrentUser("DOMAIN\\testuser");

        // Act
        var result = await userContext.GetCurrentAzureDevOpsUserAsync();

        // Assert
        Assert.Equal("testuser@company.com", result);
    }

    [Fact]
    public async Task GetCurrentAzureDevOpsUserAsync_WithoutSettingUser_ReturnsNull()
    {
        // Arrange
        var mockMappingService = new Mock<IUserMappingService>();
        var userContext = new UserContext(mockMappingService.Object);

        // Act
        var result = await userContext.GetCurrentAzureDevOpsUserAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentAzureDevOpsUserAsync_WithUnmappedUser_ReturnsNull()
    {
        // Arrange
        var mockMappingService = new Mock<IUserMappingService>();
        mockMappingService
            .Setup(s => s.GetAzureDevOpsUserFromWindowsUserAsync("DOMAIN\\unknown"))
            .ReturnsAsync((string?)null);
        
        var userContext = new UserContext(mockMappingService.Object);
        userContext.SetCurrentUser("DOMAIN\\unknown");

        // Act
        var result = await userContext.GetCurrentAzureDevOpsUserAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CurrentWindowsUsername_InitialState_ReturnsNull()
    {
        // Arrange
        var mockMappingService = new Mock<IUserMappingService>();
        var userContext = new UserContext(mockMappingService.Object);

        // Assert
        Assert.Null(userContext.CurrentWindowsUsername);
    }
}