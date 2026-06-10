using System.ComponentModel.DataAnnotations;

namespace AzureDevOpsMcpServer.Models;

/// <summary>
/// Windows 用户与 Azure DevOps 用户的映射关系
/// </summary>
public class UserMapping
{
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Windows 用户名（如: DOMAIN\username）
    /// </summary>
    public string WindowsUsername { get; set; } = string.Empty;
    
    /// <summary>
    /// Azure DevOps 用户（如: user@company.com）
    /// </summary>
    public string AzureDevOpsUser { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsed { get; set; }
}