using System.ComponentModel.DataAnnotations;

namespace AzureDevOpsMcpServer.Models;

/// <summary>
/// 任务状态枚举（基于 CONTEXT.md 术语定义）
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// 未实现 - 尚未开始或被搁置的任务
    /// </summary>
    NotImplemented,
    
    /// <summary>
    /// 当前任务 - 正在积极开发中的任务
    /// </summary>
    Current,
    
    /// <summary>
    /// 阻塞中 - 因外部依赖或问题无法继续推进的任务
    /// </summary>
    Blocked,
    
    /// <summary>
    /// 归档 - 已完成所有验证步骤的任务
    /// </summary>
    Archived
}

public class TaskItem
{
    [Key]
    public Guid Id { get; set; }
    
    public string AzureDevOpsId { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string AssignedTo { get; set; } = string.Empty;
    
    public string ProjectId { get; set; } = string.Empty;
    
    public string ProjectName { get; set; } = string.Empty;
    
    public TaskStatus Status { get; set; } = TaskStatus.NotImplemented;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// 优先级（Low/Medium/High/Critical）
    /// </summary>
    public string Priority { get; set; } = "Medium";
    
    /// <summary>
    /// 是否被阻塞
    /// </summary>
    public bool IsBlocked { get; set; }
    
    /// <summary>
    /// 阻塞原因
    /// </summary>
    public string? BlockedReason { get; set; }
    
    /// <summary>
    /// 阻塞开始时间
    /// </summary>
    public DateTime? BlockedSince { get; set; }
}