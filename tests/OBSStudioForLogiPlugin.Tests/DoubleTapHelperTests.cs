namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System;
using System.Threading;
using Loupedeck.OBSStudioForLogiPlugin;
using Loupedeck.OBSStudioForLogiPlugin.Helpers;
using Xunit;

public class DoubleTapHelperTests
{
    private readonly DoubleTapHelper _helper = new DoubleTapHelper();

    [Fact]
    public void OnTap_NullParameter_DoesNotInvoke()
    {
        Boolean singleFired = false;
        Boolean doubleFired = false;

        this._helper.OnTap(null, _ => singleFired = true, _ => doubleFired = true);

        Thread.Sleep(OBSTimings.TestAsyncDelay);
        Assert.False(singleFired);
        Assert.False(doubleFired);
    }

    [Fact]
    public void OnTap_EmptyParameter_DoesNotInvoke()
    {
        Boolean singleFired = false;

        this._helper.OnTap(String.Empty, _ => singleFired = true, null);

        Thread.Sleep(OBSTimings.TestAsyncDelay);
        Assert.False(singleFired);
    }

    [Fact]
    public void OnTap_SingleTap_FiresSingleAfterThreshold()
    {
        String? received = null;

        this._helper.OnTap("input1", p => received = p, _ => received = "double");

        WaitFor(() => received != null);
        Assert.Equal("input1", received);
    }

    [Fact]
    public void OnTap_DoubleTap_FiresDoubleNotSingle()
    {
        Boolean singleFired = false;
        String? doubleReceived = null;

        this._helper.OnTap("input1", _ => singleFired = true, p => doubleReceived = p);
        this._helper.OnTap("input1", _ => singleFired = true, p => doubleReceived = p);

        Thread.Sleep(OBSTimings.TestAsyncDelay);
        Assert.Equal("input1", doubleReceived);
        Assert.False(singleFired);
    }

    [Fact]
    public void OnTap_TwoDistinctParameters_FireIndependently()
    {
        String? single1 = null;
        String? single2 = null;

        this._helper.OnTap("a", p => single1 = p, null);
        this._helper.OnTap("b", p => single2 = p, null);

        WaitFor(() => single1 != null && single2 != null);
        Assert.Equal("a", single1);
        Assert.Equal("b", single2);
    }

    [Fact]
    public void Reset_CancelsPendingSingleTap()
    {
        Boolean singleFired = false;

        this._helper.OnTap("input1", _ => singleFired = true, null);
        this._helper.Reset();

        Thread.Sleep(OBSTimings.TestAsyncDelay);
        Assert.False(singleFired);
    }

    [Fact]
    public void Reset_DoesNotThrowWhenEmpty()
    {
        var exception = Record.Exception(() => this._helper.Reset());

        Assert.Null(exception);
    }

    [Fact]
    public void OnTap_WindowOff_FiresSingleImmediately()
    {
        DoubleTapHelper helper = new DoubleTapHelper(() => 0);
        String? received = null;

        helper.OnTap("input1", p => received = p, _ => received = "double");

        Assert.Equal("input1", received);
    }

    [Fact]
    public void OnTap_WindowOff_QuickSecondTapFiresSingleAgain()
    {
        DoubleTapHelper helper = new DoubleTapHelper(() => 0);
        Int32 singleCount = 0;
        Boolean doubleFired = false;

        helper.OnTap("input1", _ => singleCount++, _ => doubleFired = true);
        helper.OnTap("input1", _ => singleCount++, _ => doubleFired = true);

        Assert.Equal(2, singleCount);
        Assert.False(doubleFired);
    }

    [Fact]
    public void OnTap_NegativeWindow_TreatedAsOff()
    {
        DoubleTapHelper helper = new DoubleTapHelper(() => -100);
        Boolean singleFired = false;

        helper.OnTap("input1", _ => singleFired = true, null);

        Assert.True(singleFired);
    }

    [Fact]
    public void OnTap_LongWindow_WaitsForWindowBeforeSingle()
    {
        DoubleTapHelper helper = new DoubleTapHelper(() => 10000);
        Boolean singleFired = false;

        helper.OnTap("input1", _ => singleFired = true, null);

        Thread.Sleep(OBSTimings.TestAsyncDelay);
        Assert.False(singleFired);
        helper.Reset();
    }

    [Fact]
    public void OnTap_ReadsWindowOnEachTap()
    {
        Int32 windowMs = 0;
        DoubleTapHelper helper = new DoubleTapHelper(() => windowMs);
        Int32 singleCount = 0;
        String? doubleReceived = null;

        helper.OnTap("input1", _ => singleCount++, p => doubleReceived = p);
        windowMs = 10000;
        helper.OnTap("input1", _ => singleCount++, p => doubleReceived = p);
        helper.OnTap("input1", _ => singleCount++, p => doubleReceived = p);

        Assert.Equal(1, singleCount);
        Assert.Equal("input1", doubleReceived);
    }

    // Polls instead of a fixed sleep so these assertions don't flake under full-suite
    // thread-pool contention, where a fixed window can be too tight (Assessment #14).
    private static void WaitFor(Func<Boolean> condition, Int32 timeoutMs = 3000)
    {
        Int32 elapsed = 0;
        const Int32 pollIntervalMs = 20;

        while (!condition() && elapsed < timeoutMs)
        {
            Thread.Sleep(pollIntervalMs);
            elapsed += pollIntervalMs;
        }
    }
}
