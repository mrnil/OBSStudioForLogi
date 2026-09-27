namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

public class LogThrottleTests
{
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(60);
    private readonly LogThrottle _throttle;
    private DateTime _now = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    public LogThrottleTests()
    {
        this._throttle = new LogThrottle(Window, () => this._now);
    }

    private void Advance(TimeSpan by) => this._now = this._now.Add(by);

    [Fact]
    public void ShouldWrite_FirstOccurrence_WritesWithNothingSuppressed()
    {
        Assert.True(this._throttle.ShouldWrite("Request timed out", out Int32 suppressed));
        Assert.Equal(0, suppressed);
    }

    [Fact]
    public void ShouldWrite_RepeatWithinWindow_IsSuppressed()
    {
        this._throttle.ShouldWrite("Request timed out", out _);

        this.Advance(TimeSpan.FromSeconds(10));

        Assert.False(this._throttle.ShouldWrite("Request timed out", out _));
    }

    [Fact]
    public void ShouldWrite_RepeatAfterWindow_WritesAndReportsSuppressedCount()
    {
        this._throttle.ShouldWrite("Request timed out", out _);
        this._throttle.ShouldWrite("Request timed out", out _);
        this._throttle.ShouldWrite("Request timed out", out _);

        this.Advance(Window + TimeSpan.FromSeconds(1));

        Assert.True(this._throttle.ShouldWrite("Request timed out", out Int32 suppressed));
        Assert.Equal(2, suppressed);
    }

    [Fact]
    public void ShouldWrite_AfterReportingSuppressed_StartsCountingAgain()
    {
        this._throttle.ShouldWrite("Request timed out", out _);
        this._throttle.ShouldWrite("Request timed out", out _);
        this.Advance(Window + TimeSpan.FromSeconds(1));
        this._throttle.ShouldWrite("Request timed out", out _);

        this.Advance(Window + TimeSpan.FromSeconds(1));

        Assert.True(this._throttle.ShouldWrite("Request timed out", out Int32 suppressed));
        Assert.Equal(0, suppressed);
    }

    [Fact]
    public void ShouldWrite_DifferentMessages_AreThrottledIndependently()
    {
        this._throttle.ShouldWrite("Failed for 'Mic'", out _);

        Assert.True(this._throttle.ShouldWrite("Failed for 'Webcam'", out _));
    }

    [Fact]
    public void ShouldWrite_ManyDistinctMessages_ForgetsExpiredOnes()
    {
        for (Int32 i = 0; i < 1000; i++)
        {
            this._throttle.ShouldWrite($"message {i}", out _);
        }

        this.Advance(Window + TimeSpan.FromSeconds(1));
        this._throttle.ShouldWrite("trigger prune", out _);

        Assert.True(this._throttle.TrackedMessageCount <= 1);
    }
}
