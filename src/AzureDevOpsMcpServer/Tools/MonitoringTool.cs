using AzureDevOpsMcpServer.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsMcpServer.Tools;

/// <summary>
/// 监控工具类，提供健康检查、日志收集和性能监控功能
/// </summary>
public class MonitoringTool
{
    private readonly IPerformanceMonitorService _performanceMonitor;
    private readonly ILoggingService _loggingService;
    private readonly ILogger<MonitoringTool> _logger;

    public MonitoringTool(
        IPerformanceMonitorService performanceMonitor,
        ILoggingService loggingService,
        ILogger<MonitoringTool> logger)
    {
        _performanceMonitor = performanceMonitor;
        _loggingService = loggingService;
        _logger = logger;
    }

    /// <summary>
    /// 获取健康检查状态
    /// </summary>
    /// <returns>健康状态信息</returns>
    public HealthCheckResult GetHealthStatus()
    {
        _logger.LogInformation("Health check requested");
        
        var stats = _performanceMonitor.GetStats();
        
        return new HealthCheckResult
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            TotalOperations = stats.TotalOperations,
            AverageResponseTime = stats.AverageDuration.TotalMilliseconds,
            MaxResponseTime = stats.MaxDuration.TotalMilliseconds,
            MinResponseTime = stats.MinDuration.TotalMilliseconds,
            ActiveConnections = 0,
            MemoryUsage = GetMemoryUsage(),
            CpuUsage = GetCpuUsage()
        };
    }

    /// <summary>
    /// 获取性能统计信息
    /// </summary>
    /// <returns>性能统计数据</returns>
    public PerformanceMetrics GetPerformanceMetrics()
    {
        _logger.LogInformation("Performance metrics requested");
        
        var stats = _performanceMonitor.GetStats();
        
        return new PerformanceMetrics
        {
            TotalOperations = stats.TotalOperations,
            AverageDuration = stats.AverageDuration,
            MaxDuration = stats.MaxDuration,
            MinDuration = stats.MinDuration,
            OperationDetails = stats.OperationStats.ToDictionary(
                kvp => kvp.Key,
                kvp => new OperationMetric
                {
                    Count = kvp.Value.Count,
                    TotalDuration = kvp.Value.TotalDuration,
                    AverageDuration = kvp.Value.AverageDuration,
                    MaxDuration = kvp.Value.MaxDuration,
                    MinDuration = kvp.Value.MinDuration
                })
        };
    }

    /// <summary>
    /// 获取最近的日志
    /// </summary>
    /// <param name="limit">日志条数限制，默认100</param>
    /// <returns>日志列表</returns>
    public List<LogEntry> GetRecentLogs(int limit = 100)
    {
        _logger.LogInformation("Logs requested with limit: {Limit}", limit);
        
        return _loggingService.GetRecentLogs(limit);
    }

    /// <summary>
    /// 重置性能统计数据
    /// </summary>
    public void ResetPerformanceStats()
    {
        _logger.LogInformation("Performance stats reset requested");
        _performanceMonitor.ResetStats();
    }

    private double GetMemoryUsage()
    {
        var process = System.Diagnostics.Process.GetCurrentProcess();
        return Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2);
    }

    private double GetCpuUsage()
    {
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var totalTime = DateTime.Now.Subtract(process.StartTime).TotalMilliseconds;
        if (totalTime <= 0) return 0;
        return Math.Round(process.TotalProcessorTime.TotalMilliseconds / totalTime * 100, 2);
    }
}

/// <summary>
/// 健康检查结果
/// </summary>
public class HealthCheckResult
{
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }
    public long TotalOperations { get; set; }
    public double AverageResponseTime { get; set; }
    public double MaxResponseTime { get; set; }
    public double MinResponseTime { get; set; }
    public int ActiveConnections { get; set; }
    public double MemoryUsage { get; set; }
    public double CpuUsage { get; set; }
}

/// <summary>
/// 性能指标
/// </summary>
public class PerformanceMetrics
{
    public long TotalOperations { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public Dictionary<string, OperationMetric> OperationDetails { get; set; } = new();
}

/// <summary>
/// 操作指标
/// </summary>
public class OperationMetric
{
    public long Count { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
}

/// <summary>
/// 日志条目
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Category { get; set; }
    public string Message { get; set; }
}
