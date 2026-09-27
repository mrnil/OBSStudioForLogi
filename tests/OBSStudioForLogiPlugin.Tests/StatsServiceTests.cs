namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Threading.Tasks;
using Loupedeck.OBSStudioForLogiPlugin.Models;
using Loupedeck.OBSStudioForLogiPlugin.Services;

public class StatsServiceTests
{
    [Fact]
    public void Poll_StoresStatsAndRaisesUpdated()
    {
        OBSStats stats = new OBSStats { Fps = 60 };
        using StatsService service = new StatsService(5000, () => stats, () => OBSStreamStats.Empty);
        Int32 updates = 0;
        service.StatsUpdated += (s, e) => updates++;

        service.Poll();

        Assert.Same(stats, service.CurrentStats);
        Assert.Equal(1, updates);
    }

    // The timer fires every interval regardless of whether the last poll finished, and each poll
    // makes blocking OBS requests that can take up to the request timeout - so a slow OBS used to
    // accumulate overlapping polls.
    [Fact]
    public void Poll_WhilePreviousPollStillRunning_IsSkipped()
    {
        ManualResetEventSlim release = new ManualResetEventSlim(false);
        ManualResetEventSlim firstPollStarted = new ManualResetEventSlim(false);
        Int32 statsCalls = 0;
        using StatsService service = new StatsService(5000, () =>
        {
            Interlocked.Increment(ref statsCalls);
            firstPollStarted.Set();
            release.Wait(TimeSpan.FromSeconds(5));
            return OBSStats.Empty;
        }, () => OBSStreamStats.Empty);

        Task firstPoll = Task.Run(service.Poll);
        Assert.True(firstPollStarted.Wait(TimeSpan.FromSeconds(5)));

        service.Poll();
        release.Set();
        firstPoll.Wait(TimeSpan.FromSeconds(5));

        Assert.Equal(1, statsCalls);
    }

    [Fact]
    public void Poll_AfterPreviousPollCompletes_RunsAgain()
    {
        Int32 statsCalls = 0;
        using StatsService service = new StatsService(5000, () =>
        {
            Interlocked.Increment(ref statsCalls);
            return OBSStats.Empty;
        }, () => OBSStreamStats.Empty);

        service.Poll();
        service.Poll();

        Assert.Equal(2, statsCalls);
    }

    [Fact]
    public void Poll_WhenProviderThrows_DoesNotBlockLaterPolls()
    {
        Int32 statsCalls = 0;
        using StatsService service = new StatsService(5000, () =>
        {
            if (Interlocked.Increment(ref statsCalls) == 1)
            {
                throw new InvalidOperationException("OBS error");
            }

            return OBSStats.Empty;
        }, () => OBSStreamStats.Empty);

        service.Poll();
        service.Poll();

        Assert.Equal(2, statsCalls);
        Assert.NotNull(service.CurrentStats);
    }
}
