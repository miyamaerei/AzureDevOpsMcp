using AzureDevOpsMcpServer.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureDevOpsMcpServer.Tests;

public class PerformanceMonitorServiceTests
{
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

    [Fact]
    public void TrackOperation_ShouldRecordDuration()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PerformanceMonitorService>>();
        var service = new PerformanceMonitorService(loggerMock.Object);

        // Act
        using (service.TrackOperation("TrackedOperation"))
        {
            // Simulate operation
            System.Threading.Thread.Sleep(10);
        }

        // Assert
        var stats = service.GetStats();
        Assert.Equal(1, stats.TotalOperations);
        Assert.True(stats.OperationStats.ContainsKey("TrackedOperation"));
        Assert.Equal(1, stats.OperationStats["TrackedOperation"].Count);
    }
}
