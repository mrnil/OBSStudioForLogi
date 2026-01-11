namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Moq;
using Xunit;

public class SourceVisibilityTests
{
    private readonly Mock<IOBSWebsocket> _mockObs;
    private readonly Mock<IPluginLog> _mockLog;
    private readonly OBSActionExecutor _executor;

    public SourceVisibilityTests()
    {
        this._mockObs = new Mock<IOBSWebsocket>();
        this._mockLog = new Mock<IPluginLog>();
        this._executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object);
    }

    [Fact]
    public void GetSceneItemList_WhenConnected_ReturnsItems()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneItemList("Scene1")).Returns(new[] { "Source1", "Source2" });

        var result = this._executor.GetSceneItemList("Scene1");

        Assert.Equal(2, result.Length);
        Assert.Contains("Source1", result);
    }

    [Fact]
    public void GetSceneItemList_WhenNotConnected_ReturnsEmpty()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetSceneItemList("Scene1");

        Assert.Empty(result);
    }

    [Fact]
    public void ToggleSourceVisibility_WhenConnected_CallsObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        this._executor.ToggleSourceVisibility("Scene1", "Source1");

        System.Threading.Thread.Sleep(100);
        this._mockObs.Verify(x => x.SetSceneItemEnabled("Scene1", "Source1", It.IsAny<Boolean>()), Times.Once);
    }

    [Fact]
    public void ToggleSourceVisibility_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.ToggleSourceVisibility("Scene1", "Source1");

        System.Threading.Thread.Sleep(100);
        this._mockObs.Verify(x => x.SetSceneItemEnabled(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Boolean>()), Times.Never);
    }

    [Fact]
    public void GetSceneItemEnabled_WhenConnected_ReturnsState()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneItemEnabled("Scene1", "Source1")).Returns(true);

        var result = this._executor.GetSceneItemEnabled("Scene1", "Source1");

        Assert.True(result);
    }

    [Fact]
    public void GetSceneItemEnabled_WhenNotConnected_ReturnsFalse()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        var result = this._executor.GetSceneItemEnabled("Scene1", "Source1");

        Assert.False(result);
    }
}
