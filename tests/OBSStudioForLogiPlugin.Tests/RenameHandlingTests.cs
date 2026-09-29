namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Collections.Generic;
using Loupedeck.OBSStudioForLogiPlugin.Helpers;
using Loupedeck.OBSStudioForLogiPlugin.Models;
using Moq;

// OBS scene and input renames (assessment #21): every cache keyed by name moves the old name's
// state to the new name, so renamed buttons keep showing the right state without a refetch.
public class KeyedStateCacheRenameTests
{
    private readonly Queue<Action> _background = new Queue<Action>();
    private readonly List<(String Scene, String Source)> _fetchRequests = new List<(String Scene, String Source)>();
    private readonly KeyedStateCache<(String Scene, String Source), Boolean> _cache;
    private Boolean _fetchSucceeds = true;

    public KeyedStateCacheRenameTests()
    {
        this._cache = new KeyedStateCache<(String Scene, String Source), Boolean>(
            this.Fetch,
            _ => { },
            false,
            "source visibility",
            action => this._background.Enqueue(action),
            () => new DateTime(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc));
    }

    private Boolean Fetch((String Scene, String Source) key, out Boolean value)
    {
        this._fetchRequests.Add(key);
        value = true;
        return this._fetchSucceeds;
    }

    private void RunBackgroundWork()
    {
        while (this._background.Count > 0)
        {
            this._background.Dequeue()();
        }
    }

    private static (String Scene, String Source) RenameSource((String Scene, String Source) key) =>
        key.Source == "Camera" ? (key.Scene, "Webcam") : key;

    [Fact]
    public void RenameKeys_MovesValueToNewKeyWithoutFetching()
    {
        this._cache.Set(("Main", "Camera"), true);

        this._cache.RenameKeys(RenameSource);

        Assert.True(this._cache.Get(("Main", "Webcam")));
        Assert.Empty(this._background);
    }

    [Fact]
    public void RenameKeys_RemovesOldKey()
    {
        this._cache.Set(("Main", "Camera"), true);

        this._cache.RenameKeys(RenameSource);

        Assert.False(this._cache.Get(("Main", "Camera")));
        Assert.Single(this._background);
    }

    [Fact]
    public void RenameKeys_LeavesUnaffectedKeysAlone()
    {
        this._cache.Set(("Main", "Microphone"), true);

        this._cache.RenameKeys(RenameSource);

        Assert.True(this._cache.Get(("Main", "Microphone")));
    }

    [Fact]
    public void RenameKeys_WhenNewKeyAlreadyHasValue_KeepsNewerValue()
    {
        this._cache.Set(("Main", "Camera"), true);
        this._cache.Set(("Main", "Webcam"), false);

        this._cache.RenameKeys(RenameSource);

        Assert.False(this._cache.Get(("Main", "Webcam")));
        Assert.Empty(this._background);
    }

    [Fact]
    public void RenameKeys_DropsFetchInFlightForOldKey()
    {
        this._cache.Get(("Main", "Camera"));

        this._cache.RenameKeys(RenameSource);
        this.RunBackgroundWork();
        this._fetchRequests.Clear();

        // The dropped result wasn't stored, so the old key is still a miss.
        this._cache.Get(("Main", "Camera"));
        Assert.Single(this._background);
    }

    [Fact]
    public void RenameKeys_ClearsFailureBackoffForOldKey()
    {
        this._fetchSucceeds = false;
        this._cache.Get(("Main", "Camera"));
        this.RunBackgroundWork();

        this._cache.RenameKeys(RenameSource);
        this._cache.Get(("Main", "Camera"));

        Assert.Single(this._background);
    }

    [Fact]
    public void RenameKeys_WhenRenameNull_DoesNotThrow()
    {
        this._cache.Set(("Main", "Camera"), true);

        Exception exception = Record.Exception(() => this._cache.RenameKeys(null!));

        Assert.Null(exception);
        Assert.True(this._cache.Get(("Main", "Camera")));
    }
}

public class AudioStateCacheRenameTests
{
    private readonly Queue<Action> _background = new Queue<Action>();
    private readonly AudioStateCache _cache;

    public AudioStateCacheRenameTests()
    {
        this._cache = new AudioStateCache(
            _ => new AudioInputState { IsMuted = false, VolumeMul = 1.0f, MonitorType = AudioInputState.MonitorTypeNone },
            _ => { },
            action => this._background.Enqueue(action),
            () => new DateTime(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc));
    }

    private void SetComplete(String inputName, Boolean isMuted, Single volumeMul)
    {
        this._cache.SetMuted(inputName, isMuted);
        this._cache.SetVolume(inputName, volumeMul);
        this._cache.SetMonitorType(inputName, AudioInputState.MonitorTypeNone);
    }

    [Fact]
    public void Rename_MovesStateToNewNameWithoutFetching()
    {
        this.SetComplete("Mic", true, 0.25f);

        this._cache.Rename("Mic", "Microphone");
        AudioInputState state = this._cache.Get("Microphone");

        Assert.True(state.IsMuted);
        Assert.Equal(0.25f, state.VolumeMul);
        Assert.Empty(this._background);
    }

    [Fact]
    public void Rename_RemovesOldName()
    {
        this.SetComplete("Mic", true, 0.25f);

        this._cache.Rename("Mic", "Microphone");
        AudioInputState state = this._cache.Get("Mic");

        Assert.False(state.IsMuted);
        Assert.Single(this._background);
    }

    [Fact]
    public void Rename_WhenNewNameAlreadyHasState_KeepsNewerState()
    {
        this.SetComplete("Mic", true, 0.25f);
        this.SetComplete("Microphone", false, 0.75f);

        this._cache.Rename("Mic", "Microphone");
        AudioInputState state = this._cache.Get("Microphone");

        Assert.False(state.IsMuted);
        Assert.Equal(0.75f, state.VolumeMul);
    }

    [Fact]
    public void Rename_DropsFetchInFlightForOldName()
    {
        this._cache.Get("Mic");

        this._cache.Rename("Mic", "Microphone");
        this._background.Dequeue()();

        this._cache.Get("Mic");
        Assert.Single(this._background);
    }

    [Theory]
    [InlineData("", "Microphone")]
    [InlineData("Mic", "")]
    [InlineData("Mic", "Mic")]
    public void Rename_WhenNamesInvalidOrEqual_LeavesStateAlone(String oldName, String newName)
    {
        this.SetComplete("Mic", true, 0.25f);

        this._cache.Rename(oldName, newName);

        Assert.True(this._cache.Get("Mic").IsMuted);
    }
}

public class AudioMeterServiceRenameTests
{
    private readonly AudioMeterService _service;

    public AudioMeterServiceRenameTests()
    {
        DateTime now = new DateTime(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);
        this._service = new AudioMeterService(() => now);
    }

    private static AudioMeterLevels Levels(params Single[] peaks) => new AudioMeterLevels { ChannelPeaks = peaks };

    [Fact]
    public void RenameInput_MovesLevelsAndKeepsLiveOrder()
    {
        this._service.UpdateLevels("Mic", Levels(0.5f));
        this._service.UpdateLevels("Desktop", Levels(0.2f));

        this._service.RenameInput("Mic", "Microphone");

        Assert.Equal(new[] { "Microphone", "Desktop" }, this._service.GetLiveInputs());
        Assert.Equal(new[] { 0.5f }, this._service.GetLevels("Microphone").ChannelPeaks);
        Assert.False(this._service.GetLevels("Mic").HasData);
    }

    [Fact]
    public void RenameInput_MovesMuteState()
    {
        this._service.SetMuted("Mic", true);

        this._service.RenameInput("Mic", "Microphone");

        Assert.True(this._service.GetLevels("Microphone").IsMuted);
        Assert.False(this._service.GetLevels("Mic").IsMuted);
    }

    [Fact]
    public void RenameInput_WhenMuteStateMoved_DoesNotStartNewLookup()
    {
        this._service.SetMuted("Mic", true);

        this._service.RenameInput("Mic", "Microphone");

        Assert.False(this._service.TryBeginMuteLookup("Microphone"));
    }

    [Fact]
    public void RenameInput_WhenNewNameAlreadyReported_KeepsNewerLevels()
    {
        this._service.UpdateLevels("Mic", Levels(0.5f));
        this._service.UpdateLevels("Microphone", Levels(0.9f));

        this._service.RenameInput("Mic", "Microphone");

        Assert.Equal(new[] { "Microphone" }, this._service.GetLiveInputs());
        Assert.Equal(new[] { 0.9f }, this._service.GetLevels("Microphone").ChannelPeaks);
    }
}

[Collection(AudioSelectionStateCollection.Name)]
public class AudioSelectionStateRenameTests : IDisposable
{
    public AudioSelectionStateRenameTests()
    {
        AudioSelectionState.Deselect();
    }

    public void Dispose()
    {
        AudioSelectionState.Deselect();
    }

    [Fact]
    public void RenameIfMatches_WhenSelected_MovesSelectionAndRaisesChange()
    {
        AudioSelectionState.Select("Mic");
        List<(String? Previous, String? Current)> changes = new List<(String? Previous, String? Current)>();
        Action<String, String> handler = (previous, current) => changes.Add((previous, current));
        AudioSelectionState.SelectionChanged += handler;

        try
        {
            AudioSelectionState.RenameIfMatches("Mic", "Microphone");
        }
        finally
        {
            AudioSelectionState.SelectionChanged -= handler;
        }

        Assert.Equal("Microphone", AudioSelectionState.SelectedInput);
        Assert.Equal(new[] { ((String?)"Mic", (String?)"Microphone") }, changes);
    }

    [Fact]
    public void RenameIfMatches_WhenOtherInputSelected_LeavesSelection()
    {
        AudioSelectionState.Select("Desktop");

        AudioSelectionState.RenameIfMatches("Mic", "Microphone");

        Assert.Equal("Desktop", AudioSelectionState.SelectedInput);
    }

    [Fact]
    public void RenameIfMatches_WhenNothingSelected_LeavesNothingSelected()
    {
        AudioSelectionState.RenameIfMatches("Mic", "Microphone");

        Assert.Null(AudioSelectionState.SelectedInput);
    }
}

[Collection(AudioSelectionStateCollection.Name)]
public class OBSWebSocketManagerRenameTests : IDisposable
{
    private readonly OBSWebSocketManager _manager;

    public OBSWebSocketManagerRenameTests()
    {
        this._manager = new OBSWebSocketManager(new Mock<IPluginLog>().Object);
        AudioSelectionState.Deselect();
    }

    public void Dispose()
    {
        AudioSelectionState.Deselect();
        this._manager.Dispose();
    }

    [Fact]
    public void ApplyInputRename_MovesAudioMediaAndVisibilityState()
    {
        this._manager.AudioState.SetMuted("Mic", true);
        this._manager.AudioState.SetVolume("Mic", 0.5f);
        this._manager.AudioState.SetMonitorType("Mic", AudioInputState.MonitorTypeNone);
        this._manager.MediaState.Set("Mic", MediaInputStates.Playing);
        this._manager.SourceVisibility.Set(("Main", "Mic"), true);

        this._manager.ApplyInputRename("Mic", "Microphone");

        Assert.True(this._manager.AudioState.Get("Microphone").IsMuted);
        Assert.Equal(MediaInputStates.Playing, this._manager.MediaState.Get("Microphone"));
        Assert.True(this._manager.SourceVisibility.Get(("Main", "Microphone")));
    }

    [Fact]
    public void ApplyInputRename_MovesMeterLevels()
    {
        this._manager.AudioMeters.UpdateLevels("Mic", new AudioMeterLevels { ChannelPeaks = new[] { 0.5f } });

        this._manager.ApplyInputRename("Mic", "Microphone");

        Assert.Equal(new[] { "Microphone" }, this._manager.AudioMeters.GetLiveInputs());
    }

    [Fact]
    public void ApplyInputRename_KeepsAudioSelectionOnRenamedInput()
    {
        AudioSelectionState.Select("Mic");

        this._manager.ApplyInputRename("Mic", "Microphone");

        Assert.Equal("Microphone", AudioSelectionState.SelectedInput);
    }

    [Fact]
    public void ApplySceneRename_WhenCurrentScene_UpdatesCurrentSceneAndReturnsTrue()
    {
        this._manager.Actions.SetCurrentSceneState("Main");

        Boolean wasCurrent = this._manager.ApplySceneRename("Main", "Live");

        Assert.True(wasCurrent);
        Assert.Equal("Live", this._manager.Actions.CurrentScene);
    }

    [Fact]
    public void ApplySceneRename_WhenNotCurrentScene_LeavesCurrentSceneAndReturnsFalse()
    {
        this._manager.Actions.SetCurrentSceneState("Main");

        Boolean wasCurrent = this._manager.ApplySceneRename("Backup", "Standby");

        Assert.False(wasCurrent);
        Assert.Equal("Main", this._manager.Actions.CurrentScene);
    }

    [Fact]
    public void ApplySceneRename_RenamesSceneAndNestedSceneSourceInVisibilityKeys()
    {
        this._manager.SourceVisibility.Set(("Main", "Camera"), true);
        this._manager.SourceVisibility.Set(("Overlay", "Main"), true);

        this._manager.ApplySceneRename("Main", "Live");

        Assert.True(this._manager.SourceVisibility.Get(("Live", "Camera")));
        Assert.True(this._manager.SourceVisibility.Get(("Overlay", "Live")));
    }
}
