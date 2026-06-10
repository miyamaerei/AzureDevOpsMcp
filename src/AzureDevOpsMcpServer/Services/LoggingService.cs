using AzureDevOpsMcpServer.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 日志服务
/// </summary>
public interface ILoggingService
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception ex, string context, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogTaskEvent(string taskId, string eventType, string details);
    void LogSyncEvent(string workItemId, string status, string details);
    
    /// <summary>
    /// 获取最近的日志条目
    /// </summary>
    /// <param name="limit">日志条数限制</param>
    /// <returns>日志条目列表</returns>
    List<LogEntry> GetRecentLogs(int limit = 100);
}

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly ConcurrentQueue<LogEntry> _recentLogs = new();
    private const int MaxLogs = 1000;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
        AddLog("Information", message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
        AddLog("Warning", message, args);
    }

    public void LogError(Exception ex, string context, params object[] args)
    {
        var errorData = new Dictionary<string, object>
        {
            { "Context", context },
            { "ExceptionType", ex.GetType().Name },
            { "Message", ex.Message },
            { "StackTrace", ex.StackTrace }
        };

        if (ex.InnerException != null)
        {
            errorData.Add("InnerException", new
            {
                Type = ex.InnerException.GetType().Name,
                Message = ex.InnerException.Message
            });
        }

        _logger.LogError(ex, "[{Context}] Error: {ErrorMessage}", context, ex.Message);
        AddLog("Error", $"[{context}] Error: {ex.Message}");
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
        AddLog("Debug", message, args);
    }

    public void LogTaskEvent(string taskId, string eventType, string details)
    {
        var eventData = new
        {
            TaskId = taskId,
            EventType = eventType,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("[TaskEvent] TaskId={TaskId}, EventType={EventType}, Details={Details}", 
            taskId, eventType, details);
        AddLog("Information", $"[TaskEvent] TaskId={taskId}, EventType={eventType}, Details={details}");
    }

    public void LogSyncEvent(string workItemId, string status, string details)
    {
        _logger.LogInformation("[SyncEvent] WorkItemId={WorkItemId}, Status={Status}, Details={Details}", 
            workItemId, status, details);
        AddLog("Information", $"[SyncEvent] WorkItemId={workItemId}, Status={status}, Details={details}");
    }

    public List<LogEntry> GetRecentLogs(int limit = 100)
    {
        return _recentLogs.ToArray().TakeLast(limit).ToList();
    }

    private void AddLog(string level, string message, params object[] args)
    {
        if (args != null && args.Length > 0)
        {
            try
            {
                message = string.Format(message, args);
            }
            catch
            {
                // 如果格式化失败，保留原始消息
            }
        }

        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Category = "AzureDevOpsMcpServer",
            Message = message
        };

        _recentLogs.Enqueue(entry);

        // 保持日志数量限制
        while (_recentLogs.Count > MaxLogs)
        {
            _recentLogs.TryDequeue(out _);
        }
    }
}
