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
    /// 同步单个任务到 Azure DevOps
    /// </summary>
    [McpServerTool]
    [Description("将指定任务同步到 Azure DevOps")]
    public async Task<SyncResult> SyncTask(
        [Description("任务 ID")] Guid taskId)
    {
        bool success = await _taskSyncService.SyncTaskToAzureDevOpsAsync(taskId);
        
        return new SyncResult
        {
            Success = success,
            Message = success ? "同步成功" : "同步失败"
        };
    }

    /// <summary>
    /// 根据 WorkItemId 同步任务
    /// </summary>
    [McpServerTool]
    [Description("根据 Azure DevOps WorkItem ID 同步任务")]
    public async Task<SyncResult> SyncTaskByWorkItemId(
        [Description("Azure DevOps WorkItem ID")] string workItemId)
    {
        bool success = await _taskSyncService.SyncTaskByWorkItemIdAsync(workItemId);
        
        return new SyncResult
        {
            Success = success,
            Message = success ? "同步成功" : "同步失败"
        };
    }

    /// <summary>
    /// 同步所有待同步的任务
    /// </summary>
    [McpServerTool]
    [Description("同步所有待同步的任务（状态为归档的任务）")]
    public async Task<SyncResult> SyncAllTasks()
    {
        bool success = await _taskSyncService.SyncAllPendingTasksAsync();
        
        return new SyncResult
        {
            Success = success,
            Message = success ? "所有任务同步成功" : "部分任务同步失败"
        };
    }

    /// <summary>
    /// 获取任务的同步历史
    /// </summary>
    [McpServerTool]
    [Description("获取指定任务的同步历史记录")]
    public async Task<IEnumerable<TaskSyncRecord>> GetSyncHistory(
        [Description("任务 ID")] Guid taskId)
    {
        return await _taskSyncService.GetSyncHistoryAsync(taskId);
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