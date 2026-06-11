namespace AzureDevOpsMcpServer.Models;

/// <summary>
/// 与某个仓库解析结果一起返回的 Workitem。
/// </summary>
public class RepositoryWorkItem
{
    public TaskItem WorkItem { get; set; } = new();

    public RepositoryMapping? RepositoryMapping { get; set; }

    public RepositoryResolutionSource ResolutionSource { get; set; } = RepositoryResolutionSource.Unresolved;

    public List<WorkItemRelationInfo> MatchingRelations { get; set; } = new();
}
