using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// Azure DevOps WorkItem MCP 工具
/// 提供真实的 WorkItem 查询和操作功能
/// </summary>
public class WorkItemTool
{
    private readonly IAzureDevOpsApiService _apiService;
    private readonly MappingService _mappingService;
    private readonly RepositoryMappingService _repositoryMappingService;
    private readonly IUserContext _userContext;

    public WorkItemTool(
        IAzureDevOpsApiService apiService,
        MappingService mappingService,
        RepositoryMappingService repositoryMappingService,
        IUserContext userContext)
    {
        _apiService = apiService;
        _mappingService = mappingService;
        _repositoryMappingService = repositoryMappingService;
        _userContext = userContext;
    }

    /// <summary>
    /// 获取指派给用户的 WorkItem 列表（支持无参调用使用默认项目）
    /// </summary>
    [McpServerTool]
    [Description("获取指派给指定用户的 WorkItem 列表")]
    public async Task<IEnumerable<TaskItem>> GetAssignedWorkItems(
        [Description("用户标识 (邮箱或显示名称)")] string userId,
        [Description("项目名称或 ID（可选，不填则使用默认项目）")] string? projectId = null)
    {
        var effectiveProjectId = projectId ?? await GetDefaultProjectIdAsync();
        if (string.IsNullOrEmpty(effectiveProjectId))
        {
            throw new InvalidOperationException("未指定项目且没有设置默认项目映射");
        }
        return await _apiService.GetAssignedWorkItemsAsync(userId, effectiveProjectId);
    }

    /// <summary>
    /// 获取 WorkItem 详情
    /// </summary>
    [McpServerTool]
    [Description("根据 ID 获取 WorkItem 详细信息")]
    public async Task<TaskItem?> GetWorkItemDetails(
        [Description("WorkItem ID")] int workItemId)
    {
        return await _apiService.GetWorkItemDetailsAsync(workItemId);
    }

    /// <summary>
    /// 获取带代码资产关系的 WorkItem 详情
    /// </summary>
    [McpServerTool]
    [Description("根据 ID 获取 WorkItem 详情以及关联的 PR、Commit、Branch 信息")]
    public async Task<WorkItemWithRelations?> GetWorkItemDetailsWithRelations(
        [Description("WorkItem ID")] int workItemId)
    {
        return await _apiService.GetWorkItemDetailsWithRelationsAsync(workItemId);
    }

    /// <summary>
    /// 根据 WorkItem 关系推断当前用户关联的仓库映射
    /// </summary>
    [McpServerTool]
    [Description("根据 WorkItem 关联的 PR、Commit 或 Branch 自动推断当前用户的 Azure Repo 映射；无关系时返回当前用户默认 Repo 映射")]
    public async Task<RepositoryMapping?> AutoResolveRepositoryForWorkItem(
        [Description("WorkItem ID")] int workItemId)
    {
        var assignment = await GetWorkItemAssignment(workItemId);
        return assignment.RepositoryMapping;
    }

    /// <summary>
    /// 获取当前用户处理 WorkItem 所需的完整上下文。
    /// </summary>
    [McpServerTool]
    [Description("获取 WorkItem、父级 Issue/User Story、代码关系和当前用户本地 Repo 映射")]
    public async Task<WorkItemAssignment> GetWorkItemAssignment(
        [Description("WorkItem ID")] int workItemId)
    {
        var workItem = await _apiService.GetWorkItemDetailsWithRelationsAsync(workItemId)
            ?? throw new InvalidOperationException($"找不到 WorkItem: {workItemId}");

        WorkItemWithRelations? parentWorkItem = null;
        var parentId = workItem.Relations
            .FirstOrDefault(relation => relation.LinkType == WorkItemRelationLinkType.ParentWorkItem)
            ?.LinkedWorkItemId;

        if (int.TryParse(parentId, out var parsedParentId))
        {
            parentWorkItem = await _apiService.GetWorkItemDetailsWithRelationsAsync(parsedParentId);
        }

        var resolved = await ResolveRepositoryMappingAsync(workItem, parentWorkItem);

        return new WorkItemAssignment
        {
            WorkItem = workItem,
            ParentWorkItem = parentWorkItem,
            RepositoryMapping = resolved.Mapping,
            ResolutionSource = resolved.Source
        };
    }

    /// <summary>
    /// 更新 WorkItem 状态
    /// </summary>
    [McpServerTool]
    [Description("更新 WorkItem 的状态")]
    public async Task<TaskItem?> UpdateWorkItemState(
        [Description("WorkItem ID")] int workItemId,
        [Description("新状态 (New, Active, Resolved, Closed)")] string state)
    {
        return await _apiService.UpdateWorkItemStateAsync(workItemId, state);
    }

    /// <summary>
    /// 执行自定义 WIQL 查询（支持无参调用使用默认项目）
    /// </summary>
    [McpServerTool]
    [Description("执行自定义 WIQL 查询获取 WorkItem")]
    public async Task<IEnumerable<TaskItem>> QueryWorkItems(
        [Description("WIQL 查询语句")] string wiqlQuery,
        [Description("项目名称或 ID（可选，不填则使用默认项目）")] string? projectId = null)
    {
        var effectiveProjectId = projectId ?? await GetDefaultProjectIdAsync();
        if (string.IsNullOrEmpty(effectiveProjectId))
        {
            throw new InvalidOperationException("未指定项目且没有设置默认项目映射");
        }
        return await _apiService.QueryWorkItemsAsync(wiqlQuery, effectiveProjectId);
    }

    /// <summary>
    /// 给 WorkItem 添加 UI 可见评论（Discussion）
    /// </summary>
    [McpServerTool]
    [Description("给 WorkItem 添加 UI 可见评论（Discussion/Comments），不同于 System.History")]
    public async Task<CommentInfo> AddWorkItemComment(
        [Description("WorkItem ID")] int workItemId,
        [Description("评论内容，支持 HTML")]
        string text,
        [Description("项目名称（可选，不填则使用默认项目映射的项目名称）")]
        string? projectName = null)
    {
        var effectiveProjectName = projectName ?? await GetDefaultProjectNameAsync();
        if (string.IsNullOrEmpty(effectiveProjectName))
        {
            throw new InvalidOperationException("未指定项目且没有设置默认项目映射");
        }

        return await _apiService.AddWorkItemCommentAsync(workItemId, effectiveProjectName, text);
    }

    /// <summary>
    /// 获取默认项目 ID
    /// </summary>
    private async Task<string?> GetDefaultProjectIdAsync()
    {
        var defaultMapping = await _mappingService.GetDefaultMappingAsync();
        return defaultMapping?.AzureDevOpsProjectId;
    }

    /// <summary>
    /// 获取默认项目名称
    /// </summary>
    private async Task<string?> GetDefaultProjectNameAsync()
    {
        var defaultMapping = await _mappingService.GetDefaultMappingAsync();
        return defaultMapping?.AzureDevOpsProjectName;
    }

    private async Task<(RepositoryMapping? Mapping, RepositoryResolutionSource Source)> ResolveRepositoryMappingAsync(
        WorkItemWithRelations workItem,
        WorkItemWithRelations? parentWorkItem)
    {
        var windowsUsername = GetCurrentWindowsUsername();
        var mappings = await _repositoryMappingService.GetAllRepositoryMappingsAsync(windowsUsername);

        var workItemRepositoryId = GetFirstRepositoryId(workItem.Relations);
        var workItemMapping = FindMappingByRepositoryId(mappings, workItemRepositoryId);
        if (workItemMapping != null)
        {
            return (workItemMapping, RepositoryResolutionSource.WorkItemArtifactLink);
        }

        var parentRepositoryId = parentWorkItem == null ? null : GetFirstRepositoryId(parentWorkItem.Relations);
        var parentMapping = FindMappingByRepositoryId(mappings, parentRepositoryId);
        if (parentMapping != null)
        {
            return (parentMapping, RepositoryResolutionSource.ParentWorkItemArtifactLink);
        }

        var defaultMapping = await _repositoryMappingService.GetDefaultRepositoryMappingAsync(windowsUsername);
        return defaultMapping == null
            ? (null, RepositoryResolutionSource.Unresolved)
            : (defaultMapping, RepositoryResolutionSource.UserDefaultRepositoryMapping);
    }

    private static string? GetFirstRepositoryId(IEnumerable<WorkItemRelationInfo> relations)
    {
        return relations.FirstOrDefault(relation => !string.IsNullOrEmpty(relation.RepositoryId))?.RepositoryId;
    }

    private static RepositoryMapping? FindMappingByRepositoryId(IEnumerable<RepositoryMapping> mappings, string? repositoryId)
    {
        if (string.IsNullOrEmpty(repositoryId))
        {
            return null;
        }

        return mappings.FirstOrDefault(mapping =>
            mapping.RepositoryId.Equals(repositoryId, StringComparison.OrdinalIgnoreCase));
    }

    private string GetCurrentWindowsUsername()
    {
        return _userContext.CurrentWindowsUsername
            ?? throw new InvalidOperationException("无法获取当前 Windows 用户名");
    }
}
