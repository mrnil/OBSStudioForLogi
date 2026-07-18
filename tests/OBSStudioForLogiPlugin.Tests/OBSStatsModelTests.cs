namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Loupedeck.OBSStudioForLogiPlugin.Models;

public class OBSStatsModelTests
{
    [Fact]
    public void Fps_ReturnsSetValue()
    {
        var stats = new OBSStats { Fps = 60.0 };

        Assert.Equal(60.0, stats.Fps);
    }

    [Fact]
    public void Fps_DefaultsToZero()
    {
        var stats = new OBSStats();

        Assert.Equal(0, stats.Fps);
    }

    [Fact]
    public void RenderLagPercent_WhenFramesExist_ReturnsPercentage()
    {
        var stats = new OBSStats { RenderTotalFrames = 1000, RenderMissedFrames = 10 };

        Assert.Equal(1.0, stats.RenderLagPercent);
    }

    [Fact]
    public void RenderLagPercent_WhenNoFrames_ReturnsZero()
    {
        var stats = new OBSStats { RenderTotalFrames = 0, RenderMissedFrames = 0 };

        Assert.Equal(0, stats.RenderLagPercent);
    }

    [Fact]
    public void EncodingLagPercent_WhenFramesExist_ReturnsPercentage()
    {
        var stats = new OBSStats { OutputTotalFrames = 2000, OutputSkippedFrames = 50 };

        Assert.Equal(2.5, stats.EncodingLagPercent);
    }

    [Fact]
    public void EncodingLagPercent_WhenNoFrames_ReturnsZero()
    {
        var stats = new OBSStats { OutputTotalFrames = 0, OutputSkippedFrames = 0 };

        Assert.Equal(0, stats.EncodingLagPercent);
    }

    [Fact]
    public void TotalDroppedFrames_ReturnsSumOfMissedAndSkipped()
    {
        var stats = new OBSStats { RenderMissedFrames = 5, OutputSkippedFrames = 3 };

        Assert.Equal(8, stats.TotalDroppedFrames);
    }

    [Fact]
    public void Empty_ReturnsInstanceWithAllZeroValues()
    {
        var empty = OBSStats.Empty;

        Assert.Equal(0, empty.Fps);
        Assert.Equal(0, empty.CpuUsage);
        Assert.Equal(0, empty.MemoryUsage);
        Assert.Equal(0, empty.TotalDroppedFrames);
    }

    [Fact]
    public void Empty_ReturnsNewInstanceEachCall()
    {
        Assert.NotSame(OBSStats.Empty, OBSStats.Empty);
    }
}

public class OBSStreamStatsModelTests
{
    [Fact]
    public void SkippedPercent_WhenFramesExist_ReturnsPercentage()
    {
        var stats = new OBSStreamStats { TotalFrames = 1000, SkippedFrames = 25 };

        Assert.Equal(2.5, stats.SkippedPercent);
    }

    [Fact]
    public void SkippedPercent_WhenNoFrames_ReturnsZero()
    {
        var stats = new OBSStreamStats { TotalFrames = 0, SkippedFrames = 0 };

        Assert.Equal(0, stats.SkippedPercent);
    }

    [Fact]
    public void DurationFormatted_WhenUnderOneHour_ReturnsMinutesAndSeconds()
    {
        var stats = new OBSStreamStats { Duration = 125000 }; // 2 min 5 sec

        Assert.Equal("2:05", stats.DurationFormatted);
    }

    [Fact]
    public void DurationFormatted_WhenOverOneHour_ReturnsHoursMinutesSeconds()
    {
        var stats = new OBSStreamStats { Duration = 3725000 }; // 1h 2m 5s

        Assert.Equal("1:02:05", stats.DurationFormatted);
    }

    [Fact]
    public void DurationFormatted_WhenZero_ReturnsZeroMinutes()
    {
        var stats = new OBSStreamStats { Duration = 0 };

        Assert.Equal("0:00", stats.DurationFormatted);
    }

    [Fact]
    public void Empty_ReturnsInstanceWithIsActiveFalse()
    {
        var empty = OBSStreamStats.Empty;

        Assert.False(empty.IsActive);
        Assert.Equal(0, empty.BytesSent);
        Assert.Equal(0, empty.SkippedFrames);
    }

    [Fact]
    public void Empty_ReturnsNewInstanceEachCall()
    {
        Assert.NotSame(OBSStreamStats.Empty, OBSStreamStats.Empty);
    }
}
