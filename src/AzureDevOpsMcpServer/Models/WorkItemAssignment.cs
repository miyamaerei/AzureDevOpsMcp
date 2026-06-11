namespace AzureDevOpsMcpServer.Models;

public enum RepositoryResolutionSource
{
    Unresolved,
    WorkItemArtifactLink,
    ParentWorkItemArtifactLink,
    UserDefaultRepositoryMapping
}

/// <summary>
/// 当前用户处理某个 Workitem 所需的完整上下文。
/// </summary>
public class WorkItemAssignment
{
    public WorkItemWithRelations WorkItem { get; set; } = new();

    public WorkItemWithRelations? ParentWorkItem { get; set; }

    public RepositoryMapping? RepositoryMapping { get; set; }

    public string? LocalWorkingDirectory => RepositoryMapping?.WorkingDirectory;

    public RepositoryResolutionSource ResolutionSource { get; set; } = RepositoryResolutionSource.Unresolved;
}
