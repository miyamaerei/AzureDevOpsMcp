using AzureDevOpsMcpServer.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 任务同步服务接口
/// </summary>
public interface ITaskSyncService
{
    /// <summary>
    /// 同步单个任务到 Azure DevOps
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>同步是否成功</returns>
    Task<bool> SyncTaskToAzureDevOpsAsync(Guid taskId);

    /// <summary>
    /// 获取任务的同步历史
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <returns>同步记录列表</returns>
    Task<IEnumerable<TaskSyncRecord>> GetSyncHistoryAsync(Guid taskId);

    /// <summary>
    /// 同步所有待同步的任务
    /// </summary>
    /// <returns>同步是否成功</returns>
    Task<bool> SyncAllPendingTasksAsync();

    /// <summary>
    /// 根据 WorkItemId 同步任务
    /// </summary>
    /// <param name="workItemId">Azure DevOps WorkItem ID</param>
    /// <returns>同步是否成功</returns>
    Task<bool> SyncTaskByWorkItemIdAsync(string workItemId);
}

/// <summary>
/// 任务同步服务实现
/// </summary>
public class TaskSyncService : ITaskSyncService
{
    private readonly AppDbContext _dbContext;
    private readonly IAzureDevOpsApiService _azureDevOpsApiService;

    public TaskSyncService(AppDbContext dbContext, IAzureDevOpsApiService azureDevOpsApiService)
    {
        _dbContext = dbContext;
        _azureDevOpsApiService = azureDevOpsApiService;
    }

    /// <summary>
    /// 同步单个任务到 Azure DevOps
    /// </summary>
    public async Task<bool> SyncTaskToAzureDevOpsAsync(Guid taskId)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null)
        {
            return false;
        }

        return await SyncTaskInternalAsync(task);
    }

    /// <summary>
    /// 根据 WorkItemId 同步任务
    /// </summary>
    public async Task<bool> SyncTaskByWorkItemIdAsync(string workItemId)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.AzureDevOpsId == workItemId);
        if (task == null)
        {
            return false;
        }

        return await SyncTaskInternalAsync(task);
    }

    /// <summary>
    /// 获取任务的同步历史
    /// </summary>
    public async Task<IEnumerable<TaskSyncRecord>> GetSyncHistoryAsync(Guid taskId)
    {
        return await _dbContext.TaskSyncRecords
            .Where(r => r.TaskId == taskId)
            .OrderByDescending(r => r.SyncedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 同步所有待同步的任务（状态为归档的任务）
    /// </summary>
    public async Task<bool> SyncAllPendingTasksAsync()
    {
        var pendingTasks = await _dbContext.Tasks
            .Where(t => t.Status == Models.TaskStatus.Archived)
            .ToListAsync();

        bool allSuccess = true;
        foreach (var task in pendingTasks)
        {
            bool success = await SyncTaskInternalAsync(task);
            if (!success)
            {
                allSuccess = false;
            }
        }

        return allSuccess;
    }

    /// <summary>
    /// 内部同步逻辑
    /// </summary>
    private async Task<bool> SyncTaskInternalAsync(TaskItem task)
    {
        var syncRecord = new TaskSyncRecord
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            WorkItemId = task.AzureDevOpsId,
            InternalStatus = task.Status,
            AzureDevOpsState = ConvertToAzureDevOpsState(task.Status),
            SyncedAt = DateTime.UtcNow,
            AttemptCount = 1
        };

        try
        {
            // 获取上次同步记录，确定重试次数
            var lastRecord = await _dbContext.TaskSyncRecords
                .Where(r => r.TaskId == task.Id)
                .OrderByDescending(r => r.SyncedAt)
                .FirstOrDefaultAsync();

            if (lastRecord != null)
            {
                syncRecord.AttemptCount = lastRecord.AttemptCount + 1;
            }

            // 解析 WorkItemId
            if (!int.TryParse(task.AzureDevOpsId, out int workItemId))
            {
                throw new InvalidOperationException("无效的 WorkItem ID");
            }

            // 调用 Azure DevOps API 更新状态
            var result = await _azureDevOpsApiService.UpdateWorkItemStateAsync(workItemId, syncRecord.AzureDevOpsState);
            
            if (result != null)
            {
                syncRecord.SyncSuccess = true;
            }
            else
            {
                syncRecord.SyncSuccess = false;
                syncRecord.ErrorMessage = "更新返回空结果";
            }
        }
        catch (Exception ex)
        {
            syncRecord.SyncSuccess = false;
            syncRecord.ErrorMessage = ex.Message;
        }

        await _dbContext.TaskSyncRecords.AddAsync(syncRecord);
        await _dbContext.SaveChangesAsync();

        return syncRecord.SyncSuccess;
    }

    /// <summary>
    /// 将内部状态转换为 Azure DevOps 状态（基于 CONTEXT.md 术语）
    /// </summary>
    private string ConvertToAzureDevOpsState(Models.TaskStatus status)
    {
        return StateMapper.ToAzureDevOpsState(status);
    }
}