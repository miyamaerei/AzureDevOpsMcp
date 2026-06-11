using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// Azure Repo 映射 MCP 工具。
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

    [McpServerTool]
    [Description("配置当前用户的本地项目、本地目录与 Azure Repo 的映射关系")]
    public async Task<RepositoryMapping> SetRepositoryMapping(
        [Description("本地项目名称")] string localProject,
        [Description("Azure DevOps 项目 ID")] string azureProjectId,
        [Description("Azure DevOps 项目名称")] string azureProjectName,
        [Description("Azure Repo ID")] string repositoryId,
        [Description("Azure Repo 名称")] string repositoryName,
        [Description("Azure Repo 远程地址")] string remoteUrl,
        [Description("组织名称")] string organization,
        [Description("本地工作目录路径（可选）")] string? workingDirectory = null,
        [Description("是否设置为默认 Repo 映射")]
        bool isDefault = false)
    {
        var windowsUsername = GetCurrentWindowsUsername();
        var azureDevOpsUser = await _userContext.GetCurrentAzureDevOpsUserAsync() ?? string.Empty;

        return await _repositoryMappingService.CreateOrUpdateRepositoryMappingAsync(
            windowsUsername,
            azureDevOpsUser,
            localProject,
            workingDirectory,
            azureProjectId,
            azureProjectName,
            repositoryId,
            repositoryName,
            remoteUrl,
            organization,
            isDefault);
    }

    [McpServerTool]
    [Description("获取当前用户本地项目对应的 Azure Repo 映射")]
    public async Task<RepositoryMapping?> GetRepositoryMapping(
        [Description("本地项目名称")] string localProject)
    {
        return await _repositoryMappingService.GetRepositoryMappingByLocalProjectAsync(GetCurrentWindowsUsername(), localProject);
    }

    [McpServerTool]
    [Description("根据当前用户的工作目录获取 Azure Repo 映射")]
    public async Task<RepositoryMapping?> GetRepositoryMappingByWorkingDirectory(
        [Description("本地工作目录路径")] string workingDirectory)
    {
        return await _repositoryMappingService.GetRepositoryMappingByWorkingDirectoryAsync(GetCurrentWindowsUsername(), workingDirectory);
    }

    [McpServerTool]
    [Description("获取当前用户默认 Azure Repo 映射")]
    public async Task<RepositoryMapping?> GetDefaultRepositoryMapping()
    {
        return await _repositoryMappingService.GetDefaultRepositoryMappingAsync(GetCurrentWindowsUsername());
    }

    [McpServerTool]
    [Description("获取当前用户所有 Azure Repo 映射")]
    public async Task<IEnumerable<RepositoryMapping>> GetAllRepositoryMappings()
    {
        return await _repositoryMappingService.GetAllRepositoryMappingsAsync(GetCurrentWindowsUsername());
    }

    private string GetCurrentWindowsUsername()
    {
        return _userContext.CurrentWindowsUsername
            ?? throw new InvalidOperationException("无法获取当前 Windows 用户名");
    }
}
