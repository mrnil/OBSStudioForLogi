namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Loupedeck.OBSStudioForLogiPlugin.Helpers;
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
        this._executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object, action => action());
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

        this._mockObs.Verify(x => x.SetSceneItemEnabled("Scene1", "Source1", It.IsAny<Boolean>()), Times.Once);
    }

    [Fact]
    public void ToggleSourceVisibility_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.ToggleSourceVisibility("Scene1", "Source1");

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

    // --- TryGetSceneItemEnabled (source visibility cache fetch) ---

    [Fact]
    public void TryGetSceneItemEnabled_WhenConnected_ReturnsTrueWithState()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneItemEnabled("Scene1", "Source1")).Returns(true);

        Boolean fetched = this._executor.TryGetSceneItemEnabled("Scene1", "Source1", out Boolean enabled);

        Assert.True(fetched);
        Assert.True(enabled);
    }

    [Fact]
    public void TryGetSceneItemEnabled_WhenNotConnected_ReturnsFalseWithoutQuerying()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        Boolean fetched = this._executor.TryGetSceneItemEnabled("Scene1", "Source1", out Boolean enabled);

        Assert.False(fetched);
        Assert.False(enabled);
        this._mockObs.Verify(x => x.GetSceneItemEnabled(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
    }

    [Theory]
    [InlineData("", "Source1")]
    [InlineData("Scene1", "")]
    public void TryGetSceneItemEnabled_WhenNameEmpty_ReturnsFalseWithoutQuerying(String sceneName, String sourceName)
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);

        Boolean fetched = this._executor.TryGetSceneItemEnabled(sceneName, sourceName, out Boolean _);

        Assert.False(fetched);
        this._mockObs.Verify(x => x.GetSceneItemEnabled(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
    }

    [Fact]
    public void TryGetSceneItemEnabled_WhenOBSThrows_LogsErrorAndReturnsFalse()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneItemEnabled("Scene1", "Source1")).Throws(new Exception("OBS error"));

        Boolean fetched = this._executor.TryGetSceneItemEnabled("Scene1", "Source1", out Boolean enabled);

        Assert.False(fetched);
        Assert.False(enabled);
        this._mockLog.Verify(x => x.Error(It.Is<String>(s => s.Contains("Source1") && s.Contains("OBS error"))), Times.Once);
    }
}

