using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// 任务同步 MCP 工具
/// 提供任务状态同步到 Azure DevOps 的功能
/// </summary>
public class SyncTaskTool
{
    private readonly ITaskSyncService _taskSyncService;

    public SyncTaskTool(ITaskSyncService taskSyncService)
    {
        _taskSyncService = taskSyncService;
    }

    /// <summary>
    /// 同步指定任务到 Azure DevOps
    /// </summary>
    [McpServerTool]
    [Description("将指定任务同步到 Azure DevOps")]
    public async Task<SyncResult> SyncTaskToAzureDevOps(
        [Description("任务 ID")] Guid taskId)
    {
        bool success = await _taskSyncService.SyncTaskToAzureDevOpsAsync(taskId);

        return new SyncResult
        {
            Success = success,
            Message = success ? "同步成功" : "同步失败"
        };
    }
}

/// <summary>
/// 同步结果
/// </summary>
public class SyncResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 结果消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
