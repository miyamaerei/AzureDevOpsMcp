using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// 任务历史 MCP 工具
/// 提供 WorkItem 状态变更历史的查询功能
/// </summary>
public class TaskHistoryTool
{
    private readonly IAzureDevOpsApiService _apiService;

    public TaskHistoryTool(IAzureDevOpsApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// 获取 WorkItem 的状态变更历史
    /// </summary>
    [McpServerTool]
    [Description("获取指定 WorkItem 的状态变更历史记录")]
    public async Task<IEnumerable<TaskStateHistory>> GetTaskHistory(
        [Description("WorkItem ID")] int workItemId)
    {
        return await _apiService.GetTaskHistoryAsync(workItemId);
    }
}
