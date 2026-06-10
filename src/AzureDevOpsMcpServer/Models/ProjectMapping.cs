using System.ComponentModel.DataAnnotations;

namespace AzureDevOpsMcpServer.Models;

public class ProjectMapping
{
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// 本地项目名称
    /// </summary>
    public string LocalProjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// Azure DevOps 项目 ID
    /// </summary>
    public string AzureDevOpsProjectId { get; set; } = string.Empty;
    
    /// <summary>
    /// Azure DevOps 项目名称
    /// </summary>
    public string AzureDevOpsProjectName { get; set; } = string.Empty;
    
    /// <summary>
    /// 组织名称
    /// </summary>
    public string Organization { get; set; } = string.Empty;
    
    /// <summary>
    /// 工作目录路径（可选）
    /// </summary>
    public string? WorkingDirectory { get; set; }
    
    /// <summary>
    /// 是否为默认项目
    /// </summary>
    public bool IsDefault { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}