using AzureDevOpsMcpServer.Models;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

public class WorkItemRelationModelTests
{
    [Fact]
    public void WorkItemRelationInfo_FromPullRequestArtifactLink_IdentifiesRepositoryAndPullRequest()
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

    [Fact]
    public void WorkItemRelationInfo_FromHierarchyReverse_IdentifiesParentIssue()
    {
        var relation = WorkItemRelationInfo.FromAzureDevOpsRelation(
            relationType: "System.LinkTypes.Hierarchy-Reverse",
            name: "Parent",
            url: "https://dev.azure.com/org/project/_apis/wit/workItems/12345");

        Assert.Equal(WorkItemRelationLinkType.ParentWorkItem, relation.LinkType);
        Assert.Equal("12345", relation.LinkedWorkItemId);
    }
}
