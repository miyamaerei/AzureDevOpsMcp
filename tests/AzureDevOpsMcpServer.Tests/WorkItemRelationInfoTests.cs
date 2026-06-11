using AzureDevOpsMcpServer.Models;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

/// <summary>
/// WorkItemRelationInfo 模型测试类
/// 测试从 Azure DevOps 关联关系解析仓库身份和链接类型
/// </summary>
public class WorkItemRelationInfoTests
{
    /// <summary>
    /// 测试从 GitHub 相关 ArtifactLink 解析仓库身份
    /// 覆盖 GitHub PR、Commit、Branch、Issue 四种链接类型
    /// </summary>
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

    /// <summary>
    /// 测试从 Azure Repos Pull Request ArtifactLink 解析项目、仓库和 PR ID
    /// </summary>
    [Fact]
    public void FromAzureDevOpsRelation_FromPullRequestArtifactLink_IdentifiesRepositoryAndPullRequest()
    {
        var relation = WorkItemRelationInfo.FromAzureDevOpsRelation(
            relationType: "ArtifactLink",
            name: "Pull Request",
            url: "vstfs:///Git/PullRequestId/project-1%2Frepo-1%2F42");

        Assert.Equal("ArtifactLink", relation.RelationType);
        Assert.Equal("Pull Request", relation.Name);
        Assert.Equal("project-1", relation.ProjectId);
        Assert.Equal("repo-1", relation.RepositoryId);
        Assert.Equal("42", relation.PullRequestId);
        Assert.Equal(WorkItemRelationLinkType.PullRequest, relation.LinkType);
    }

    /// <summary>
    /// 测试从 Hierarchy-Reverse 链接类型解析父级工作项 ID
    /// </summary>
    [Fact]
    public void FromAzureDevOpsRelation_FromHierarchyReverse_IdentifiesParentIssue()
    {
        var relation = WorkItemRelationInfo.FromAzureDevOpsRelation(
            relationType: "System.LinkTypes.Hierarchy-Reverse",
            name: "Parent",
            url: "https://dev.azure.com/org/project/_apis/wit/workItems/12345");

        Assert.Equal(WorkItemRelationLinkType.ParentWorkItem, relation.LinkType);
        Assert.Equal("12345", relation.LinkedWorkItemId);
    }
}
