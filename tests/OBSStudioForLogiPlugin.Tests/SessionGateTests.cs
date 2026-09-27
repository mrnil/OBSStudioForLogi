namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

public class SessionGateTests
{
    [Fact]
    public void TryOpen_WhenClosed_ReturnsTrue()
    {
        SessionGate gate = new SessionGate();

        Assert.True(gate.TryOpen());
        Assert.True(gate.IsOpen);
    }

    [Fact]
    public void TryOpen_WhenAlreadyOpen_ReturnsFalse()
    {
        SessionGate gate = new SessionGate();
        gate.TryOpen();

        Assert.False(gate.TryOpen());
        Assert.True(gate.IsOpen);
    }

    [Fact]
    public void TryOpen_AfterClose_ReturnsTrueAgain()
    {
        SessionGate gate = new SessionGate();
        gate.TryOpen();

        gate.Close();

        Assert.False(gate.IsOpen);
        Assert.True(gate.TryOpen());
    }

    [Fact]
    public void TryOpen_CalledConcurrently_OpensExactlyOnce()
    {
        SessionGate gate = new SessionGate();
        Int32 opened = 0;

        Parallel.For(0, 64, _ =>
        {
            if (gate.TryOpen())
            {
                Interlocked.Increment(ref opened);
            }
        });

        Assert.Equal(1, opened);
    }
}
