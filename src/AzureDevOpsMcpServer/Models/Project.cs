using System.ComponentModel.DataAnnotations;

namespace AzureDevOpsMcpServer.Models;

public class Project
{
    [Key]
    public Guid Id { get; set; }
    
    public string AzureDevOpsId { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Organization { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}