namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Collections.Generic;
using Loupedeck.OBSStudioForLogiPlugin.Helpers;
using Loupedeck.OBSStudioForLogiPlugin.Models;

public class AudioStateCacheTests
{
    private readonly Queue<Action> _background = new Queue<Action>();
    private readonly List<String> _fetchedNotifications = new List<String>();
    private readonly List<String> _fetchRequests = new List<String>();
    private readonly AudioStateCache _cache;
    private Func<String, AudioInputState> _fetchResult = _ => new AudioInputState { IsMuted = true, VolumeMul = 0.5f, MonitorType = "OBS_MONITORING_TYPE_MONITOR_ONLY" };
    private DateTime _now = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    public AudioStateCacheTests()
    {
        this._cache = new AudioStateCache(
            name =>
            {
                this._fetchRequests.Add(name);
                return this._fetchResult(name);
            },
            name => this._fetchedNotifications.Add(name),
            action => this._background.Enqueue(action),
            () => this._now);
    }

    private void RunBackgroundWork()
    {
        while (this._background.Count > 0)
        {
            this._background.Dequeue()();
        }
    }

    private void Advance(Int32 milliseconds) => this._now = this._now.AddMilliseconds(milliseconds);

    // --- Misses ---

    [Fact]
    public void Get_WhenNotCached_ReturnsDefaultsWithoutWaitingForOBS()
    {
        AudioInputState state = this._cache.Get("Microphone");

        Assert.False(state.IsMuted);
        Assert.Equal(1.0f, state.VolumeMul);
        Assert.Equal(AudioInputState.MonitorTypeNone, state.MonitorType);
        Assert.Empty(this._fetchRequests);
        Assert.Single(this._background);
    }

    [Fact]
    public void Get_AfterFetchCompletes_ReturnsFetchedStateAndNotifies()
    {
        this._cache.Get("Microphone");

        this.RunBackgroundWork();
        AudioInputState state = this._cache.Get("Microphone");

        Assert.True(state.IsMuted);
        Assert.Equal(0.5f, state.VolumeMul);
        Assert.Equal("OBS_MONITORING_TYPE_MONITOR_ONLY", state.MonitorType);
        Assert.Equal(new[] { "Microphone" }, this._fetchedNotifications);
    }

    [Fact]
    public void Get_WhenFullyCached_DoesNotFetchAgain()
    {
        this._cache.Get("Microphone");
        this.RunBackgroundWork();

        this._cache.Get("Microphone");
        this._cache.Get("Microphone");

        Assert.Empty(this._background);
        Assert.Single(this._fetchRequests);
    }

    [Fact]
    public void Get_WhileFetchPending_DoesNotScheduleAnotherFetch()
    {
        this._cache.Get("Microphone");
        this._cache.Get("Microphone");
        this._cache.Get("Microphone");

        Assert.Single(this._background);
    }

    [Fact]
    public void Get_WithEmptyName_ReturnsDefaultsWithoutFetching()
    {
        AudioInputState state = this._cache.Get(String.Empty);

        Assert.Equal(1.0f, state.VolumeMul);
        Assert.Empty(this._background);
    }

    // --- Events ---

    [Fact]
    public void SetVolume_WhenNotCached_IsReturnedImmediately()
    {
        this._cache.SetVolume("Microphone", 0.25f);

        Assert.Equal(0.25f, this._cache.Get("Microphone").VolumeMul);
    }

    [Fact]
    public void SetMuted_WhileFetchPending_EventValueWinsOverFetchedValue()
    {
        this._cache.Get("Microphone");

        this._cache.SetMuted("Microphone", false);
        this.RunBackgroundWork();

        AudioInputState state = this._cache.Get("Microphone");
        Assert.False(state.IsMuted);
        Assert.Equal(0.5f, state.VolumeMul);
    }

    [Fact]
    public void SetMonitorType_AfterFetch_OverwritesCachedValue()
    {
        this._cache.Get("Microphone");
        this.RunBackgroundWork();

        this._cache.SetMonitorType("Microphone", "OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT");

        Assert.Equal("OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT", this._cache.Get("Microphone").MonitorType);
    }

    [Fact]
    public void Get_WhenAllFieldsSetByEvents_DoesNotFetch()
    {
        this._cache.SetMuted("Microphone", true);
        this._cache.SetVolume("Microphone", 0.8f);
        this._cache.SetMonitorType("Microphone", AudioInputState.MonitorTypeNone);

        this._cache.Get("Microphone");

        Assert.Empty(this._background);
    }

    // --- Failures ---

    [Fact]
    public void Get_AfterFetchFails_DoesNotNotifyOrCache()
    {
        this._fetchResult = _ => null;
        this._cache.Get("Microphone");

        this.RunBackgroundWork();

        Assert.Empty(this._fetchedNotifications);
        Assert.Equal(1.0f, this._cache.Get("Microphone").VolumeMul);
    }

    [Fact]
    public void Get_AfterFetchFails_WaitsForRetryDelayBeforeFetchingAgain()
    {
        this._fetchResult = _ => null;
        this._cache.Get("Microphone");
        this.RunBackgroundWork();

        this._cache.Get("Microphone");
        Assert.Empty(this._background);

        this.Advance(OBSTimings.AudioStateRetryDelay + 1);
        this._cache.Get("Microphone");
        Assert.Single(this._background);
    }

    // --- Invalidation ---

    [Fact]
    public void Clear_WhileFetchPending_DiscardsStaleResult()
    {
        this._cache.Get("Microphone");

        this._cache.Clear();
        this.RunBackgroundWork();

        Assert.Empty(this._fetchedNotifications);
        Assert.Equal(1.0f, this._cache.Get("Microphone").VolumeMul);
    }

    [Fact]
    public void Clear_DropsFailureCooldown()
    {
        this._fetchResult = _ => null;
        this._cache.Get("Microphone");
        this.RunBackgroundWork();

        this._cache.Clear();
        this._cache.Get("Microphone");

        Assert.Single(this._background);
    }

    [Fact]
    public void Remove_DropsCachedStateSoNextGetFetchesAgain()
    {
        this._cache.Get("Microphone");
        this.RunBackgroundWork();

        this._cache.Remove("Microphone");
        this._cache.Get("Microphone");

        Assert.Single(this._background);
    }
}
