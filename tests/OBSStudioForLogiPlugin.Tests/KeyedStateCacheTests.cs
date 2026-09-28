namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Collections.Generic;
using Loupedeck.OBSStudioForLogiPlugin.Helpers;

public class KeyedStateCacheTests
{
    private readonly Queue<Action> _background = new Queue<Action>();
    private readonly List<(String Scene, String Source)> _fetchedNotifications = new List<(String Scene, String Source)>();
    private readonly List<(String Scene, String Source)> _fetchRequests = new List<(String Scene, String Source)>();
    private readonly KeyedStateCache<(String Scene, String Source), Boolean> _cache;
    private Boolean _fetchSucceeds = true;
    private Boolean _fetchValue = true;
    private Exception? _fetchThrows;
    private DateTime _now = new DateTime(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

    public KeyedStateCacheTests()
    {
        this._cache = new KeyedStateCache<(String Scene, String Source), Boolean>(
            this.Fetch,
            key => this._fetchedNotifications.Add(key),
            false,
            "source visibility",
            action => this._background.Enqueue(action),
            () => this._now);
    }

    private Boolean Fetch((String Scene, String Source) key, out Boolean value)
    {
        this._fetchRequests.Add(key);
        if (this._fetchThrows != null)
        {
            throw this._fetchThrows;
        }

        value = this._fetchValue;
        return this._fetchSucceeds;
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
    public void Get_WhenNotCached_ReturnsDefaultWithoutWaitingForOBS()
    {
        Boolean value = this._cache.Get(("Main", "Camera"));

        Assert.False(value);
        Assert.Empty(this._fetchRequests);
        Assert.Single(this._background);
    }

    [Fact]
    public void Get_AfterFetchCompletes_ReturnsFetchedValueAndNotifies()
    {
        this._cache.Get(("Main", "Camera"));

        this.RunBackgroundWork();

        Assert.True(this._cache.Get(("Main", "Camera")));
        Assert.Equal(new[] { ("Main", "Camera") }, this._fetchedNotifications);
    }

    [Fact]
    public void Get_WhenFetchedValueEqualsDefault_CachesItAndDoesNotFetchAgain()
    {
        this._fetchValue = false;
        this._cache.Get(("Main", "Camera"));
        this.RunBackgroundWork();

        this._cache.Get(("Main", "Camera"));

        Assert.Empty(this._background);
        Assert.Single(this._fetchRequests);
    }

    [Fact]
    public void Get_WhileFetchPending_DoesNotScheduleAnotherFetch()
    {
        this._cache.Get(("Main", "Camera"));
        this._cache.Get(("Main", "Camera"));
        this._cache.Get(("Main", "Camera"));

        Assert.Single(this._background);
    }

    [Fact]
    public void Get_ForDifferentKeys_FetchesEachOnce()
    {
        this._cache.Get(("Main", "Camera"));
        this._cache.Get(("Main", "Overlay"));
        this._cache.Get(("BRB", "Camera"));

        this.RunBackgroundWork();

        Assert.Equal(3, this._fetchRequests.Count);
    }

    [Fact]
    public void Get_WithNullKey_ReturnsDefaultWithoutFetching()
    {
        KeyedStateCache<String, String> cache = new KeyedStateCache<String, String>(
            (String key, out String value) =>
            {
                value = "OBS_MEDIA_STATE_PLAYING";
                return true;
            },
            null,
            "OBS_MEDIA_STATE_NONE",
            "media state",
            action => this._background.Enqueue(action),
            () => this._now);

        Assert.Equal("OBS_MEDIA_STATE_NONE", cache.Get(null!));
        Assert.Empty(this._background);
    }

    // --- Events ---

    [Fact]
    public void Set_WhenNotCached_IsReturnedImmediatelyWithoutFetching()
    {
        this._cache.Set(("Main", "Camera"), true);

        Assert.True(this._cache.Get(("Main", "Camera")));
        Assert.Empty(this._background);
    }

    [Fact]
    public void Set_WhileFetchPending_EventValueWinsOverFetchedValue()
    {
        this._cache.Get(("Main", "Camera"));

        this._cache.Set(("Main", "Camera"), false);
        this.RunBackgroundWork();

        Assert.False(this._cache.Get(("Main", "Camera")));
        Assert.Empty(this._fetchedNotifications);
    }

    [Fact]
    public void Set_AfterFetch_OverwritesCachedValue()
    {
        this._cache.Get(("Main", "Camera"));
        this.RunBackgroundWork();

        this._cache.Set(("Main", "Camera"), false);

        Assert.False(this._cache.Get(("Main", "Camera")));
    }

    // --- Failures ---

    [Fact]
    public void Get_AfterFetchFails_DoesNotNotifyOrCache()
    {
        this._fetchSucceeds = false;
        this._cache.Get(("Main", "Camera"));

        this.RunBackgroundWork();

        Assert.Empty(this._fetchedNotifications);
        Assert.False(this._cache.Get(("Main", "Camera")));
    }

    [Fact]
    public void Get_AfterFetchThrows_TreatsItAsFailure()
    {
        this._fetchThrows = new InvalidOperationException("OBS error");
        this._cache.Get(("Main", "Camera"));

        this.RunBackgroundWork();

        Assert.Empty(this._fetchedNotifications);
        Assert.False(this._cache.Get(("Main", "Camera")));
        Assert.Empty(this._background);
    }

    [Fact]
    public void Get_AfterFetchFails_WaitsForRetryDelayBeforeFetchingAgain()
    {
        this._fetchSucceeds = false;
        this._cache.Get(("Main", "Camera"));
        this.RunBackgroundWork();

        this._cache.Get(("Main", "Camera"));
        Assert.Empty(this._background);

        this.Advance(OBSTimings.RenderStateRetryDelay + 1);
        this._cache.Get(("Main", "Camera"));
        Assert.Single(this._background);
    }

    [Fact]
    public void Set_AfterFetchFails_DropsFailureCooldown()
    {
        this._fetchSucceeds = false;
        this._cache.Get(("Main", "Camera"));
        this.RunBackgroundWork();

        this._cache.Set(("Main", "Camera"), true);

        Assert.True(this._cache.Get(("Main", "Camera")));
    }

    // --- Invalidation ---

    [Fact]
    public void Clear_WhileFetchPending_DiscardsStaleResult()
    {
        this._cache.Get(("Main", "Camera"));

        this._cache.Clear();
        this.RunBackgroundWork();

        Assert.Empty(this._fetchedNotifications);
        this._cache.Get(("Main", "Camera"));
        Assert.Single(this._background);
    }

    [Fact]
    public void Clear_DropsFailureCooldown()
    {
        this._fetchSucceeds = false;
        this._cache.Get(("Main", "Camera"));
        this.RunBackgroundWork();

        this._cache.Clear();
        this._cache.Get(("Main", "Camera"));

        Assert.Single(this._background);
    }

    [Fact]
    public void Remove_DropsCachedValueSoNextGetFetchesAgain()
    {
        this._cache.Get(("Main", "Camera"));
        this.RunBackgroundWork();

        this._cache.Remove(("Main", "Camera"));
        this._cache.Get(("Main", "Camera"));

        Assert.Single(this._background);
    }

    [Fact]
    public void Remove_WhileFetchPending_DiscardsStaleResult()
    {
        this._cache.Get(("Main", "Camera"));

        this._cache.Remove(("Main", "Camera"));
        this.RunBackgroundWork();

        Assert.Empty(this._fetchedNotifications);
    }

    [Fact]
    public void RemoveWhere_DropsOnlyMatchingKeys()
    {
        this._cache.Set(("Main", "Camera"), true);
        this._cache.Set(("Main", "Overlay"), true);
        this._cache.Set(("BRB", "Camera"), true);

        this._cache.RemoveWhere(key => key.Scene == "Main");

        Assert.False(this._cache.Get(("Main", "Camera")));
        Assert.False(this._cache.Get(("Main", "Overlay")));
        Assert.True(this._cache.Get(("BRB", "Camera")));
        Assert.Equal(2, this._background.Count);
    }

    [Fact]
    public void RemoveWhere_DiscardsPendingFetchesAndFailuresForMatchingKeys()
    {
        this._fetchSucceeds = false;
        this._cache.Get(("Main", "Camera"));
        this.RunBackgroundWork();
        this._fetchSucceeds = true;
        this._cache.Get(("Main", "Overlay"));

        this._cache.RemoveWhere(key => key.Scene == "Main");
        this.RunBackgroundWork();

        Assert.Empty(this._fetchedNotifications);
        this._cache.Get(("Main", "Camera"));
        Assert.Single(this._background);
    }
}
