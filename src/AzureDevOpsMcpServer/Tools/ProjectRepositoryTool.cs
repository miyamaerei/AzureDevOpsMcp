using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// Azure DevOps Project 和 Repository MCP 工具
/// 提供项目和仓库的查询功能
/// </summary>
public class ProjectRepositoryTool
{
    private readonly IAzureDevOpsApiService _apiService;

    public ProjectRepositoryTool(IAzureDevOpsApiService apiService)
    {
        _apiService = apiService;
    }

    #region Project 操作

    /// <summary>
    /// 获取所有项目
    /// </summary>
    [McpServerTool]
    [Description("获取当前组织下的所有项目，可选按用户过滤")]
    public async Task<IEnumerable<Project>> GetProjects(
        [Description("用户标识（可选），用于过滤用户可访问的项目")] string? userId = null)
    {
        return await _apiService.GetProjectsAsync(userId);
    }

    /// <summary>
    /// 获取项目详情
    /// </summary>
    [McpServerTool]
    [Description("根据 ID 或名称获取项目详情")]
    public async Task<Project?> GetProject(
        [Description("项目 ID 或名称")] string projectId)
    {
        return await _apiService.GetProjectAsync(projectId);
    }

    #endregion

    #region Repository 操作

    /// <summary>
    /// 获取项目下的所有仓库
    /// </summary>
    [McpServerTool]
    [Description("获取指定项目下的所有 Git 仓库")]
    public async Task<IEnumerable<RepositoryInfo>> GetRepositories(
        [Description("项目 ID 或名称")] string projectId)
    {
        return await _apiService.GetRepositoriesAsync(projectId);
    }

    /// <summary>
    /// 获取仓库详情
    /// </summary>
    [McpServerTool]
    [Description("获取指定仓库的详细信息")]
    public async Task<RepositoryInfo?> GetRepository(
        [Description("项目 ID 或名称")] string projectId,
        [Description("仓库 ID 或名称")] string repositoryId)
    {
        return await _apiService.GetRepositoryAsync(projectId, repositoryId);
    }

    #endregion
}
