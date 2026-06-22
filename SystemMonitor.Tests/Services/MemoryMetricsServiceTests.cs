using System;
using SystemMonitor.Core.Services;
using Xunit;

namespace SystemMonitor.Tests.Services;

public sealed class MemoryMetricsServiceTests
{
    [Fact]
    public void GetSnapshot_ShouldReturnMemorySnapshot()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var service = new MemoryMetricsService();

        var snapshot = service.GetSnapshot();

        Assert.NotNull(snapshot);
        Assert.True(snapshot.Timestamp <= DateTime.UtcNow);
        Assert.True(snapshot.TotalBytes >= 0);
        Assert.True(snapshot.UsedBytes >= 0);
        Assert.True(snapshot.AvailableBytes >= 0);
    }

    [Fact]
    public void GetSnapshot_ShouldReturnUsedMemoryNotGreaterThanTotalMemory()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var service = new MemoryMetricsService();

        var snapshot = service.GetSnapshot();

        Assert.True(snapshot.UsedBytes <= snapshot.TotalBytes);
    }

    [Fact]
    public void GetSnapshot_ShouldReturnTotalMemoryWhenWindowsApiIsAvailable()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var service = new MemoryMetricsService();

        var snapshot = service.GetSnapshot();

        Assert.True(snapshot.TotalBytes > 0);
    }
}
