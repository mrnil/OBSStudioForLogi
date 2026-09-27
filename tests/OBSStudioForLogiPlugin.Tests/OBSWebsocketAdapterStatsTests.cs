namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Loupedeck.OBSStudioForLogiPlugin.Models;
using Newtonsoft.Json.Linq;

public class OBSWebsocketAdapterStatsTests
{
    [Fact]
    public void ParseStats_MapsEveryField()
    {
        JObject response = JObject.Parse(@"{
            ""activeFps"": 60.0,
            ""cpuUsage"": 12.5,
            ""memoryUsage"": 512.0,
            ""averageFrameRenderTime"": 1.8,
            ""availableDiskSpace"": 1024.0,
            ""renderTotalFrames"": 1000,
            ""renderSkippedFrames"": 3,
            ""outputTotalFrames"": 900,
            ""outputSkippedFrames"": 7
        }");

        OBSStats stats = OBSWebsocketAdapter.ParseStats(response);

        Assert.Equal(60.0, stats.Fps);
        Assert.Equal(12.5, stats.CpuUsage);
        Assert.Equal(512.0, stats.MemoryUsage);
        Assert.Equal(1.8, stats.AverageFrameTime);
        Assert.Equal(1024.0, stats.FreeDiskSpace);
        Assert.Equal(1000, stats.RenderTotalFrames);
        Assert.Equal(3, stats.RenderMissedFrames);
        Assert.Equal(900, stats.OutputTotalFrames);
        Assert.Equal(7, stats.OutputSkippedFrames);
    }

    // OBS briefly reported "cpuUsage": null under load; the library's ObsStats uses non-nullable
    // doubles, so the whole GetStats call failed and logged an error on every poll.
    [Fact]
    public void ParseStats_WhenFieldIsNull_UsesZeroAndKeepsOtherFields()
    {
        JObject response = JObject.Parse(@"{ ""activeFps"": 30.0, ""cpuUsage"": null, ""renderTotalFrames"": 10 }");

        OBSStats stats = OBSWebsocketAdapter.ParseStats(response);

        Assert.Equal(0.0, stats.CpuUsage);
        Assert.Equal(30.0, stats.Fps);
        Assert.Equal(10, stats.RenderTotalFrames);
    }

    [Fact]
    public void ParseStats_WhenFieldsMissing_UsesZero()
    {
        OBSStats stats = OBSWebsocketAdapter.ParseStats(new JObject());

        Assert.Equal(0.0, stats.Fps);
        Assert.Equal(0, stats.OutputSkippedFrames);
    }

    [Fact]
    public void ParseStats_WhenResponseNull_ReturnsNull()
    {
        Assert.Null(OBSWebsocketAdapter.ParseStats(null));
    }
}
