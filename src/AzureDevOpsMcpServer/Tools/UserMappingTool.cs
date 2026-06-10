using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// Windows 用户映射 MCP 工具
/// 提供用户身份映射管理功能
/// </summary>
public class UserMappingTool
{
    private readonly IUserMappingService _userMappingService;
    private readonly IUserContext _userContext;

    public UserMappingTool(IUserMappingService userMappingService, IUserContext userContext)
    {
        _userMappingService = userMappingService;
        _userContext = userContext;
    }

    /// <summary>
    /// 获取当前用户的 Azure DevOps 用户名
    /// </summary>
    [McpServerTool]
    [Description("获取当前 Windows 用户对应的 Azure DevOps 用户名")]
    public async Task<string?> GetCurrentAzureDevOpsUser()
    {
        return await _userContext.GetCurrentAzureDevOpsUserAsync();
    }

    /// <summary>
    /// 设置当前用户的映射
    /// </summary>
    [McpServerTool]
    [Description("设置当前 Windows 用户与 Azure DevOps 用户的映射关系")]
    public async Task<UserMapping> SetCurrentUserMapping(
        [Description("Azure DevOps 用户邮箱或显示名称")] string azureDevOpsUser)
    {
        var windowsUsername = _userContext.CurrentWindowsUsername;
        if (string.IsNullOrEmpty(windowsUsername))
        {
            throw new InvalidOperationException("无法获取当前 Windows 用户名");
        }
        
        return await _userMappingService.CreateOrUpdateUserMappingAsync(windowsUsername, azureDevOpsUser);
    }

    /// <summary>
    /// 管理员：设置指定用户的映射
    /// </summary>
    [McpServerTool]
    [Description("管理员：设置指定 Windows 用户与 Azure DevOps 用户的映射关系")]
    public async Task<UserMapping> SetUserMapping(
        [Description("Windows 用户名 (如 DOMAIN\\username)")] string windowsUsername,
        [Description("Azure DevOps 用户邮箱或显示名称")] string azureDevOpsUser)
    {
        return await _userMappingService.CreateOrUpdateUserMappingAsync(windowsUsername, azureDevOpsUser);
    }

    /// <summary>
    /// 管理员：获取所有用户映射
    /// </summary>
    [McpServerTool]
    [Description("管理员：获取所有用户映射关系")]
    public async Task<IEnumerable<UserMapping>> GetAllUserMappings()
    {
        return await _userMappingService.GetAllUserMappingsAsync();
    }

    /// <summary>
    /// 管理员：删除用户映射
    /// </summary>
    [McpServerTool]
    [Description("管理员：删除指定 Windows 用户的映射关系")]
    public async Task<bool> DeleteUserMapping(
        [Description("Windows 用户名")] string windowsUsername)
    {
        return await _userMappingService.DeleteUserMappingAsync(windowsUsername);
    }
}