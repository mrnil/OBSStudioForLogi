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
    public void Generation_ChangesEachTimeASessionOpens()
    {
        SessionGate gate = new SessionGate();
        gate.TryOpen();
        Int64 first = gate.Generation;

        gate.TryOpen();
        Int64 afterRepeat = gate.Generation;
        gate.Close();
        gate.TryOpen();

        Assert.Equal(first, afterRepeat);
        Assert.NotEqual(first, gate.Generation);
    }

    [Fact]
    public void IsCurrent_WhileSessionOpen_ReturnsTrue()
    {
        SessionGate gate = new SessionGate();
        gate.TryOpen();

        Assert.True(gate.IsCurrent(gate.Generation));
    }

    [Fact]
    public void IsCurrent_AfterClose_ReturnsFalse()
    {
        SessionGate gate = new SessionGate();
        gate.TryOpen();
        Int64 session = gate.Generation;

        gate.Close();

        Assert.False(gate.IsCurrent(session));
    }

    // Work started for a connection must stop when it drops, even if a new one has opened since.
    [Fact]
    public void IsCurrent_AfterReconnect_ReturnsFalseForOldSession()
    {
        SessionGate gate = new SessionGate();
        gate.TryOpen();
        Int64 oldSession = gate.Generation;

        gate.Close();
        gate.TryOpen();

        Assert.False(gate.IsCurrent(oldSession));
        Assert.True(gate.IsCurrent(gate.Generation));
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
