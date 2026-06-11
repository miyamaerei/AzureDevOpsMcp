using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// 仓库映射 MCP 工具
/// 提供本地仓库与 Azure DevOps 项目映射的配置功能
/// </summary>
public class RepositoryMappingTool
{
    private readonly RepositoryMappingService _repositoryMappingService;
    private readonly IUserContext _userContext;

    public RepositoryMappingTool(
        RepositoryMappingService repositoryMappingService,
        IUserContext userContext)
    {
        _repositoryMappingService = repositoryMappingService;
        _userContext = userContext;
    }

    /// <summary>
    /// 设置仓库映射关系
    /// </summary>
    [McpServerTool]
    [Description("配置本地 Git 仓库与 Azure DevOps Project 的映射关系")]
    public async Task<RepositoryMapping> SetRepositoryMapping(
        [Description("本地项目/仓库名称")] string localProject,
        [Description("Azure DevOps Project ID")] string azureProjectId,
        [Description("Azure DevOps Project 名称")] string azureProjectName,
        [Description("仓库 ID")] string repositoryId,
        [Description("仓库名称")] string repositoryName,
        [Description("仓库远程地址")] string remoteUrl,
        [Description("组织名称")] string organization,
        [Description("本地 Git 仓库工作目录")] string workingDirectory,
        [Description("是否设为默认映射")] bool isDefault = false,
        [Description("仓库提供方，例如 GitHub 或 AzureRepos")] string repositoryProvider = "AzureRepos",
        [Description("仓库所有者；GitHub 场景为 owner/organization")] string? repositoryOwner = null)
    {
        var windowsUsername = _userContext.CurrentWindowsUsername
            ?? throw new InvalidOperationException("无法获取当前 Windows 用户名");

        var azureDevOpsUser = await _userContext.GetCurrentAzureDevOpsUserAsync()
            ?? throw new InvalidOperationException("无法获取当前 Azure DevOps 用户");

        var mapping = await _repositoryMappingService.CreateOrUpdateRepositoryMappingAsync(
            windowsUsername: windowsUsername,
            azureDevOpsUser: azureDevOpsUser,
            localProjectName: localProject,
            workingDirectory: workingDirectory,
            azureDevOpsProjectId: azureProjectId,
            azureDevOpsProjectName: azureProjectName,
            repositoryId: repositoryId,
            repositoryName: repositoryName,
            remoteUrl: remoteUrl,
            organization: organization,
            isDefault: isDefault,
            machineName: Environment.MachineName,
            repositoryProvider: repositoryProvider,
            repositoryOwner: repositoryOwner ?? organization);

        return mapping;
    }
}
