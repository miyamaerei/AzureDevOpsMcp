using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AzureDevOpsMcpServer.Services;

/// <summary>
/// 后台任务队列接口
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// 入队任务
    /// </summary>
    void Enqueue(Func<CancellationToken, Task> workItem);

    /// <summary>
    /// 出队任务
    /// </summary>
    Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 获取队列长度
    /// </summary>
    int Count { get; }
}

/// <summary>
/// 后台任务队列实现
/// </summary>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void Enqueue(Func<CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        _workItems.Enqueue(workItem);
        _signal.Release();
    }

    public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _workItems.TryDequeue(out var workItem);
        return workItem!;
    }

    public int Count => _workItems.Count;
}

/// <summary>
/// 后台任务处理器
/// </summary>
public interface IBackgroundTaskProcessor
{
    Task ProcessTaskAsync(Func<CancellationToken, Task> task, CancellationToken cancellationToken);
}

public class BackgroundTaskProcessor : IBackgroundTaskProcessor
{
    private readonly ILogger<BackgroundTaskProcessor> _logger;

    public BackgroundTaskProcessor(ILogger<BackgroundTaskProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessTaskAsync(Func<CancellationToken, Task> task, CancellationToken cancellationToken)
    {
        try
        {
            await task(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Background task was canceled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background task failed");
        }
    }
}