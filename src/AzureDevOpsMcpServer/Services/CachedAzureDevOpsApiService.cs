using AzureDevOpsMcpServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 带缓存的 Azure DevOps API 服务
/// </summary>
public interface ICachedAzureDevOpsApiService : IAzureDevOpsApiService
{
    /// <summary>
    /// 刷新项目缓存
    /// </summary>
    Task RefreshProjectsCacheAsync(string? userId = null);

    /// <summary>
    /// 刷新任务缓存
    /// </summary>
    Task RefreshTasksCacheAsync(string userId, string? projectId = null);
}

public class CachedAzureDevOpsApiService : ICachedAzureDevOpsApiService
{
    private readonly IAzureDevOpsApiService _apiService;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _projectCacheExpiration = TimeSpan.FromMinutes(15);
    private readonly TimeSpan _taskCacheExpiration = TimeSpan.FromMinutes(5);

    public CachedAzureDevOpsApiService(IAzureDevOpsApiService apiService, ICacheService cacheService)
    {
        _apiService = apiService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// 获取指派给用户的任务列表（带缓存）
    /// </summary>
    public async Task<IEnumerable<TaskItem>> GetAssignedWorkItemsAsync(string userId, string projectId)
    {
        var cacheKey = $"assignedTasks:{userId}:{projectId}";
        
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _apiService.GetAssignedWorkItemsAsync(userId, projectId);
        }, _taskCacheExpiration);
    }

    /// <summary>
    /// 获取任务详情（带缓存）
    /// </summary>
    public async Task<TaskItem?> GetWorkItemDetailsAsync(int workItemId)
    {
        var cacheKey = string.Format(CacheKeys.Task, workItemId);
        
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _apiService.GetWorkItemDetailsAsync(workItemId);
        }, _taskCacheExpiration);
    }

    /// <summary>
    /// 更新任务状态（清除缓存）
    /// </summary>
    public async Task<TaskItem?> UpdateWorkItemStateAsync(int workItemId, string state)
    {
        // 清除相关缓存
        var cacheKey = string.Format(CacheKeys.Task, workItemId);
        _cacheService.Remove(cacheKey);
        
        return await _apiService.UpdateWorkItemStateAsync(workItemId, state);
    }

    /// <summary>
    /// 查询任务（带缓存）
    /// </summary>
    public async Task<IEnumerable<TaskItem>> QueryWorkItemsAsync(string wiqlQuery, string projectId)
    {
        var cacheKey = $"queryTasks:{projectId}:{wiqlQuery.GetHashCode()}";
        
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _apiService.QueryWorkItemsAsync(wiqlQuery, projectId);
        }, _taskCacheExpiration);
    }

    /// <summary>
    /// 获取任务历史（带缓存）
    /// </summary>
    public async Task<IEnumerable<TaskStateHistory>> GetTaskHistoryAsync(int workItemId)
    {
        var cacheKey = $"taskHistory:{workItemId}";
        
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _apiService.GetTaskHistoryAsync(workItemId);
        }, _taskCacheExpiration);
    }

    /// <summary>
    /// 获取项目列表（带缓存）
    /// </summary>
    public async Task<IEnumerable<Project>> GetProjectsAsync(string? userId = null)
    {
        var cacheKey = CacheKeys.FormatProjectCacheKey(userId);
        
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _apiService.GetProjectsAsync(userId);
        }, _projectCacheExpiration);
    }

    /// <summary>
    /// 获取项目详情（带缓存）
    /// </summary>
    public async Task<Project?> GetProjectAsync(string projectId)
    {
        var cacheKey = string.Format(CacheKeys.Project, projectId);
        
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _apiService.GetProjectAsync(projectId);
        }, _projectCacheExpiration);
    }

    /// <summary>
    /// 获取仓库列表（带缓存）
    /// </summary>
    public async Task<IEnumerable<RepositoryInfo>> GetRepositoriesAsync(string projectId)
    {
        var cacheKey = string.Format(CacheKeys.Repositories, projectId);
        
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _apiService.GetRepositoriesAsync(projectId);
        }, _projectCacheExpiration);
    }

    /// <summary>
    /// 获取仓库详情（带缓存）
    /// </summary>
    public async Task<RepositoryInfo?> GetRepositoryAsync(string projectId, string repositoryId)
    {
        var cacheKey = string.Format(CacheKeys.Repository, projectId, repositoryId);
        
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _apiService.GetRepositoryAsync(projectId, repositoryId);
        }, _projectCacheExpiration);
    }

    /// <summary>
    /// 刷新项目缓存
    /// </summary>
    public async Task RefreshProjectsCacheAsync(string? userId = null)
    {
        var cacheKey = CacheKeys.FormatProjectCacheKey(userId);
        _cacheService.Remove(cacheKey);
        
        // 预加载缓存
        await GetProjectsAsync(userId);
    }

    /// <summary>
    /// 刷新任务缓存
    /// </summary>
    public async Task RefreshTasksCacheAsync(string userId, string? projectId = null)
    {
        if (projectId != null)
        {
            var cacheKey = $"assignedTasks:{userId}:{projectId}";
            _cacheService.Remove(cacheKey);
        }
        else
        {
            var cacheKey = CacheKeys.FormatTaskCacheKey(userId);
            _cacheService.Remove(cacheKey);
        }
    }
}