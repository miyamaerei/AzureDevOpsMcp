using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 任务同步后台服务
/// 支持定时同步和自动触发同步
/// </summary>
public class TaskSyncBackgroundService : BackgroundService
{
    private readonly ILogger<TaskSyncBackgroundService> _logger;
    private readonly ITaskSyncService _taskSyncService;
    private readonly TimeSpan _syncInterval;
    private readonly bool _autoSyncOnArchive;

    public TaskSyncBackgroundService(
        ILogger<TaskSyncBackgroundService> logger,
        ITaskSyncService taskSyncService)
    {
        _logger = logger;
        _taskSyncService = taskSyncService;
        
        // 从环境变量读取配置
        _syncInterval = TimeSpan.FromMinutes(
            int.TryParse(Environment.GetEnvironmentVariable("TASK_SYNC_INTERVAL_MINUTES"), out var interval) 
                ? interval 
                : 5);
        
        _autoSyncOnArchive = Environment.GetEnvironmentVariable("TASK_SYNC_AUTO_ON_ARCHIVE") != "false";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("任务同步后台服务启动，同步间隔: {Interval}", _syncInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("执行定时同步任务");
                await _taskSyncService.SyncAllPendingTasksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定时同步任务执行失败");
            }

            await Task.Delay(_syncInterval, stoppingToken);
        }

        _logger.LogInformation("任务同步后台服务停止");
    }

    /// <summary>
    /// 当任务状态变为归档时触发同步
    /// </summary>
    public async Task OnTaskArchivedAsync(Guid taskId)
    {
        if (!_autoSyncOnArchive)
        {
            _logger.LogDebug("自动同步已禁用，跳过任务 {TaskId} 的同步", taskId);
            return;
        }

        try
        {
            _logger.LogInformation("任务 {TaskId} 已归档，触发同步", taskId);
            bool success = await _taskSyncService.SyncTaskToAzureDevOpsAsync(taskId);
            
            if (success)
            {
                _logger.LogInformation("任务 {TaskId} 同步成功", taskId);
            }
            else
            {
                _logger.LogWarning("任务 {TaskId} 同步失败", taskId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "任务 {TaskId} 同步触发失败", taskId);
        }
    }
}