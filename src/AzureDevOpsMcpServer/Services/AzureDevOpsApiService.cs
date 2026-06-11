using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AzureDevOpsMcpServer.Models;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// Azure DevOps 真实 API 服务接口
/// </summary>
public interface IAzureDevOpsApiService
{
    // WorkItem 操作
    Task<IEnumerable<TaskItem>> GetAssignedWorkItemsAsync(string userId, string projectId);
    Task<TaskItem?> GetWorkItemDetailsAsync(int workItemId);
    Task<WorkItemWithRelations?> GetWorkItemDetailsWithRelationsAsync(int workItemId);
    Task<TaskItem?> UpdateWorkItemStateAsync(int workItemId, string state);
    Task<IEnumerable<TaskItem>> QueryWorkItemsAsync(string wiqlQuery, string projectId);
    
    // WorkItem 评论操作
    Task<CommentInfo> AddWorkItemCommentAsync(int workItemId, string projectName, string text);
    
    // WorkItem 关联操作
    Task<bool> CreateArtifactLinkAsync(int workItemId, string artifactType, string artifactUrl);
    
    // TaskHistory 操作
    Task<IEnumerable<TaskStateHistory>> GetTaskHistoryAsync(int workItemId);
    
    // Project 操作
    Task<IEnumerable<Project>> GetProjectsAsync(string? userId = null);
    Task<Project?> GetProjectAsync(string projectId);
    
    // Repository 操作
    Task<IEnumerable<RepositoryInfo>> GetRepositoriesAsync(string projectId);
    Task<RepositoryInfo?> GetRepositoryAsync(string projectId, string repositoryId);
}

/// <summary>
/// 仓库信息模型
/// </summary>
public class RepositoryInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string WebUrl { get; set; } = string.Empty;
    public string RemoteUrl { get; set; } = string.Empty;
}

/// <summary>
/// WorkItem 评论信息
/// </summary>
public class CommentInfo
{
    public int Id { get; set; }
    public int Version { get; set; }
    public string Text { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Azure DevOps 真实 API 服务实现
/// </summary>
public class AzureDevOpsApiService : IAzureDevOpsApiService
{
    private readonly AzureDevOpsConnection _connection;
    private readonly HttpClient _httpClient;
    private WorkItemTrackingHttpClient? _witClient;
    private ProjectHttpClient? _projectClient;
    private GitHttpClient? _gitClient;

    public AzureDevOpsApiService(AzureDevOpsConnection connection)
    {
        _connection = connection;
        _httpClient = new HttpClient();

        var authBytes = Encoding.ASCII.GetBytes($":{connection.PersonalAccessToken}");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private WorkItemTrackingHttpClient GetWitClient()
    {
        _witClient ??= _connection.GetClient<WorkItemTrackingHttpClient>();
        return _witClient;
    }

    private ProjectHttpClient GetProjectClient()
    {
        _projectClient ??= _connection.GetClient<ProjectHttpClient>();
        return _projectClient;
    }

    private GitHttpClient GetGitClient()
    {
        _gitClient ??= _connection.GetClient<GitHttpClient>();
        return _gitClient;
    }

    /// <summary>
    /// 获取指派给用户的 WorkItem
    /// </summary>
    public async Task<IEnumerable<TaskItem>> GetAssignedWorkItemsAsync(string userId, string projectId)
    {
        var witClient = GetWitClient();
        
        // 构建 WIQL 查询
        var wiql = new Wiql
        {
            Query = $@"
                SELECT [System.Id], [System.Title], [System.State], [System.AssignedTo], 
                       [System.WorkItemType], [System.Description], [System.CreatedDate], 
                       [System.ChangedDate], [System.TeamProject]
                FROM WorkItems
                WHERE [System.AssignedTo] = '{userId}'
                  AND [System.TeamProject] = '{projectId}'
                  AND [System.State] <> 'Closed'
                  AND [System.State] <> 'Removed'
                ORDER BY [System.ChangedDate] DESC"
        };

        try
        {
            var result = await witClient.QueryByWiqlAsync(wiql, projectId);
            
            if (result.WorkItems == null || !result.WorkItems.Any())
            {
                return Enumerable.Empty<TaskItem>();
            }

            // 批量获取 WorkItem 详情
            var workItemIds = result.WorkItems.Select(wi => wi.Id).ToList();
            // 批量获取 WorkItem 详情，每次最多 200 个避免 URI 过长
            const int batchSize = 200;
            var allWorkItems = new List<WorkItem>();
            for (int i = 0; i < workItemIds.Count; i += batchSize)
            {
                var batch = workItemIds.Skip(i).Take(batchSize).ToList();
                var batchResult = await witClient.GetWorkItemsAsync(batch, null, result.AsOf);
                allWorkItems.AddRange(batchResult);
            }

            return allWorkItems.Select(ConvertToTaskItem);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取 WorkItem 失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取 WorkItem 详情
    /// </summary>
    public async Task<TaskItem?> GetWorkItemDetailsAsync(int workItemId)
    {
        var witClient = GetWitClient();
        
        try
        {
            var workItem = await witClient.GetWorkItemAsync(workItemId);
            return ConvertToTaskItem(workItem);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取 WorkItem 详情失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取带代码资产关系的 WorkItem 详情。
    /// </summary>
    public async Task<WorkItemWithRelations?> GetWorkItemDetailsWithRelationsAsync(int workItemId)
    {
        var witClient = GetWitClient();

        try
        {
            var workItem = await witClient.GetWorkItemAsync(workItemId, expand: WorkItemExpand.Relations);
            var taskItem = ConvertToTaskItem(workItem);
            var relations = workItem.Relations?
                .Select(relation => WorkItemRelationInfo.FromAzureDevOpsRelation(
                    relation.Rel,
                    relation.Attributes != null && relation.Attributes.TryGetValue("name", out var name)
                        ? name?.ToString() ?? string.Empty
                        : string.Empty,
                    relation.Url))
                .ToList() ?? new List<WorkItemRelationInfo>();

            return new WorkItemWithRelations
            {
                WorkItem = taskItem,
                Relations = relations
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取 WorkItem 关系失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 更新 WorkItem 状态
    /// </summary>
    public async Task<TaskItem?> UpdateWorkItemStateAsync(int workItemId, string state)
    {
        var witClient = GetWitClient();
        
        try
        {
            var patchDocument = new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument
            {
                new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Replace,
                    Path = "/fields/System.State",
                    Value = state
                }
            };

            var workItem = await witClient.UpdateWorkItemAsync(patchDocument, workItemId);
            return ConvertToTaskItem(workItem);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"更新 WorkItem 状态失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 给 WorkItem 添加 UI 可见的评论（Discussion）
    /// </summary>
    public async Task<CommentInfo> AddWorkItemCommentAsync(int workItemId, string projectName, string text)
    {
        try
        {
            var orgUrl = _connection.GetConnection().Uri.ToString().TrimEnd('/');
            var encodedProject = Uri.EscapeDataString(projectName);
            var url = $"{orgUrl}/{encodedProject}/_apis/wit/workItems/{workItemId}/comments?api-version=7.1-preview.4";

            var payload = JsonSerializer.Serialize(new { text });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            return new CommentInfo
            {
                Id = root.GetProperty("id").GetInt32(),
                Version = root.GetProperty("version").GetInt32(),
                Text = root.GetProperty("text").GetString() ?? string.Empty,
                CreatedBy = root.TryGetProperty("createdBy", out var createdBy)
                    ? createdBy.GetProperty("displayName").GetString() ?? string.Empty
                    : string.Empty,
                CreatedDate = root.TryGetProperty("createdDate", out var createdDate)
                    ? createdDate.GetDateTime()
                    : DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"添加 WorkItem 评论失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 为 WorkItem 创建 ArtifactLink 关联
    /// </summary>
    public async Task<bool> CreateArtifactLinkAsync(int workItemId, string artifactType, string artifactUrl)
    {
        try
        {
            var witClient = GetWitClient();
            
            var patchDocument = new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument
            {
                new Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchOperation
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = artifactType,
                        url = artifactUrl,
                        attributes = new
                        {
                            comment = "Linked from MCP Server"
                        }
                    }
                }
            };

            var workItem = await witClient.UpdateWorkItemAsync(patchDocument, workItemId);
            return workItem != null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"创建 ArtifactLink 失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 执行自定义 WIQL 查询
    /// </summary>
    public async Task<IEnumerable<TaskItem>> QueryWorkItemsAsync(string wiqlQuery, string projectId)
    {
        var witClient = GetWitClient();
        
        var wiql = new Wiql { Query = wiqlQuery };
        
        try
        {
            var result = await witClient.QueryByWiqlAsync(wiql, projectId);
            
            if (result.WorkItems == null || !result.WorkItems.Any())
            {
                return Enumerable.Empty<TaskItem>();
            }

            var workItemIds = result.WorkItems.Select(wi => wi.Id).ToList();
            // 批量获取 WorkItem 详情，每次最多 200 个避免 URI 过长
            const int batchSize = 200;
            var allWorkItems = new List<WorkItem>();
            for (int i = 0; i < workItemIds.Count; i += batchSize)
            {
                var batch = workItemIds.Skip(i).Take(batchSize).ToList();
                var batchResult = await witClient.GetWorkItemsAsync(batch, null, result.AsOf);
                allWorkItems.AddRange(batchResult);
            }

            return allWorkItems.Select(ConvertToTaskItem);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"查询 WorkItem 失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取 WorkItem 的状态变更历史
    /// </summary>
    public async Task<IEnumerable<TaskStateHistory>> GetTaskHistoryAsync(int workItemId)
    {
        var witClient = GetWitClient();
        
        try
        {
            // 获取工作项的所有修订版本（返回 List<WorkItem>）
            var revisions = await witClient.GetRevisionsAsync(workItemId);
            
            if (revisions == null || revisions.Count < 2)
            {
                return Enumerable.Empty<TaskStateHistory>();
            }

            var histories = new List<TaskStateHistory>();
            
            // 从修订版本中提取状态变更记录
            for (int i = 1; i < revisions.Count; i++)
            {
                var current = revisions[i];
                var previous = revisions[i - 1];
                
                var currentState = GetRevisionFieldValue(current, "System.State");
                var previousState = GetRevisionFieldValue(previous, "System.State");
                
                // 只记录状态变更
                if (!string.Equals(currentState, previousState, StringComparison.OrdinalIgnoreCase))
                {
                    histories.Add(new TaskStateHistory
                    {
                        Id = Guid.NewGuid(),
                        WorkItemId = workItemId.ToString(),
                        OldStatus = ConvertStateToStatus(previousState),
                        NewStatus = ConvertStateToStatus(currentState),
                        ChangedBy = GetRevisionFieldValue(current, "System.ChangedBy") ?? "Unknown",
                        ChangedAt = GetRevisionFieldDateTime(current, "System.ChangedDate")
                    });
                }
            }
            
            // 按时间倒序返回
            return histories.OrderByDescending(h => h.ChangedAt);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取 WorkItem 历史失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取所有项目（可选：按用户过滤）
    /// </summary>
    /// <param name="userId">可选的用户标识，用于过滤用户可访问的项目</param>
    public async Task<IEnumerable<Project>> GetProjectsAsync(string? userId = null)
    {
        var projectClient = GetProjectClient();
        
        try
        {
            var projects = await projectClient.GetProjects();
            var allProjects = projects.Select(p => ConvertToProject(p)).ToList();
            
            // 如果没有提供 userId，返回所有项目
            if (string.IsNullOrEmpty(userId))
            {
                return allProjects;
            }
            
            // 按用户过滤项目
            // 在实际生产环境中，应使用 Azure DevOps Security API 或 Graph API
            // 获取用户有权限访问的项目列表
            // 当前实现模拟用户过滤逻辑
            return FilterProjectsByUser(allProjects, userId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取项目列表失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 根据用户过滤项目
    /// 实际实现应调用 Azure DevOps 的权限检查 API
    /// </summary>
    private IEnumerable<Project> FilterProjectsByUser(IEnumerable<Project> projects, string userId)
    {
        // 模拟用户过滤逻辑
        // 在实际场景中，可以：
        // 1. 使用 SecurityNamespace API 检查用户对每个项目的权限
        // 2. 使用 Graph API 获取用户所属的团队/项目
        // 3. 使用 IdentityService 获取用户的项目权限
        
        // 当前实现：通过用户名特征进行过滤（演示用途）
        // 实际应替换为真实的权限检查逻辑
        return projects.Where(p => IsUserAuthorizedForProject(p, userId));
    }

    /// <summary>
    /// 检查用户是否有权访问项目
    /// </summary>
    private bool IsUserAuthorizedForProject(Project project, string userId)
    {
        // 模拟权限检查逻辑
        // 在实际生产环境中，应调用 Azure DevOps 的权限 API
        // 例如使用 SecurityHttpClient.CheckPermissionAsync
        
        // 演示：允许所有用户访问项目（实际应根据角色/权限判断）
        // 这里可以扩展为基于用户映射或角色的权限检查
        return true;
    }

    /// <summary>
    /// 获取单个项目
    /// </summary>
    public async Task<Project?> GetProjectAsync(string projectId)
    {
        var projectClient = GetProjectClient();
        
        try
        {
            var project = await projectClient.GetProject(projectId);
            return ConvertToProject(project);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取项目详情失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取项目下的所有仓库
    /// </summary>
    public async Task<IEnumerable<RepositoryInfo>> GetRepositoriesAsync(string projectId)
    {
        var gitClient = GetGitClient();
        
        try
        {
            var repos = await gitClient.GetRepositoriesAsync(projectId);
            return repos.Select(ConvertToRepositoryInfo);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取仓库列表失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取单个仓库
    /// </summary>
    public async Task<RepositoryInfo?> GetRepositoryAsync(string projectId, string repositoryId)
    {
        var gitClient = GetGitClient();
        
        try
        {
            var repo = await gitClient.GetRepositoryAsync(projectId, repositoryId);
            return ConvertToRepositoryInfo(repo);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"获取仓库详情失败: {ex.Message}", ex);
        }
    }

    #region 转换方法

    private TaskItem ConvertToTaskItem(WorkItem workItem)
    {
        return new TaskItem
        {
            Id = Guid.NewGuid(),
            AzureDevOpsId = workItem.Id.ToString(),
            Title = GetFieldValue(workItem, "System.Title") ?? string.Empty,
            Description = GetFieldValue(workItem, "System.Description") ?? string.Empty,
            AssignedTo = GetFieldValue(workItem, "System.AssignedTo") ?? string.Empty,
            ProjectId = GetFieldValue(workItem, "System.TeamProject") ?? string.Empty,
            ProjectName = GetFieldValue(workItem, "System.TeamProject") ?? string.Empty,
            Status = ConvertStateToStatus(GetFieldValue(workItem, "System.State")),
            CreatedAt = workItem.Fields.TryGetValue("System.CreatedDate", out var createdDate) 
                ? (DateTime)createdDate 
                : DateTime.UtcNow,
            UpdatedAt = workItem.Fields.TryGetValue("System.ChangedDate", out var changedDate) 
                ? (DateTime)changedDate 
                : DateTime.UtcNow
        };
    }

    private string? GetFieldValue(WorkItem workItem, string fieldName)
    {
        return workItem.Fields.TryGetValue(fieldName, out var value) ? value?.ToString() : null;
    }

    /// <summary>
    /// 从 WorkItem 获取指定字段的值（字符串版本）
    /// </summary>
    private string? GetRevisionFieldValue(WorkItem workItem, string fieldName)
    {
        return workItem.Fields.TryGetValue(fieldName, out var value) ? value?.ToString() : null;
    }

    /// <summary>
    /// 从 WorkItem 获取指定字段的值（DateTime版本）
    /// </summary>
    private DateTime GetRevisionFieldDateTime(WorkItem workItem, string fieldName)
    {
        if (workItem.Fields.TryGetValue(fieldName, out var value) && value is DateTime dateValue)
        {
            return dateValue;
        }
        return DateTime.UtcNow;
    }

    /// <summary>
    /// 将 Azure DevOps 状态转换为内部任务状态（基于 CONTEXT.md 术语）
    /// </summary>
    private Models.TaskStatus ConvertStateToStatus(string? state)
    {
        return StateMapper.ToInternalStatus(state);
    }

    private Project ConvertToProject(TeamProject project)
    {
        return new Project
        {
            Id = project.Id,
            AzureDevOpsId = project.Id.ToString(),
            Name = project.Name,
            Description = project.Description ?? string.Empty,
            Organization = _connection.GetConnection().Uri.Host,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private Project ConvertToProject(TeamProjectReference projectRef)
    {
        return new Project
        {
            Id = projectRef.Id,
            AzureDevOpsId = projectRef.Id.ToString(),
            Name = projectRef.Name,
            Description = string.Empty,
            Organization = _connection.GetConnection().Uri.Host,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private RepositoryInfo ConvertToRepositoryInfo(GitRepository repo)
    {
        return new RepositoryInfo
        {
            Id = repo.Id.ToString(),
            Name = repo.Name,
            ProjectId = repo.ProjectReference?.Id.ToString() ?? string.Empty,
            ProjectName = repo.ProjectReference?.Name ?? string.Empty,
            WebUrl = repo.WebUrl ?? string.Empty,
            RemoteUrl = repo.RemoteUrl ?? string.Empty
        };
    }

    #endregion
}
