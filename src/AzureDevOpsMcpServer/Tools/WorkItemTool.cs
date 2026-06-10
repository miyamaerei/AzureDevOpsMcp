using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// Azure DevOps WorkItem MCP 工具
/// 提供真实的 WorkItem 查询和操作功能
/// </summary>
public class WorkItemTool
{
    private readonly IAzureDevOpsApiService _apiService;
    private readonly MappingService _mappingService;

    public WorkItemTool(IAzureDevOpsApiService apiService, MappingService mappingService)
    {
        _apiService = apiService;
        _mappingService = mappingService;
    }

    /// <summary>
    /// 获取指派给用户的 WorkItem 列表（支持无参调用使用默认项目）
    /// </summary>
    [McpServerTool]
    [Description("获取指派给指定用户的 WorkItem 列表")]
    public async Task<IEnumerable<TaskItem>> GetAssignedWorkItems(
        [Description("用户标识 (邮箱或显示名称)")] string userId,
        [Description("项目名称或 ID（可选，不填则使用默认项目）")] string? projectId = null)
    {
        var effectiveProjectId = projectId ?? await GetDefaultProjectIdAsync();
        if (string.IsNullOrEmpty(effectiveProjectId))
        {
            throw new InvalidOperationException("未指定项目且没有设置默认项目映射");
        }
        return await _apiService.GetAssignedWorkItemsAsync(userId, effectiveProjectId);
    }

    /// <summary>
    /// 获取 WorkItem 详情
    /// </summary>
    [McpServerTool]
    [Description("根据 ID 获取 WorkItem 详细信息")]
    public async Task<TaskItem?> GetWorkItemDetails(
        [Description("WorkItem ID")] int workItemId)
    {
        return await _apiService.GetWorkItemDetailsAsync(workItemId);
    }

    /// <summary>
    /// 更新 WorkItem 状态
    /// </summary>
    [McpServerTool]
    [Description("更新 WorkItem 的状态")]
    public async Task<TaskItem?> UpdateWorkItemState(
        [Description("WorkItem ID")] int workItemId,
        [Description("新状态 (New, Active, Resolved, Closed)")] string state)
    {
        return await _apiService.UpdateWorkItemStateAsync(workItemId, state);
    }

    /// <summary>
    /// 执行自定义 WIQL 查询（支持无参调用使用默认项目）
    /// </summary>
    [McpServerTool]
    [Description("执行自定义 WIQL 查询获取 WorkItem")]
    public async Task<IEnumerable<TaskItem>> QueryWorkItems(
        [Description("WIQL 查询语句")] string wiqlQuery,
        [Description("项目名称或 ID（可选，不填则使用默认项目）")] string? projectId = null)
    {
        var effectiveProjectId = projectId ?? await GetDefaultProjectIdAsync();
        if (string.IsNullOrEmpty(effectiveProjectId))
        {
            throw new InvalidOperationException("未指定项目且没有设置默认项目映射");
        }
        return await _apiService.QueryWorkItemsAsync(wiqlQuery, effectiveProjectId);
    }

    /// <summary>
    /// 获取默认项目 ID
    /// </summary>
    private async Task<string?> GetDefaultProjectIdAsync()
    {
        var defaultMapping = await _mappingService.GetDefaultMappingAsync();
        return defaultMapping?.AzureDevOpsProjectId;
    }
}
