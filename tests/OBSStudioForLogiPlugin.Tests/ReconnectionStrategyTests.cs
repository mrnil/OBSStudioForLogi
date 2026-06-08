namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Moq;

public class ReconnectionStrategyTests
{
    private readonly Mock<IPluginLog> _mockLog;
    private readonly ReconnectionStrategy _strategy;

    public ReconnectionStrategyTests()
    {
        this._mockLog = new Mock<IPluginLog>();
        this._strategy = new ReconnectionStrategy(this._mockLog.Object);
    }

    // --- Initial state ---

    [Fact]
    public void Attempts_Initially_ReturnsZero()
    {
        Assert.Equal(0, this._strategy.Attempts);
    }

    // --- Reset ---

    [Fact]
    public void Reset_AfterAttempts_ResetsToZero()
    {
        this._strategy.TryReconnect(() => { });
        this._strategy.TryReconnect(() => { });

        this._strategy.Reset();

        Assert.Equal(0, this._strategy.Attempts);
    }

    // --- TryReconnect ---

    [Fact]
    public void TryReconnect_IncrementsAttempts()
    {
        this._strategy.TryReconnect(() => { });

        Assert.Equal(1, this._strategy.Attempts);
    }

    [Fact]
    public void TryReconnect_CalledMultipleTimes_IncrementsEachTime()
    {
        this._strategy.TryReconnect(() => { });
        this._strategy.TryReconnect(() => { });
        this._strategy.TryReconnect(() => { });

        Assert.Equal(3, this._strategy.Attempts);
    }

    [Fact]
    public void TryReconnect_InvokesConnectAction()
    {
        var called = false;

        this._strategy.TryReconnect(() => called = true);

        Assert.True(called);
    }

    [Fact]
    public void TryReconnect_WhenActionSucceeds_ReturnsTrue()
    {
        var result = this._strategy.TryReconnect(() => { });

        Assert.True(result);
    }

    [Fact]
    public void TryReconnect_WhenActionThrows_ReturnsFalse()
    {
        var result = this._strategy.TryReconnect(() => throw new Exception("Connection refused"));

        Assert.False(result);
    }

    [Fact]
    public void TryReconnect_WhenActionThrows_LogsWarning()
    {
        this._strategy.TryReconnect(() => throw new Exception("Connection refused"));

        this._mockLog.Verify(x => x.Warning(It.Is<String>(s => s.Contains("Connection refused"))), Times.Once);
    }

    [Fact]
    public void TryReconnect_WhenActionThrows_StillIncrementsAttempts()
    {
        this._strategy.TryReconnect(() => throw new Exception("fail"));

        Assert.Equal(1, this._strategy.Attempts);
    }

    [Fact]
    public void TryReconnect_LogsAttemptNumber()
    {
        this._strategy.TryReconnect(() => { });

        this._mockLog.Verify(x => x.Info(It.Is<String>(s => s.Contains("1"))), Times.Once);
    }

    // --- GetNextDelay ---

    [Fact]
    public void GetNextDelay_FirstAttempt_ReturnsDelayInFirstRange()
    {
        var delay = this._strategy.GetNextDelay();

        Assert.InRange(delay, 850, 1150); // 1000 * 0.85-1.15
    }

    [Fact]
    public void GetNextDelay_AfterOneAttempt_ReturnsDelayInSecondRange()
    {
        this._strategy.TryReconnect(() => { });

        var delay = this._strategy.GetNextDelay();

        Assert.InRange(delay, 1700, 2300); // 2000 * 0.85-1.15
    }

    [Fact]
    public void GetNextDelay_AfterManyAttempts_CapsAtMaxDelay()
    {
        for (var i = 0; i < 20; i++)
            this._strategy.TryReconnect(() => { });

        var delay = this._strategy.GetNextDelay();

        Assert.InRange(delay, 25500, 34500); // 30000 * 0.85-1.15
    }

    [Fact]
    public void GetNextDelay_AfterReset_ReturnsFirstRange()
    {
        this._strategy.TryReconnect(() => { });
        this._strategy.TryReconnect(() => { });
        this._strategy.Reset();

        var delay = this._strategy.GetNextDelay();

        Assert.InRange(delay, 850, 1150);
    }

    // --- Integration: full reconnection sequence ---

    [Fact]
    public void FullSequence_AttemptsEscalateAndReset()
    {
        // Simulate failed attempts with escalating delays
        this._strategy.TryReconnect(() => throw new Exception("fail"));
        var delay1 = this._strategy.GetNextDelay();

        this._strategy.TryReconnect(() => throw new Exception("fail"));
        var delay2 = this._strategy.GetNextDelay();

        Assert.True(delay2 > delay1); // Delays escalate

        // Simulate successful connection
        this._strategy.Reset();
        Assert.Equal(0, this._strategy.Attempts);

        var delayAfterReset = this._strategy.GetNextDelay();
        Assert.InRange(delayAfterReset, 850, 1150); // Back to base
    }
}
