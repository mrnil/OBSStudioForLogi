namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

public class CachedValueTests
{
    private Int32 _fetches;
    private Boolean _fetchSucceeds = true;
    private String _nextValue = "first";
    private Action? _duringFetch;

    private CachedValue<String> CreateCache()
    {
        return new CachedValue<String>(this.Fetch, "test value");
    }

    private Boolean Fetch(out String value)
    {
        this._fetches++;
        this._duringFetch?.Invoke();
        value = this._fetchSucceeds ? this._nextValue : null!;
        return this._fetchSucceeds;
    }

    [Fact]
    public void TryGet_OnFirstCall_FetchesAndReturnsValue()
    {
        CachedValue<String> cache = this.CreateCache();

        Boolean found = cache.TryGet(out String value);

        Assert.True(found);
        Assert.Equal("first", value);
        Assert.Equal(1, this._fetches);
    }

    [Fact]
    public void TryGet_AfterSuccessfulFetch_ReturnsStoredValueWithoutFetching()
    {
        CachedValue<String> cache = this.CreateCache();
        cache.TryGet(out String _);
        this._nextValue = "second";

        cache.TryGet(out String value);

        Assert.Equal("first", value);
        Assert.Equal(1, this._fetches);
    }

    [Fact]
    public void TryGet_AfterInvalidate_FetchesAgain()
    {
        CachedValue<String> cache = this.CreateCache();
        cache.TryGet(out String _);
        this._nextValue = "second";

        cache.Invalidate("test");
        cache.TryGet(out String value);

        Assert.Equal("second", value);
        Assert.Equal(2, this._fetches);
    }

    // A failed fetch (not connected, OBS error) must not be remembered as an answer.
    [Fact]
    public void TryGet_WhenFetchFails_ReturnsFalseAndTriesAgainNextTime()
    {
        CachedValue<String> cache = this.CreateCache();
        this._fetchSucceeds = false;

        Boolean first = cache.TryGet(out String value);
        this._fetchSucceeds = true;
        Boolean second = cache.TryGet(out String _);

        Assert.False(first);
        Assert.Null(value);
        Assert.True(second);
        Assert.Equal(2, this._fetches);
    }

    [Fact]
    public void TryGet_WhenFetchReportsSuccessWithNull_ReturnsFalse()
    {
        CachedValue<String> cache = new CachedValue<String>((out String value) =>
        {
            value = null!;
            return true;
        }, "test value");

        Assert.False(cache.TryGet(out String _));
    }

    // OBS changed while the fetch was running, so its result may already be out of date: the
    // caller gets it, but the next caller fetches again.
    [Fact]
    public void TryGet_WhenInvalidatedDuringFetch_ReturnsResultButDoesNotKeepIt()
    {
        CachedValue<String> cache = this.CreateCache();
        this._duringFetch = () =>
        {
            this._duringFetch = null;
            cache.Invalidate("changed during fetch");
        };

        Boolean found = cache.TryGet(out String value);
        cache.TryGet(out String _);

        Assert.True(found);
        Assert.Equal("first", value);
        Assert.Equal(2, this._fetches);
    }

    [Fact]
    public void Invalidate_WhenNothingCached_DoesNotThrow()
    {
        CachedValue<String> cache = this.CreateCache();

        Exception? exception = Record.Exception(() => cache.Invalidate("test"));

        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WhenFetchNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CachedValue<String>(null!, "test value"));
    }
}
