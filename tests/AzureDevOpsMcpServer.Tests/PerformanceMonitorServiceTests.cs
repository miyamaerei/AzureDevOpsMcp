using AzureDevOpsMcpServer.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

/// <summary>
/// PerformanceMonitorService 测试类
/// 测试性能监控服务的统计记录和追踪功能
/// </summary>
public class PerformanceMonitorServiceTests
{
    /// <summary>
    /// 测试初始状态下统计返回默认值
    /// </summary>
    [Fact]
    public void GetStats_ShouldReturnInitialState()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PerformanceMonitorService>>();
        var service = new PerformanceMonitorService(loggerMock.Object);

        // Act
        var stats = service.GetStats();

        // Assert
        Assert.Equal(0, stats.TotalOperations);
        Assert.Equal(TimeSpan.Zero, stats.AverageDuration);
        Assert.Equal(TimeSpan.Zero, stats.MaxDuration);
        Assert.Equal(TimeSpan.Zero, stats.MinDuration);
        Assert.Empty(stats.OperationStats);
    }

    /// <summary>
    /// 测试记录操作时间后统计正确更新
    /// </summary>
    [Fact]
    public void RecordOperationTime_ShouldUpdateStats()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PerformanceMonitorService>>();
        var service = new PerformanceMonitorService(loggerMock.Object);

        // Act
        service.RecordOperationTime("TestOperation", TimeSpan.FromMilliseconds(100));
        service.RecordOperationTime("TestOperation", TimeSpan.FromMilliseconds(200));

        // Assert
        var stats = service.GetStats();
        Assert.Equal(2, stats.TotalOperations);
        Assert.Equal(2, stats.OperationStats["TestOperation"].Count);
        Assert.Equal(TimeSpan.FromMilliseconds(150), stats.AverageDuration);
    }

    /// <summary>
    /// 测试重置统计后清空所有数据
    /// </summary>
    [Fact]
    public void ResetStats_ShouldClearAllData()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PerformanceMonitorService>>();
        var service = new PerformanceMonitorService(loggerMock.Object);
        
        service.RecordOperationTime("TestOperation", TimeSpan.FromMilliseconds(100));

        // Act
        service.ResetStats();

        // Assert
        var stats = service.GetStats();
        Assert.Equal(0, stats.TotalOperations);
        Assert.Empty(stats.OperationStats);
    }

    /// <summary>
    /// 测试使用 TrackOperation 自动记录操作耗时
    /// </summary>
    [Fact]
    public void TrackOperation_ShouldRecordDuration()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PerformanceMonitorService>>();
        var service = new PerformanceMonitorService(loggerMock.Object);

        // Act
        using (service.TrackOperation("TrackedOperation"))
        {
            // 模拟操作耗时
            System.Threading.Thread.Sleep(10);
        }

        // Assert
        var stats = service.GetStats();
        Assert.Equal(1, stats.TotalOperations);
        Assert.True(stats.OperationStats.ContainsKey("TrackedOperation"));
        Assert.Equal(1, stats.OperationStats["TrackedOperation"].Count);
    }
}
