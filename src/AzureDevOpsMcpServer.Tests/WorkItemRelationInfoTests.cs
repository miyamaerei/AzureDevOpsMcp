using AzureDevOpsMcpServer.Models;

namespace AzureDevOpsMcpServer.Tests;

public class WorkItemRelationInfoTests
{
    [Theory]
    [InlineData("GitHub Pull Request", "https://github.com/miyamaerei/AzureDevOpsMcp/pull/42", WorkItemRelationLinkType.GitHubPullRequest, "42")]
    [InlineData("GitHub Commit", "https://github.com/miyamaerei/AzureDevOpsMcp/commit/abc123", WorkItemRelationLinkType.GitHubCommit, "abc123")]
    [InlineData("GitHub Branch", "https://github.com/miyamaerei/AzureDevOpsMcp/tree/feature/repo-work-items", WorkItemRelationLinkType.GitHubBranch, "feature/repo-work-items")]
    [InlineData("GitHub Issue", "https://github.com/miyamaerei/AzureDevOpsMcp/issues/7", WorkItemRelationLinkType.GitHubIssue, "7")]
    public void FromAzureDevOpsRelation_ParsesGitHubRepositoryIdentity(
        string name,
        string url,
        WorkItemRelationLinkType expectedLinkType,
        string expectedObjectId)
    {
        var relation = WorkItemRelationInfo.FromAzureDevOpsRelation("ArtifactLink", name, url);

        Assert.Equal(expectedLinkType, relation.LinkType);
        Assert.Equal("GitHub", relation.RepositoryProvider);
        Assert.Equal("miyamaerei", relation.RepositoryOwner);
        Assert.Equal("AzureDevOpsMcp", relation.RepositoryName);
        Assert.Equal(url, relation.ExternalUrl);

        var actualObjectId = expectedLinkType switch
        {
            WorkItemRelationLinkType.GitHubPullRequest => relation.PullRequestId,
            WorkItemRelationLinkType.GitHubCommit => relation.CommitId,
            WorkItemRelationLinkType.GitHubBranch => relation.BranchName,
            WorkItemRelationLinkType.GitHubIssue => relation.GitHubIssueId,
            _ => null
        };

        Assert.Equal(expectedObjectId, actualObjectId);
    }

    [Fact]
    public void FromAzureDevOpsRelation_KeepsAzureRepoArtifactParsing()
    {
        var relation = WorkItemRelationInfo.FromAzureDevOpsRelation(
            "ArtifactLink",
            "Pull Request",
            "vstfs:///Git/PullRequestId/project-1%2Frepo-1%2F42");

        Assert.Equal(WorkItemRelationLinkType.PullRequest, relation.LinkType);
        Assert.Equal("project-1", relation.ProjectId);
        Assert.Equal("repo-1", relation.RepositoryId);
        Assert.Equal("42", relation.PullRequestId);
    }
}
