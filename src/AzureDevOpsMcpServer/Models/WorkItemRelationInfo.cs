namespace AzureDevOpsMcpServer.Models;

public enum WorkItemRelationLinkType
{
    Unknown,
    Branch,
    Commit,
    PullRequest,
    GitHubBranch,
    GitHubCommit,
    GitHubPullRequest,
    GitHubIssue,
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

    public string? RepositoryProvider { get; set; }

    public string? RepositoryOwner { get; set; }

    public string? RepositoryName { get; set; }

    public string? PullRequestId { get; set; }

    public string? CommitId { get; set; }

    public string? BranchName { get; set; }

    public string? GitHubIssueId { get; set; }

    public string? ExternalUrl { get; set; }

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

        if (IsGitHubRelation(name, url, "Pull Request", "pull"))
        {
            return WorkItemRelationLinkType.GitHubPullRequest;
        }

        if (IsGitHubRelation(name, url, "Issue", "issues"))
        {
            return WorkItemRelationLinkType.GitHubIssue;
        }

        if (IsGitHubRelation(name, url, "Commit", "commit"))
        {
            return WorkItemRelationLinkType.GitHubCommit;
        }

        if (IsGitHubRelation(name, url, "Branch", "tree"))
        {
            return WorkItemRelationLinkType.GitHubBranch;
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

    private static bool IsGitHubRelation(string name, string url, string relationNameFragment, string urlPathFragment)
    {
        return (name.Contains("GitHub", StringComparison.OrdinalIgnoreCase) &&
                name.Contains(relationNameFragment, StringComparison.OrdinalIgnoreCase)) ||
               (url.Contains("github.com", StringComparison.OrdinalIgnoreCase) &&
                url.Contains($"/{urlPathFragment}/", StringComparison.OrdinalIgnoreCase));
    }

    private void ApplyGitArtifactParts()
    {
        if (LinkType is WorkItemRelationLinkType.GitHubBranch or
            WorkItemRelationLinkType.GitHubCommit or
            WorkItemRelationLinkType.GitHubPullRequest or
            WorkItemRelationLinkType.GitHubIssue)
        {
            ApplyGitHubArtifactParts();
            return;
        }

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

    private void ApplyGitHubArtifactParts()
    {
        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri) ||
            !uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var parts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return;
        }

        RepositoryProvider = "GitHub";
        RepositoryOwner = parts[0];
        RepositoryName = parts[1];
        ExternalUrl = Url;

        if (parts.Length < 4)
        {
            return;
        }

        var objectType = parts[2];
        var objectId = string.Join('/', parts.Skip(3));
        if (objectType.Equals("pull", StringComparison.OrdinalIgnoreCase))
        {
            PullRequestId = objectId;
        }
        else if (objectType.Equals("commit", StringComparison.OrdinalIgnoreCase))
        {
            CommitId = objectId;
        }
        else if (objectType.Equals("tree", StringComparison.OrdinalIgnoreCase))
        {
            BranchName = objectId;
        }
        else if (objectType.Equals("issues", StringComparison.OrdinalIgnoreCase))
        {
            GitHubIssueId = objectId;
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
