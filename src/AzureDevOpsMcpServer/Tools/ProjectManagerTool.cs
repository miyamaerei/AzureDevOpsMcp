using AzureDevOpsMcpServer.Models;
using AzureDevOpsMcpServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TaskStatusEnum = AzureDevOpsMcpServer.Models.TaskStatus;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// 项目经理工具类，提供任务状态仪表板、阻塞任务高亮和使用情况统计功能
/// </summary>
public class ProjectManagerTool
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProjectManagerTool> _logger;

    public ProjectManagerTool(
        AppDbContext dbContext,
        ILogger<ProjectManagerTool> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 获取任务状态仪表板
    /// </summary>
    /// <param name="projectId">项目ID（可选）</param>
    /// <returns>任务状态仪表板数据</returns>
    public TaskStatusDashboard GetTaskStatusDashboard(string? projectId = null)
    {
        _logger.LogInformation("Getting task status dashboard for project: {ProjectId}", projectId ?? "all");

        var query = _dbContext.Tasks.AsQueryable();

        if (!string.IsNullOrEmpty(projectId))
        {
            query = query.Where(t => t.ProjectId == projectId);
        }

        var tasks = query.ToList();

        var dashboard = new TaskStatusDashboard
        {
            TotalTasks = tasks.Count,
            StatusDistribution = tasks
                .GroupBy(t => t.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            PriorityDistribution = tasks
                .GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key, g => g.Count()),
            BlockedTasksCount = tasks.Count(t => t.IsBlocked),
            CompletedTasksCount = tasks.Count(t => t.Status == TaskStatusEnum.Archived),
            AverageCompletionTime = CalculateAverageCompletionTime(tasks),
            LastUpdated = DateTime.UtcNow
        };

        return dashboard;
    }

    /// <summary>
    /// 获取阻塞任务列表
    /// </summary>
    /// <param name="projectId">项目ID（可选）</param>
    /// <returns>阻塞任务列表</returns>
    public List<BlockedTaskInfo> GetBlockedTasks(string? projectId = null)
    {
        _logger.LogInformation("Getting blocked tasks for project: {ProjectId}", projectId ?? "all");

        var query = _dbContext.Tasks.Where(t => t.IsBlocked);

        if (!string.IsNullOrEmpty(projectId))
        {
            query = query.Where(t => t.ProjectId == projectId);
        }

        var blockedTasks = query.ToList();

        return blockedTasks.Select(t => new BlockedTaskInfo
        {
            TaskId = t.AzureDevOpsId,
            Title = t.Title,
            Status = t.Status.ToString(),
            BlockedReason = t.BlockedReason ?? "Unknown",
            BlockedSince = t.BlockedSince ?? DateTime.UtcNow,
            DaysBlocked = (DateTime.UtcNow - (t.BlockedSince ?? DateTime.UtcNow)).Days,
            Priority = t.Priority,
            Assignee = t.AssignedTo
        }).OrderByDescending(t => t.DaysBlocked).ToList();
    }

    /// <summary>
    /// 获取使用情况统计
    /// </summary>
    /// <param name="startDate">开始日期（可选）</param>
    /// <param name="endDate">结束日期（可选）</param>
    /// <returns>使用情况统计数据</returns>
    public UsageStatistics GetUsageStatistics(DateTime? startDate = null, DateTime? endDate = null)
    {
        _logger.LogInformation("Getting usage statistics from {StartDate} to {EndDate}", 
            startDate?.ToString("yyyy-MM-dd") ?? "beginning",
            endDate?.ToString("yyyy-MM-dd") ?? "now");

        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var stats = new UsageStatistics
        {
            PeriodStart = start,
            PeriodEnd = end,
            TotalOperations = _dbContext.TaskSyncRecords
                .Count(r => r.SyncedAt >= start && r.SyncedAt <= end),
            ActiveProjects = _dbContext.Projects
                .Where(p => p.LastUsed >= start && p.LastUsed <= end)
                .Count(),
            ActiveUsers = _dbContext.UserMappings
                .Where(u => u.LastUsed >= start && u.LastUsed <= end)
                .Count(),
            TasksCreated = _dbContext.Tasks
                .Count(t => t.CreatedAt >= start && t.CreatedAt <= end),
            TasksCompleted = _dbContext.Tasks
                .Count(t => t.Status == TaskStatusEnum.Archived && t.UpdatedAt >= start && t.UpdatedAt <= end),
            AverageTasksPerDay = CalculateAverageTasksPerDay(start, end),
            TopActiveProjects = GetTopActiveProjects(start, end, 5)
        };

        return stats;
    }

    private TimeSpan CalculateAverageCompletionTime(List<TaskItem> tasks)
    {
        var completedTasks = tasks
            .Where(t => t.Status == TaskStatusEnum.Archived)
            .ToList();

        if (!completedTasks.Any())
            return TimeSpan.Zero;

        var totalDuration = completedTasks
            .Sum(t => (t.UpdatedAt - t.CreatedAt).TotalHours);

        return TimeSpan.FromHours(totalDuration / completedTasks.Count);
    }

    private double CalculateAverageTasksPerDay(DateTime start, DateTime end)
    {
        var totalDays = Math.Max(1, (end - start).TotalDays);
        var tasksCreated = _dbContext.Tasks
            .Count(t => t.CreatedAt >= start && t.CreatedAt <= end);

        return Math.Round(tasksCreated / totalDays, 2);
    }

    private List<ProjectUsageInfo> GetTopActiveProjects(DateTime start, DateTime end, int topN)
    {
        return _dbContext.Projects
            .Where(p => p.LastUsed >= start && p.LastUsed <= end)
            .OrderByDescending(p => p.LastUsed)
            .Take(topN)
            .Select(p => new ProjectUsageInfo
            {
                ProjectId = p.AzureDevOpsId,
                ProjectName = p.Name,
                LastUsed = p.LastUsed ?? DateTime.UtcNow,
                TaskCount = _dbContext.Tasks.Count(t => t.ProjectId == p.AzureDevOpsId)
            })
            .ToList();
    }
}

/// <summary>
/// 任务状态仪表板
/// </summary>
public class TaskStatusDashboard
{
    public int TotalTasks { get; set; }
    public Dictionary<string, int> StatusDistribution { get; set; } = new();
    public Dictionary<string, int> PriorityDistribution { get; set; } = new();
    public int BlockedTasksCount { get; set; }
    public int CompletedTasksCount { get; set; }
    public TimeSpan AverageCompletionTime { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 阻塞任务信息
/// </summary>
public class BlockedTaskInfo
{
    public string TaskId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string BlockedReason { get; set; } = string.Empty;
    public DateTime BlockedSince { get; set; }
    public int DaysBlocked { get; set; }
    public string Priority { get; set; } = "Medium";
    public string? Assignee { get; set; }
}

/// <summary>
/// 使用情况统计
/// </summary>
public class UsageStatistics
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalOperations { get; set; }
    public int ActiveProjects { get; set; }
    public int ActiveUsers { get; set; }
    public int TasksCreated { get; set; }
    public int TasksCompleted { get; set; }
    public double AverageTasksPerDay { get; set; }
    public List<ProjectUsageInfo> TopActiveProjects { get; set; } = new();
}

/// <summary>
/// 项目使用信息
/// </summary>
public class ProjectUsageInfo
{
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public DateTime LastUsed { get; set; }
    public int TaskCount { get; set; }
}