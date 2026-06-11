using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// Azure DevOps WorkItem MCP 工具
/// 提供获取当前仓库关联任务的功能
/// </summary>
public class WorkItemTool
{
    private readonly IAzureDevOpsApiService _apiService;
    private readonly RepositoryMappingService _repositoryMappingService;
    private readonly IUserContext _userContext;

    public WorkItemTool(
        IAzureDevOpsApiService apiService,
        RepositoryMappingService repositoryMappingService,
        IUserContext userContext)
    {
        _apiService = apiService;
        _repositoryMappingService = repositoryMappingService;
        _userContext = userContext;
    }

    /// <summary>
    /// 获取指派给当前用户且关联到当前默认仓库映射的 WorkItem 列表
    /// </summary>
    [McpServerTool]
    [Description("获取指派给当前用户且通过 GitHub/Azure Repo 关系关联到当前默认仓库的 WorkItem 列表")]
    public async Task<IEnumerable<RepositoryWorkItem>> GetAssignedWorkItemsForCurrentRepository(
        [Description("是否包含无法解析仓库关系的 WorkItem，默认 false")]
        bool includeUnresolved = false)
    {
        // 自动获取当前用户
        var userId = await _userContext.GetCurrentAzureDevOpsUserAsync()
            ?? throw new InvalidOperationException("无法获取当前用户");

        var mapping = await _repositoryMappingService.GetDefaultRepositoryMappingAsync(GetCurrentWindowsUsername())
            ?? throw new InvalidOperationException("没有设置默认仓库映射");

        return await GetAssignedWorkItemsForRepositoryMapping(userId, mapping, includeUnresolved);
    }

    /// <summary>
    /// 根据仓库映射获取指派给当前用户的 WorkItem 列表
    /// </summary>
    private async Task<IEnumerable<RepositoryWorkItem>> GetAssignedWorkItemsForRepositoryMapping(
        string userId,
        RepositoryMapping targetMapping,
        bool includeUnresolved)
    {
        if (string.IsNullOrWhiteSpace(targetMapping.AzureDevOpsProjectId))
        {
            throw new InvalidOperationException("仓库映射缺少 Azure DevOps Project ID，无法查询 Azure Boards Workitem");
        }

        var assignedWorkItems = await _apiService.GetAssignedWorkItemsAsync(userId, targetMapping.AzureDevOpsProjectId);
        var results = new List<RepositoryWorkItem>();

        foreach (var task in assignedWorkItems)
        {
            if (!int.TryParse(task.AzureDevOpsId, out var workItemId))
            {
                continue;
            }

            var workItem = await _apiService.GetWorkItemDetailsWithRelationsAsync(workItemId);
            if (workItem == null)
            {
                continue;
            }

            var matchingRelations = GetMatchingRepositoryRelations(workItem.Relations, targetMapping).ToList();
            if (matchingRelations.Count > 0)
            {
                results.Add(new RepositoryWorkItem
                {
                    WorkItem = workItem.WorkItem,
                    RepositoryMapping = targetMapping,
                    ResolutionSource = RepositoryResolutionSource.WorkItemArtifactLink,
                    MatchingRelations = matchingRelations
                });
                continue;
            }

            if (includeUnresolved)
            {
                results.Add(new RepositoryWorkItem
                {
                    WorkItem = workItem.WorkItem,
                    RepositoryMapping = null,
                    ResolutionSource = RepositoryResolutionSource.Unresolved,
                    MatchingRelations = new List<WorkItemRelationInfo>()
                });
            }
        }

        return results;
    }

    /// <summary>
    /// 获取与仓库映射匹配的关联关系
    /// </summary>
    private static IEnumerable<WorkItemRelationInfo> GetMatchingRepositoryRelations(
        IEnumerable<WorkItemRelationInfo> relations,
        RepositoryMapping mapping)
    {
        return relations.Where(relation => MatchesRepository(relation, mapping));
    }

    /// <summary>
    /// 判断关联关系是否匹配仓库映射
    /// </summary>
    private static bool MatchesRepository(WorkItemRelationInfo relation, RepositoryMapping mapping)
    {
        if (!string.IsNullOrWhiteSpace(relation.RepositoryProvider) &&
            relation.RepositoryProvider.Equals(mapping.RepositoryProvider, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(relation.RepositoryOwner) &&
            relation.RepositoryOwner.Equals(mapping.RepositoryOwner, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(relation.RepositoryName) &&
            relation.RepositoryName.Equals(mapping.RepositoryName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(relation.RepositoryId) &&
            !string.IsNullOrWhiteSpace(mapping.RepositoryId) &&
            relation.RepositoryId.Equals(mapping.RepositoryId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取当前 Windows 用户名
    /// </summary>
    private string GetCurrentWindowsUsername()
    {
        return _userContext.CurrentWindowsUsername
            ?? throw new InvalidOperationException("无法获取当前 Windows 用户名");
    }
}
