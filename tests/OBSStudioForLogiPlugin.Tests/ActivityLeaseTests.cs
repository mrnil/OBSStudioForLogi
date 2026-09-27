namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

public class ActivityLeaseTests
{
    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(3);

    private readonly ActivityLease _lease;
    private DateTime _now = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    public ActivityLeaseTests()
    {
        this._lease = new ActivityLease(Duration, () => this._now);
    }

    private void Advance(TimeSpan by) => this._now = this._now.Add(by);

    [Fact]
    public void IsActive_Initially_IsFalse()
    {
        Assert.False(this._lease.IsActive);
    }

    [Fact]
    public void Touch_FirstTime_ActivatesAndReturnsTrue()
    {
        Assert.True(this._lease.Touch());
        Assert.True(this._lease.IsActive);
    }

    [Fact]
    public void Touch_WhenAlreadyActive_ReturnsFalse()
    {
        this._lease.Touch();

        Assert.False(this._lease.Touch());
    }

    [Fact]
    public void ExpireIfIdle_WhenNeverTouched_ReturnsFalse()
    {
        Assert.False(this._lease.ExpireIfIdle());
    }

    [Fact]
    public void ExpireIfIdle_WithinDuration_StaysActive()
    {
        this._lease.Touch();
        this.Advance(Duration);

        Assert.False(this._lease.ExpireIfIdle());
        Assert.True(this._lease.IsActive);
    }

    [Fact]
    public void ExpireIfIdle_PastDuration_DeactivatesAndReturnsTrueOnce()
    {
        this._lease.Touch();
        this.Advance(Duration + TimeSpan.FromMilliseconds(1));

        Assert.True(this._lease.ExpireIfIdle());
        Assert.False(this._lease.IsActive);
        Assert.False(this._lease.ExpireIfIdle());
    }

    [Fact]
    public void Touch_RenewsLease_DelaysExpiry()
    {
        this._lease.Touch();
        this.Advance(TimeSpan.FromSeconds(2));
        this._lease.Touch();
        this.Advance(TimeSpan.FromSeconds(2));

        Assert.False(this._lease.ExpireIfIdle());
    }

    [Fact]
    public void Touch_AfterExpiry_ReactivatesAndReturnsTrue()
    {
        this._lease.Touch();
        this.Advance(Duration + TimeSpan.FromSeconds(1));
        this._lease.ExpireIfIdle();

        Assert.True(this._lease.Touch());
    }

    [Fact]
    public void Release_WhenActive_ReturnsTrueAndDeactivates()
    {
        this._lease.Touch();

        Assert.True(this._lease.Release());
        Assert.False(this._lease.IsActive);
    }

    [Fact]
    public void Release_WhenInactive_ReturnsFalse()
    {
        Assert.False(this._lease.Release());
    }
}
