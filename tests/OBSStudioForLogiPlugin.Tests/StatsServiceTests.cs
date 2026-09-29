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

    // --- Polling only while connected and something shows stats ---

    private Int32 _statsCalls;
    private readonly Queue<Action> _background = new Queue<Action>();

    private StatsService CreateService(Boolean inline = true)
    {
        return new StatsService(5000, () =>
        {
            Interlocked.Increment(ref this._statsCalls);
            return new OBSStats { Fps = 60 };
        }, () => OBSStreamStats.Empty, inline ? action => action() : action => this._background.Enqueue(action));
    }

    [Fact]
    public void Start_WithNoViewers_DoesNotPoll()
    {
        using StatsService service = this.CreateService();

        service.Start();

        Assert.False(service.IsPolling);
        Assert.Equal(0, this._statsCalls);
    }

    [Fact]
    public void AddViewer_WhenNotStarted_DoesNotPoll()
    {
        using StatsService service = this.CreateService();

        service.AddViewer("Folder");

        Assert.False(service.IsPolling);
        Assert.Equal(0, this._statsCalls);
    }

    // The first viewer shouldn't wait a whole interval for its first numbers.
    [Fact]
    public void AddViewer_WhenStarted_StartsPollingAndPollsImmediately()
    {
        using StatsService service = this.CreateService();
        service.Start();

        service.AddViewer("Folder");

        Assert.True(service.IsPolling);
        Assert.Equal(1, this._statsCalls);
        Assert.Equal(60, service.CurrentStats.Fps);
    }

    [Fact]
    public void AddViewer_ImmediatePollRunsInBackground()
    {
        using StatsService service = this.CreateService(inline: false);
        service.Start();

        service.AddViewer("Folder");

        Assert.Equal(0, this._statsCalls);
        Assert.Single(this._background);
    }

    [Fact]
    public void Start_WithViewerAlreadyRegistered_StartsPollingAndPollsImmediately()
    {
        using StatsService service = this.CreateService();
        service.AddViewer("Folder");

        service.Start();

        Assert.True(service.IsPolling);
        Assert.Equal(1, this._statsCalls);
    }

    [Fact]
    public void AddViewer_SecondViewer_DoesNotPollAgain()
    {
        using StatsService service = this.CreateService();
        service.Start();
        service.AddViewer("Folder");

        service.AddViewer("Button");

        Assert.Equal(1, this._statsCalls);
    }

    [Fact]
    public void RemoveViewer_LastViewer_StopsPolling()
    {
        using StatsService service = this.CreateService();
        service.Start();
        service.AddViewer("Folder");

        service.RemoveViewer("Folder");

        Assert.False(service.IsPolling);
    }

    [Fact]
    public void RemoveViewer_WhileAnotherViewerRemains_KeepsPolling()
    {
        using StatsService service = this.CreateService();
        service.Start();
        service.AddViewer("Folder");
        service.AddViewer("Button");

        service.RemoveViewer("Folder");

        Assert.True(service.IsPolling);
    }

    [Fact]
    public void RemoveViewer_OwnerNeverAdded_KeepsPolling()
    {
        using StatsService service = this.CreateService();
        service.Start();
        service.AddViewer("Folder");

        service.RemoveViewer("Button");

        Assert.True(service.IsPolling);
    }

    [Fact]
    public void Stop_WithViewers_StopsPollingAndClearsStats()
    {
        using StatsService service = this.CreateService();
        service.Start();
        service.AddViewer("Folder");

        service.Stop();

        Assert.False(service.IsPolling);
        Assert.Null(service.CurrentStats);
    }

    // Viewers survive a disconnect: an open folder picks up polling again on reconnect.
    [Fact]
    public void Start_AfterStopWithViewerStillRegistered_ResumesPolling()
    {
        using StatsService service = this.CreateService();
        service.Start();
        service.AddViewer("Folder");
        service.Stop();

        service.Start();

        Assert.True(service.IsPolling);
        Assert.Equal(2, this._statsCalls);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AddViewer_WhenOwnerEmpty_IsIgnored(String? owner)
    {
        using StatsService service = this.CreateService();
        service.Start();

        service.AddViewer(owner!);

        Assert.False(service.IsPolling);
    }

    [Fact]
    public void AddViewer_AfterDispose_DoesNotPoll()
    {
        StatsService service = this.CreateService();
        service.Start();
        service.Dispose();

        service.AddViewer("Folder");

        Assert.Equal(0, this._statsCalls);
    }
}
