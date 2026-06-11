namespace AzureDevOpsMcpServer.Models;

public enum WorkItemRelationLinkType
{
    Unknown,
    Branch,
    Commit,
    PullRequest,
    ParentWorkItem,
    ChildWorkItem
}

/// <summary>
/// Workitem 与代码资产或父子 Workitem 之间的关系信息。
/// </summary>
public class WorkItemRelationInfo
{
    public string RelationType { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public WorkItemRelationLinkType LinkType { get; set; } = WorkItemRelationLinkType.Unknown;

    public string? ProjectId { get; set; }

    public string? RepositoryId { get; set; }

    public string? PullRequestId { get; set; }

    public string? CommitId { get; set; }

    public string? BranchName { get; set; }

    public string? LinkedWorkItemId { get; set; }

    public static WorkItemRelationInfo FromAzureDevOpsRelation(string relationType, string name, string url)
    {
        var relation = new WorkItemRelationInfo
        {
            RelationType = relationType,
            Name = name,
            Url = url,
            LinkType = DetermineLinkType(name, url)
        };

        relation.ApplyGitArtifactParts();
        relation.ApplyLinkedWorkItemId();
        return relation;
    }

    private static WorkItemRelationLinkType DetermineLinkType(string name, string url)
    {
        if (url.Contains("Hierarchy-Reverse", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Parent", StringComparison.OrdinalIgnoreCase))
        {
            return WorkItemRelationLinkType.ParentWorkItem;
        }

        if (url.Contains("Hierarchy-Forward", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Child", StringComparison.OrdinalIgnoreCase))
        {
            return WorkItemRelationLinkType.ChildWorkItem;
        }

        if (name.Contains("Pull Request", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("PullRequest", StringComparison.OrdinalIgnoreCase))
        {
            return WorkItemRelationLinkType.PullRequest;
        }

        if (name.Contains("Commit", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("Commit", StringComparison.OrdinalIgnoreCase))
        {
            return WorkItemRelationLinkType.Commit;
        }

        if (name.Contains("Branch", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("Ref", StringComparison.OrdinalIgnoreCase))
        {
            return WorkItemRelationLinkType.Branch;
        }

        return WorkItemRelationLinkType.Unknown;
    }

    private void ApplyGitArtifactParts()
    {
        var marker = LinkType switch
        {
            WorkItemRelationLinkType.PullRequest => "PullRequestId/",
            WorkItemRelationLinkType.Commit => "Commit/",
            WorkItemRelationLinkType.Branch => "Ref/",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(marker))
        {
            return;
        }

        var markerIndex = Url.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return;
        }

        var encodedPayload = Url[(markerIndex + marker.Length)..];
        var payload = Uri.UnescapeDataString(encodedPayload);
        var parts = payload.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length > 0)
        {
            ProjectId = parts[0];
        }

        if (parts.Length > 1)
        {
            RepositoryId = parts[1];
        }

        if (parts.Length > 2)
        {
            if (LinkType == WorkItemRelationLinkType.PullRequest)
            {
                PullRequestId = parts[2];
            }
            else if (LinkType == WorkItemRelationLinkType.Commit)
            {
                CommitId = parts[2];
            }
            else if (LinkType == WorkItemRelationLinkType.Branch)
            {
                BranchName = parts[2];
            }
        }
    }

    private void ApplyLinkedWorkItemId()
    {
        if (LinkType is not (WorkItemRelationLinkType.ParentWorkItem or WorkItemRelationLinkType.ChildWorkItem))
        {
            return;
        }

        var lastSlash = Url.LastIndexOf('/');
        if (lastSlash < 0 || lastSlash == Url.Length - 1)
        {
            return;
        }

        var candidate = Url[(lastSlash + 1)..];
        if (candidate.All(char.IsDigit))
        {
            LinkedWorkItemId = candidate;
        }
    }
}
