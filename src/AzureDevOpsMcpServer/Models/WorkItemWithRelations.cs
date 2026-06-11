namespace AzureDevOpsMcpServer.Models;

/// <summary>
/// 带代码资产关系的 Workitem 详情。
/// </summary>
public class WorkItemWithRelations
{
    public TaskItem WorkItem { get; set; } = new();

    public List<WorkItemRelationInfo> Relations { get; set; } = new();
}
