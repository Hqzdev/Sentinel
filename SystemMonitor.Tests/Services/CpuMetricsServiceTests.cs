using System;
using System.Threading;
using SystemMonitor.Core.Services;
using Xunit;

namespace SystemMonitor.Tests.Services;

public sealed class CpuMetricsServiceTests
{
    [Fact]
    public void Constructor_ShouldSetLogicalCoreCount()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var service = new CpuMetricsService();

        Assert.True(service.LogicalCoreCount > 0);
        Assert.Equal(Environment.ProcessorCount, service.LogicalCoreCount);
    }

    [Fact]
    public void GetSnapshot_ShouldReturnCpuSnapshot()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var service = new CpuMetricsService();

        Thread.Sleep(1000);

        var snapshot = service.GetSnapshot();

        Assert.NotNull(snapshot);
        Assert.True(snapshot.Timestamp <= DateTime.UtcNow);
        Assert.True(snapshot.TotalUsagePercent >= 0);
        Assert.NotNull(snapshot.PerCoreUsagePercent);
        Assert.Equal(service.LogicalCoreCount, snapshot.PerCoreUsagePercent.Count);
    }

    [Fact]
    public void GetSnapshot_ShouldReturnValidCoreValues()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var service = new CpuMetricsService();

        Thread.Sleep(1000);

        var snapshot = service.GetSnapshot();

        Assert.All(snapshot.PerCoreUsagePercent, usage =>
        {
            Assert.True(usage >= 0);
            Assert.True(usage <= 100);
        });
    }
}
