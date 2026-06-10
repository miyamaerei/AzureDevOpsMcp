using System.ComponentModel.DataAnnotations;

namespace AzureDevOpsMcpServer.Models;

/// <summary>
/// 任务状态变更历史记录
/// </summary>
public class TaskStateHistory
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Azure DevOps WorkItem ID
    /// </summary>
    public string WorkItemId { get; set; } = string.Empty;

    /// <summary>
    /// 变更前的状态
    /// </summary>
    public TaskStatus OldStatus { get; set; }

    /// <summary>
    /// 变更后的状态
    /// </summary>
    public TaskStatus NewStatus { get; set; }

    /// <summary>
    /// 变更人（用户名或邮箱）
    /// </summary>
    public string ChangedBy { get; set; } = string.Empty;

    /// <summary>
    /// 变更时间
    /// </summary>
    public DateTime ChangedAt { get; set; }
}
