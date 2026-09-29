namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Threading.Tasks;
using Moq;
using OBSWebsocketDotNet;

public class InitialStateLoaderTests
{
    private readonly Mock<IPluginLog> _mockLog = new Mock<IPluginLog>();
    private readonly List<Int32> _delays = new List<Int32>();
    private Boolean _isCurrent = true;
    private Int32 _loads;

    private InitialStateLoader CreateLoader(Int32 maxAttempts = 5)
    {
        return new InitialStateLoader(this._mockLog.Object, delayMs =>
        {
            this._delays.Add(delayMs);
            return Task.CompletedTask;
        }, 500, maxAttempts);
    }

    private static ErrorResponseException NotReady() => new ErrorResponseException("OBS is not ready to perform the request.", 207);

    // Throws "not ready" (or error) for the first `failures` calls, then succeeds.
    private Action LoadFailingFirst(Int32 failures, Exception? error = null)
    {
        return () =>
        {
            this._loads++;
            if (this._loads <= failures)
            {
                throw error ?? NotReady();
            }
        };
    }

    [Fact]
    public async Task RunAsync_WhenLoadSucceeds_RunsOnceAndReturnsTrue()
    {
        InitialStateLoader loader = this.CreateLoader();

        Boolean loaded = await loader.RunAsync(this.LoadFailingFirst(0), () => this._isCurrent);

        Assert.True(loaded);
        Assert.Equal(1, this._loads);
        Assert.Empty(this._delays);
    }

    // OBS accepts connections while starting and answers "not ready" until its scene collection
    // has loaded; the load used to give up on the first attempt.
    [Fact]
    public async Task RunAsync_WhileOBSNotReady_RetriesAfterDelayUntilItSucceeds()
    {
        InitialStateLoader loader = this.CreateLoader();

        Boolean loaded = await loader.RunAsync(this.LoadFailingFirst(3), () => this._isCurrent);

        Assert.True(loaded);
        Assert.Equal(4, this._loads);
        Assert.Equal(new[] { 500, 500, 500 }, this._delays);
        this._mockLog.Verify(x => x.Info(It.Is<String>(s => s.Contains("attempt 4"))), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhileOBSNotReady_LogsTheWaitOnceAtInfo()
    {
        InitialStateLoader loader = this.CreateLoader();

        await loader.RunAsync(this.LoadFailingFirst(3), () => this._isCurrent);

        this._mockLog.Verify(x => x.Info(It.Is<String>(s => s.Contains("not ready"))), Times.Once);
        this._mockLog.Verify(x => x.Debug(It.Is<String>(s => s.Contains("not ready"))), Times.Exactly(2));
    }

    [Fact]
    public async Task RunAsync_WhenOBSNeverReady_GivesUpAfterMaxAttemptsWithWarning()
    {
        InitialStateLoader loader = this.CreateLoader(maxAttempts: 3);

        Boolean loaded = await loader.RunAsync(this.LoadFailingFirst(Int32.MaxValue), () => this._isCurrent);

        Assert.False(loaded);
        Assert.Equal(3, this._loads);
        Assert.Equal(2, this._delays.Count);
        this._mockLog.Verify(x => x.Warning(It.Is<String>(s => s.Contains("Failed to get initial state") && s.Contains("3 attempts"))), Times.Once);
    }

    // Only "not ready" is known to clear up by itself; other failures keep the old single attempt.
    [Fact]
    public async Task RunAsync_WhenLoadFailsForAnotherReason_DoesNotRetry()
    {
        InitialStateLoader loader = this.CreateLoader();

        Boolean loaded = await loader.RunAsync(this.LoadFailingFirst(1, new InvalidOperationException("OBS error")), () => this._isCurrent);

        Assert.False(loaded);
        Assert.Equal(1, this._loads);
        this._mockLog.Verify(x => x.Warning(It.Is<String>(s => s.Contains("Failed to get initial state") && s.Contains("OBS error"))), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenAnotherOBSErrorCode_DoesNotRetry()
    {
        InitialStateLoader loader = this.CreateLoader();

        Boolean loaded = await loader.RunAsync(this.LoadFailingFirst(1, new ErrorResponseException("Resource not found", 600)), () => this._isCurrent);

        Assert.False(loaded);
        Assert.Equal(1, this._loads);
    }

    [Fact]
    public async Task RunAsync_WhenConnectionClosesWhileWaiting_StopsRetrying()
    {
        InitialStateLoader loader = new InitialStateLoader(this._mockLog.Object, delayMs =>
        {
            this._isCurrent = false;
            return Task.CompletedTask;
        }, 500, 5);

        Boolean loaded = await loader.RunAsync(this.LoadFailingFirst(Int32.MaxValue), () => this._isCurrent);

        Assert.False(loaded);
        Assert.Equal(1, this._loads);
    }

    [Fact]
    public async Task RunAsync_WhenConnectionAlreadyClosed_DoesNotLoad()
    {
        InitialStateLoader loader = this.CreateLoader();
        this._isCurrent = false;

        Boolean loaded = await loader.RunAsync(this.LoadFailingFirst(0), () => this._isCurrent);

        Assert.False(loaded);
        Assert.Equal(0, this._loads);
    }

    [Fact]
    public void IsNotReady_OnlyForErrorCode207()
    {
        Assert.True(InitialStateLoader.IsNotReady(NotReady()));
        Assert.False(InitialStateLoader.IsNotReady(new ErrorResponseException("other", 600)));
        Assert.False(InitialStateLoader.IsNotReady(new InvalidOperationException("OBS error")));
    }

    [Fact]
    public void Constructor_WhenLogNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new InitialStateLoader(null!));
    }
}
