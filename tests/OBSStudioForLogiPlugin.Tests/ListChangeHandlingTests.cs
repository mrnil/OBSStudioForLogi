namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Collections.Generic;
using Moq;

// ProfileListChanged / SceneCollectionListChanged (assessment #8): the new list goes straight to
// the commands, unless the current item is missing from it (it was renamed), in which case the
// current item is read again first. What reaches the commands can't be observed until #20 removes
// the static plugin instance, so these tests check the path taken through the log.
public class ListChangeHandlingTests
{
    private readonly Mock<IPluginLog> _mockLog = new Mock<IPluginLog>();
    private readonly OBSWebSocketManager _manager;

    public ListChangeHandlingTests()
    {
        this._manager = new OBSWebSocketManager(this._mockLog.Object);
    }

    [Fact]
    public void ApplyProfileList_CurrentProfileListed_PushesListWithoutRereading()
    {
        this._manager.Actions.SetCurrentProfileState("Streaming");

        this._manager.ApplyProfileList(new List<String> { "Streaming", "Recording" });

        this._mockLog.Verify(x => x.Debug("Loaded 2 profiles"), Times.Once);
        this._mockLog.Verify(x => x.Info(It.Is<String>(s => s.Contains("no longer listed"))), Times.Never);
    }

    [Fact]
    public void ApplyProfileList_NoCurrentProfileYet_PushesList()
    {
        this._manager.ApplyProfileList(new List<String> { "Streaming" });

        this._mockLog.Verify(x => x.Debug("Loaded 1 profiles"), Times.Once);
    }

    [Fact]
    public void ApplyProfileList_NullList_PushesEmptyList()
    {
        this._manager.ApplyProfileList(null);

        this._mockLog.Verify(x => x.Debug("Loaded 0 profiles"), Times.Once);
    }

    [Fact]
    public void ApplyProfileList_CurrentProfileMissing_RereadsCurrentProfile()
    {
        this._manager.Actions.SetCurrentProfileState("Streaming");

        this._manager.ApplyProfileList(new List<String> { "Streaming 2", "Recording" });

        this._mockLog.Verify(x => x.Info("Current profile 'Streaming' is no longer listed, reading it again"), Times.Once);
        this._mockLog.Verify(x => x.Debug("Loaded 2 profiles"), Times.Never);
    }

    [Fact]
    public void ApplySceneCollectionList_CurrentCollectionListed_PushesListWithoutRereading()
    {
        this._manager.Actions.SetCurrentSceneCollectionState("Main");

        this._manager.ApplySceneCollectionList(new List<String> { "Main", "Podcast", "Gaming" });

        this._mockLog.Verify(x => x.Debug("Loaded 3 scene collections"), Times.Once);
        this._mockLog.Verify(x => x.Info(It.Is<String>(s => s.Contains("no longer listed"))), Times.Never);
    }

    [Fact]
    public void ApplySceneCollectionList_NullList_PushesEmptyList()
    {
        this._manager.ApplySceneCollectionList(null);

        this._mockLog.Verify(x => x.Debug("Loaded 0 scene collections"), Times.Once);
    }

    // Creating or deleting a collection only sends CurrentSceneCollectionChanged, so the list is
    // re-read then. If that read fails, the folder keeps its last list rather than going empty.
    [Fact]
    public void RefreshSceneCollectionList_RequestFails_KeepsTheLastListAndWarns()
    {
        this._manager.RefreshSceneCollectionList("Main");

        this._mockLog.Verify(x => x.Warning(It.Is<String>(s => s.StartsWith("Failed to refresh the scene collection list"))), Times.Once);
        this._mockLog.Verify(x => x.Debug(It.Is<String>(s => s.EndsWith("scene collections"))), Times.Never);
    }

    [Fact]
    public void ApplySceneCollectionList_CurrentCollectionMissing_RereadsCurrentCollection()
    {
        this._manager.Actions.SetCurrentSceneCollectionState("Main");

        this._manager.ApplySceneCollectionList(new List<String> { "Main Show", "Podcast" });

        this._mockLog.Verify(x => x.Info("Current scene collection 'Main' is no longer listed, reading it again"), Times.Once);
        this._mockLog.Verify(x => x.Debug("Loaded 2 scene collections"), Times.Never);
    }
}
