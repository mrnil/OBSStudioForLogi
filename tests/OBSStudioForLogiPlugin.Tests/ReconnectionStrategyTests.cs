namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Linq;
using System.Threading.Tasks;
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
        this._strategy.BeginAttempt();
        this._strategy.BeginAttempt();

        this._strategy.Reset();

        Assert.Equal(0, this._strategy.Attempts);
    }

    // --- BeginAttempt ---

    [Fact]
    public void BeginAttempt_IncrementsAttempts()
    {
        this._strategy.BeginAttempt();

        Assert.Equal(1, this._strategy.Attempts);
    }

    [Fact]
    public void BeginAttempt_CalledMultipleTimes_IncrementsEachTime()
    {
        this._strategy.BeginAttempt();
        this._strategy.BeginAttempt();
        this._strategy.BeginAttempt();

        Assert.Equal(3, this._strategy.Attempts);
    }

    [Fact]
    public void BeginAttempt_FirstAttempt_ReturnsDelayInFirstRange()
    {
        Int32 delay = this._strategy.BeginAttempt();

        Assert.InRange(delay, 850, 1150); // 1000 * 0.85-1.15
    }

    [Fact]
    public void BeginAttempt_SecondAttempt_ReturnsDelayInSecondRange()
    {
        this._strategy.BeginAttempt();

        Int32 delay = this._strategy.BeginAttempt();

        Assert.InRange(delay, 1700, 2300); // 2000 * 0.85-1.15
    }

    [Fact]
    public void BeginAttempt_LogsAttemptNumberAndDelay()
    {
        Int32 delay = this._strategy.BeginAttempt();

        this._mockLog.Verify(x => x.Info(It.Is<String>(s => s.Contains("attempt 1") && s.Contains($"{delay}ms"))), Times.Once);
    }

    // Connects and drops are reported on different threads (#29).
    [Fact]
    public void BeginAttempt_FromManyThreads_CountsEveryAttempt()
    {
        Parallel.For(0, 200, _ => this._strategy.BeginAttempt());

        Assert.Equal(200, this._strategy.Attempts);
    }

    // --- Custom delays ---

    [Fact]
    public void Constructor_WithBackoffDelays_UsesThemInOrderThenRepeatsTheLast()
    {
        ReconnectionStrategy strategy = new ReconnectionStrategy(this._mockLog.Object, new[] { 100, 1000 });

        Int32[] delays = Enumerable.Range(0, 3).Select(_ => strategy.BeginAttempt()).ToArray();

        Assert.InRange(delays[0], 85, 115);
        Assert.InRange(delays[1], 850, 1150);
        Assert.InRange(delays[2], 850, 1150);
    }

    [Fact]
    public void Constructor_WithEmptyBackoffDelays_UsesTheDefaults()
    {
        ReconnectionStrategy strategy = new ReconnectionStrategy(this._mockLog.Object, new Int32[0]);

        Assert.InRange(strategy.GetNextDelay(), 850, 1150);
    }

    // --- GetNextDelay ---

    [Fact]
    public void GetNextDelay_FirstAttempt_ReturnsDelayInFirstRange()
    {
        Int32 delay = this._strategy.GetNextDelay();

        Assert.InRange(delay, 850, 1150); // 1000 * 0.85-1.15
    }

    [Fact]
    public void GetNextDelay_DoesNotCountAnAttempt()
    {
        this._strategy.GetNextDelay();

        Assert.Equal(0, this._strategy.Attempts);
    }

    [Fact]
    public void GetNextDelay_AfterOneAttempt_ReturnsDelayInSecondRange()
    {
        this._strategy.BeginAttempt();

        Int32 delay = this._strategy.GetNextDelay();

        Assert.InRange(delay, 1700, 2300); // 2000 * 0.85-1.15
    }

    [Fact]
    public void GetNextDelay_AfterManyAttempts_CapsAtMaxDelay()
    {
        for (Int32 i = 0; i < 20; i++)
        {
            this._strategy.BeginAttempt();
        }

        Int32 delay = this._strategy.GetNextDelay();

        Assert.InRange(delay, 25500, 34500); // 30000 * 0.85-1.15
    }

    [Fact]
    public void GetNextDelay_AfterReset_ReturnsFirstRange()
    {
        this._strategy.BeginAttempt();
        this._strategy.BeginAttempt();
        this._strategy.Reset();

        Int32 delay = this._strategy.GetNextDelay();

        Assert.InRange(delay, 850, 1150);
    }

    // --- Integration: full reconnection sequence ---

    [Fact]
    public void FullSequence_AttemptsEscalateAndReset()
    {
        Int32 delay1 = this._strategy.BeginAttempt();
        Int32 delay2 = this._strategy.BeginAttempt();

        Assert.True(delay2 > delay1); // Delays escalate

        // Simulate successful connection
        this._strategy.Reset();
        Assert.Equal(0, this._strategy.Attempts);

        Int32 delayAfterReset = this._strategy.GetNextDelay();
        Assert.InRange(delayAfterReset, 850, 1150); // Back to base
    }
}
