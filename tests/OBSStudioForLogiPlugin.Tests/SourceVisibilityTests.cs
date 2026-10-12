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

    private void SetUpSceneItems(params (Int32 ItemId, String SourceName, Boolean Enabled)[] items)
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(true);
        this._mockObs.Setup(x => x.GetSceneItems("Scene1")).Returns(items);
    }

    [Fact]
    public void ToggleSourceVisibility_WhenHidden_ShowsTheItemById()
    {
        this.SetUpSceneItems((3, "Camera", true), (7, "Source1", false));

        this._executor.ToggleSourceVisibility("Scene1", "Source1");

        this._mockObs.Verify(x => x.SetSceneItemEnabled("Scene1", 7, true), Times.Once);
    }

    [Fact]
    public void ToggleSourceVisibility_WhenVisible_HidesTheItemById()
    {
        this.SetUpSceneItems((7, "Source1", true));

        this._executor.ToggleSourceVisibility("Scene1", "Source1");

        this._mockObs.Verify(x => x.SetSceneItemEnabled("Scene1", 7, false), Times.Once);
    }

    // Assessment #30: a toggle used to cost four requests (list + get, list + set).
    [Fact]
    public void ToggleSourceVisibility_MakesOneListRequestAndOneSet()
    {
        this.SetUpSceneItems((7, "Source1", false));

        this._executor.ToggleSourceVisibility("Scene1", "Source1");

        this._mockObs.Verify(x => x.GetSceneItems("Scene1"), Times.Once);
        this._mockObs.Verify(x => x.GetSceneItemEnabled(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
        this._mockObs.Verify(x => x.SetSceneItemEnabled(It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<Boolean>()), Times.Once);
    }

    // Buttons show the source's first item in the scene, so that is the one toggled.
    [Fact]
    public void ToggleSourceVisibility_WhenSourceIsInTheSceneTwice_TogglesTheFirstItem()
    {
        this.SetUpSceneItems((4, "Source1", true), (9, "Source1", false));

        this._executor.ToggleSourceVisibility("Scene1", "Source1");

        this._mockObs.Verify(x => x.SetSceneItemEnabled("Scene1", 4, false), Times.Once);
        this._mockObs.Verify(x => x.SetSceneItemEnabled("Scene1", 9, It.IsAny<Boolean>()), Times.Never);
    }

    [Fact]
    public void ToggleSourceVisibility_WhenSourceNotInScene_WarnsAndDoesNotSet()
    {
        this.SetUpSceneItems((3, "Camera", true));

        this._executor.ToggleSourceVisibility("Scene1", "Source1");

        this._mockObs.Verify(x => x.SetSceneItemEnabled(It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<Boolean>()), Times.Never);
        this._mockLog.Verify(x => x.Warning(It.Is<String>(s => s.Contains("Source1") && s.Contains("not in scene 'Scene1'"))), Times.Once);
    }

    [Fact]
    public void ToggleSourceVisibility_WhenNotConnected_DoesNotCallObs()
    {
        this._mockObs.Setup(x => x.IsConnected).Returns(false);

        this._executor.ToggleSourceVisibility("Scene1", "Source1");

        this._mockObs.Verify(x => x.GetSceneItems(It.IsAny<String>()), Times.Never);
        this._mockObs.Verify(x => x.SetSceneItemEnabled(It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<Boolean>()), Times.Never);
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

    // The redraw after a toggle goes to the callback the manager supplies, not the plugin singleton
    // (assessment #20). It fires after the real OBSTimings.StateUpdateDelay, so wait with a bound.
    [Fact]
    public void ToggleSourceVisibility_WhenSucceeds_ReportsTheToggleToTheCallback()
    {
        System.Threading.Tasks.TaskCompletionSource<(String, String)> toggled = new System.Threading.Tasks.TaskCompletionSource<(String, String)>();
        OBSActionExecutor executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object, action => action(),
            (scene, source) => toggled.TrySetResult((scene, source)));
        this.SetUpSceneItems((7, "Source1", false));

        executor.ToggleSourceVisibility("Scene1", "Source1");

        Assert.True(toggled.Task.Wait(TimeSpan.FromSeconds(5)));
        Assert.Equal(("Scene1", "Source1"), toggled.Task.Result);
    }

    [Fact]
    public void ToggleSourceVisibility_WhenSourceNotInScene_DoesNotReportAToggle()
    {
        Boolean reported = false;
        OBSActionExecutor executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object, action => action(),
            (scene, source) => reported = true);
        this.SetUpSceneItems((3, "Camera", true));

        executor.ToggleSourceVisibility("Scene1", "Source1");

        Assert.False(reported);
    }

    [Fact]
    public void ToggleSourceVisibility_WhenOBSThrows_DoesNotReportAToggle()
    {
        Boolean reported = false;
        OBSActionExecutor executor = new OBSActionExecutor(this._mockObs.Object, this._mockLog.Object, action => action(),
            (scene, source) => reported = true);
        this.SetUpSceneItems((7, "Source1", false));
        this._mockObs.Setup(x => x.SetSceneItemEnabled(It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<Boolean>())).Throws(new Exception("OBS error"));

        executor.ToggleSourceVisibility("Scene1", "Source1");

        // The notification would be scheduled synchronously after the set, so nothing is pending.
        Assert.False(reported);
    }
}
