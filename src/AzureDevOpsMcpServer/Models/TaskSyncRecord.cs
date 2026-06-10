using System.ComponentModel.DataAnnotations;

namespace AzureDevOpsMcpServer.Models;

/// <summary>
/// 任务同步记录
/// </summary>
public class TaskSyncRecord
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// 任务 ID（本地任务标识）
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Azure DevOps WorkItem ID
    /// </summary>
    public string WorkItemId { get; set; } = string.Empty;

    /// <summary>
    /// 内部状态
    /// </summary>
    public TaskStatus InternalStatus { get; set; }

    /// <summary>
    /// Azure DevOps 状态
    /// </summary>
    public string AzureDevOpsState { get; set; } = string.Empty;

    /// <summary>
    /// 同步是否成功
    /// </summary>
    public bool SyncSuccess { get; set; }

    /// <summary>
    /// 错误消息（同步失败时）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 同步时间
    /// </summary>
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 同步尝试次数
    /// </summary>
    public int AttemptCount { get; set; } = 1;
}