using System.ComponentModel.DataAnnotations;

namespace AzureDevOpsMcpServer.Models;

public enum TaskStatus
{
    NotStarted,
    InProgress,
    Blocked,
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
    
    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
}