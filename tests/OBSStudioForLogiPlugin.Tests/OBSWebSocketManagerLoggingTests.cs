namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Moq;

public class OBSWebSocketManagerLoggingTests
{
    [Fact]
    public void ConnectAsync_ShouldLogConnectionAttempt()
    {
        var mockLog = new Mock<IPluginLog>();
        var manager = new OBSWebSocketManager(mockLog.Object);
        
        _ = manager.ConnectAsync("ws://localhost:4455", "test");
        
        mockLog.Verify(x => x.Info(It.Is<String>(s => s.Contains("Connecting") || s.Contains("connect"))), Times.Once);
    }

    [Fact]
    public void Disconnect_ShouldLogDisconnection()
    {
        var mockLog = new Mock<IPluginLog>();
        var manager = new OBSWebSocketManager(mockLog.Object);
        
        manager.Disconnect();
        
        mockLog.Verify(x => x.Info(It.Is<String>(s => s.Contains("Disconnect") || s.Contains("disconnect"))), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldLogDisposal()
    {
        var mockLog = new Mock<IPluginLog>();
        var manager = new OBSWebSocketManager(mockLog.Object);
        
        manager.Dispose();
        
        mockLog.Verify(x => x.Info(It.Is<String>(s => s.Contains("Dispos") || s.Contains("dispos"))), Times.AtLeastOnce);
    }
}
