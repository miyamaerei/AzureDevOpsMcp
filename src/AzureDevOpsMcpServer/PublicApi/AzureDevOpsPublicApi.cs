using System.ComponentModel;
using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcpServer.PublicApi;

/// <summary>
/// Azure DevOps MCP 公共 API
/// 提供与 Azure DevOps 集成的核心对外接口
/// </summary>
public class AzureDevOpsPublicApi
{
    private readonly RepositoryMappingService _repositoryMappingService;
    private readonly IUserContext _userContext;
    private readonly IAzureDevOpsApiService _apiService;
    private readonly ITaskSyncService _taskSyncService;

    public AzureDevOpsPublicApi(
        RepositoryMappingService repositoryMappingService,
        IUserContext userContext,
        IAzureDevOpsApiService apiService,
        ITaskSyncService taskSyncService)
    {
        _repositoryMappingService = repositoryMappingService;
        _userContext = userContext;
        _apiService = apiService;
        _taskSyncService = taskSyncService;
    }

    /// <summary>
    /// 设置仓库映射关系
    /// </summary>
    [McpServerTool]
    [Description("配置本地 Git 仓库与 Azure DevOps Project 的映射关系")]
    public async Task<RepositoryMapping> SetRepositoryMapping(
        [Description("本地项目/仓库名称")] string localProject,
        [Description("Azure DevOps Project ID")] string azureProjectId,
        [Description("Azure DevOps Project 名称")] string azureProjectName,
        [Description("仓库 ID")] string repositoryId,
        [Description("仓库名称")] string repositoryName,
        [Description("仓库远程地址")] string remoteUrl,
        [Description("组织名称")] string organization,
        [Description("本地 Git 仓库工作目录")] string workingDirectory,
        [Description("是否设为默认映射")] bool isDefault = false,
        [Description("仓库提供方，例如 GitHub 或 AzureRepos")] string repositoryProvider = "AzureRepos",
        [Description("仓库所有者；GitHub 场景为 owner/organization")] string? repositoryOwner = null)
    {
        var windowsUsername = _userContext.CurrentWindowsUsername
            ?? throw new InvalidOperationException("无法获取当前 Windows 用户名");

        var azureDevOpsUser = await _userContext.GetCurrentAzureDevOpsUserAsync()
            ?? throw new InvalidOperationException("无法获取当前 Azure DevOps 用户");

        // 验证 Azure DevOps Project 是否存在
        var project = await _apiService.GetProjectAsync(azureProjectId);
        if (project == null)
        {
            throw new InvalidOperationException($"Azure DevOps Project '{azureProjectId}' 不存在或无权访问");
        }

        // 验证 Azure DevOps Repository 是否存在
        var repo = await _apiService.GetRepositoryAsync(azureProjectId, repositoryId);
        if (repo == null)
        {
            throw new InvalidOperationException($"Azure DevOps Repository '{repositoryId}' 在项目 '{azureProjectId}' 中不存在");
        }

        var mapping = await _repositoryMappingService.CreateOrUpdateRepositoryMappingAsync(
            windowsUsername: windowsUsername,
            azureDevOpsUser: azureDevOpsUser,
            localProjectName: localProject,
            workingDirectory: workingDirectory,
            azureDevOpsProjectId: azureProjectId,
            azureDevOpsProjectName: azureProjectName,
            repositoryId: repositoryId,
            repositoryName: repositoryName,
            remoteUrl: remoteUrl,
            organization: organization,
            isDefault: isDefault,
            machineName: Environment.MachineName,
            repositoryProvider: repositoryProvider,
            repositoryOwner: repositoryOwner ?? organization);

        // 尝试从分支名称中提取 WorkItem ID 并自动关联
        await TryAutoLinkWorkItems(workingDirectory, azureProjectId, repositoryName, remoteUrl);

        return mapping;
    }

    /// <summary>
    /// 尝试从本地分支名称提取 WorkItem ID 并自动创建关联
    /// </summary>
    private async Task TryAutoLinkWorkItems(string workingDirectory, string azureProjectId, string repositoryName, string remoteUrl)
    {
        try
        {
            // 简单实现：从工作目录路径或分支名称中提取 WorkItem ID
            // 实际实现可以扫描本地 Git 仓库获取当前分支
            var workItemId = ExtractWorkItemIdFromPath(workingDirectory);
            if (workItemId > 0)
            {
                // 创建 ArtifactLink 关联
                var artifactType = "ArtifactLink";
                var artifactUrl = $"{remoteUrl}/_git/{repositoryName}";
                
                await _apiService.CreateArtifactLinkAsync(workItemId, artifactType, artifactUrl);
            }
        }
        catch (Exception)
        {
            // 自动关联失败不影响映射设置
        }
    }

    /// <summary>
    /// 从路径中提取 WorkItem ID（简单实现）
    /// </summary>
    private int ExtractWorkItemIdFromPath(string path)
    {
        // 尝试从路径中查找数字模式（如 feature/task-123 或 bugfix/456-issue）
        var match = System.Text.RegularExpressions.Regex.Match(path, @"(?:task|bug|issue)-?(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var id))
        {
            return id;
        }
        return 0;
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
        var userId = await _userContext.GetCurrentAzureDevOpsUserAsync()
            ?? throw new InvalidOperationException("无法获取当前用户");

        var mapping = await _repositoryMappingService.GetDefaultRepositoryMappingAsync(GetCurrentWindowsUsername())
            ?? throw new InvalidOperationException("没有设置默认仓库映射");

        return await GetAssignedWorkItemsForRepositoryMapping(userId, mapping, includeUnresolved);
    }

    /// <summary>
    /// 同步指定任务到 Azure DevOps
    /// </summary>
    [McpServerTool]
    [Description("将指定任务同步到 Azure DevOps")]
    public async Task<SyncResult> SyncTaskToAzureDevOps(
        [Description("任务 ID")] Guid taskId)
    {
        bool success = await _taskSyncService.SyncTaskToAzureDevOpsAsync(taskId);

        return new SyncResult
        {
            Success = success,
            Message = success ? "同步成功" : "同步失败"
        };
    }

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

    private static IEnumerable<WorkItemRelationInfo> GetMatchingRepositoryRelations(
        IEnumerable<WorkItemRelationInfo> relations,
        RepositoryMapping mapping)
    {
        return relations.Where(relation => MatchesRepository(relation, mapping));
    }

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

    private string GetCurrentWindowsUsername()
    {
        return _userContext.CurrentWindowsUsername
            ?? throw new InvalidOperationException("无法获取当前 Windows 用户名");
    }
}

/// <summary>
/// 同步结果
/// </summary>
public class SyncResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 结果消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}