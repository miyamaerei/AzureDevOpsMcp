using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 性能监控服务接口
/// </summary>
public interface IPerformanceMonitorService
{
    /// <summary>
    /// 开始性能追踪
    /// </summary>
    IDisposable TrackOperation(string operationName);

    /// <summary>
    /// 记录操作耗时
    /// </summary>
    void RecordOperationTime(string operationName, TimeSpan duration);

    /// <summary>
    /// 获取性能统计
    /// </summary>
    PerformanceStats GetStats();

    /// <summary>
    /// 重置统计数据
    /// </summary>
    void ResetStats();
}

/// <summary>
/// 性能统计数据
/// </summary>
public class PerformanceStats
{
    public long TotalOperations { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public ConcurrentDictionary<string, OperationStats> OperationStats { get; } = new();
}

/// <summary>
/// 单个操作的统计数据
/// </summary>
public class OperationStats
{
    public long Count { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public TimeSpan MinDuration { get; set; }

    public TimeSpan AverageDuration => Count > 0 ? TimeSpan.FromTicks(TotalDuration.Ticks / Count) : TimeSpan.Zero;
}

/// <summary>
/// 性能监控服务实现
/// </summary>
public class PerformanceMonitorService : IPerformanceMonitorService
{
    private readonly ILogger<PerformanceMonitorService> _logger;
    private readonly PerformanceStats _stats = new();
    private readonly object _lock = new();

    public PerformanceMonitorService(ILogger<PerformanceMonitorService> logger)
    {
        _logger = logger;
    }

    public IDisposable TrackOperation(string operationName)
    {
        return new OperationTracker(this, operationName);
    }

    public void RecordOperationTime(string operationName, TimeSpan duration)
    {
        lock (_lock)
        {
            _stats.TotalOperations++;

            if (!_stats.OperationStats.TryGetValue(operationName, out var operationStats))
            {
                operationStats = new OperationStats
                {
                    MinDuration = duration
                };
                _stats.OperationStats[operationName] = operationStats;
            }
            else
            {
                if (duration < operationStats.MinDuration)
                    operationStats.MinDuration = duration;
            }

            operationStats.Count++;
            operationStats.TotalDuration += duration;
            
            if (duration > operationStats.MaxDuration)
                operationStats.MaxDuration = duration;

            if (_stats.MinDuration == TimeSpan.Zero || duration < _stats.MinDuration)
                _stats.MinDuration = duration;

            if (duration > _stats.MaxDuration)
                _stats.MaxDuration = duration;

            _stats.AverageDuration = TimeSpan.FromTicks(
                _stats.OperationStats.Sum(x => x.Value.TotalDuration.Ticks) / _stats.TotalOperations);

            // 记录慢操作日志
            if (duration > TimeSpan.FromSeconds(5))
            {
                _logger.LogWarning("[Performance] Slow operation detected: {OperationName} took {Duration}", 
                    operationName, duration);
            }
        }
    }

    public PerformanceStats GetStats()
    {
        return _stats;
    }

    public void ResetStats()
    {
        lock (_lock)
        {
            _stats.TotalOperations = 0;
            _stats.AverageDuration = TimeSpan.Zero;
            _stats.MaxDuration = TimeSpan.Zero;
            _stats.MinDuration = TimeSpan.Zero;
            _stats.OperationStats.Clear();
        }
    }

    private class OperationTracker : IDisposable
    {
        private readonly PerformanceMonitorService _monitor;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;

        public OperationTracker(PerformanceMonitorService monitor, string operationName)
        {
            _monitor = monitor;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _monitor.RecordOperationTime(_operationName, _stopwatch.Elapsed);
        }
    }
}