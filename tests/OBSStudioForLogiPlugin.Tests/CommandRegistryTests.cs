namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System;
using Moq;

public class CommandRegistryTests
{
    private readonly CommandRegistry _registry;

    public CommandRegistryTests()
    {
        this._registry = new CommandRegistry();
    }

    // --- Registration tests ---

    [Fact]
    public void Register_NullCommand_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._registry.Register(null));

        Assert.Null(exception);
    }

    [Fact]
    public void Register_ValidCommand_AddsCommand()
    {
        var mockCommand = new Mock<IObsCommand>();

        this._registry.Register(mockCommand.Object);
        this._registry.NotifyConnected();

        mockCommand.Verify(x => x.OnConnected(), Times.Once);
    }

    [Fact]
    public void Register_DuplicateCommand_DoesNotAddTwice()
    {
        var mockCommand = new Mock<IObsCommand>();

        this._registry.Register(mockCommand.Object);
        this._registry.Register(mockCommand.Object);
        this._registry.NotifyConnected();

        mockCommand.Verify(x => x.OnConnected(), Times.Once);
    }

    // --- NotifyConnected / NotifyDisconnected ---

    [Fact]
    public void NotifyConnected_CallsOnConnectedOnAllCommands()
    {
        var mock1 = new Mock<IObsCommand>();
        var mock2 = new Mock<IObsCommand>();
        this._registry.Register(mock1.Object);
        this._registry.Register(mock2.Object);

        this._registry.NotifyConnected();

        mock1.Verify(x => x.OnConnected(), Times.Once);
        mock2.Verify(x => x.OnConnected(), Times.Once);
    }

    [Fact]
    public void NotifyDisconnected_CallsOnDisconnectedOnAllCommands()
    {
        var mock1 = new Mock<IObsCommand>();
        var mock2 = new Mock<IObsCommand>();
        this._registry.Register(mock1.Object);
        this._registry.Register(mock2.Object);

        this._registry.NotifyDisconnected();

        mock1.Verify(x => x.OnDisconnected(), Times.Once);
        mock2.Verify(x => x.OnDisconnected(), Times.Once);
    }

    [Fact]
    public void NotifyConnected_WithNoCommands_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._registry.NotifyConnected());

        Assert.Null(exception);
    }

    // --- NotifyProfileChanged ---

    [Fact]
    public void NotifyProfileChanged_CallsOnlyIProfileAwareCommands()
    {
        var profileAware = new Mock<IProfileAwareCommand>();
        var nonProfileAware = new Mock<IObsCommand>();
        this._registry.Register(profileAware.Object);
        this._registry.Register(nonProfileAware.Object);

        this._registry.NotifyProfileChanged("old", "new");

        profileAware.Verify(x => x.OnProfileChanged("old", "new"), Times.Once);
    }

    [Fact]
    public void NotifyProfileChanged_DoesNotCallNonProfileAwareCommands()
    {
        var nonProfileAware = new Mock<IObsCommand>();
        this._registry.Register(nonProfileAware.Object);

        var exception = Record.Exception(() => this._registry.NotifyProfileChanged("old", "new"));

        Assert.Null(exception);
    }

    // --- NotifySceneCollectionChanged ---

    [Fact]
    public void NotifySceneCollectionChanged_CallsOnlyISceneCollectionAwareCommands()
    {
        var sceneCollectionAware = new Mock<ISceneCollectionAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(sceneCollectionAware.Object);
        this._registry.Register(nonAware.Object);

        this._registry.NotifySceneCollectionChanged("old", "new");

        sceneCollectionAware.Verify(x => x.OnSceneCollectionChanged("old", "new"), Times.Once);
    }

    // --- NotifySceneChanged ---

    [Fact]
    public void NotifySceneChanged_CallsOnlyISceneAwareCommands()
    {
        var sceneAware = new Mock<ISceneAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(sceneAware.Object);
        this._registry.Register(nonAware.Object);

        this._registry.NotifySceneChanged("Scene 1");

        sceneAware.Verify(x => x.OnSceneChanged("Scene 1"), Times.Once);
    }

    // --- NotifyScenesChanged ---

    [Fact]
    public void NotifyScenesChanged_CallsOnlyIScenesListAwareCommands()
    {
        var scenesListAware = new Mock<IScenesListAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(scenesListAware.Object);
        this._registry.Register(nonAware.Object);

        var scenes = new[] { "Scene 1", "Scene 2" };
        this._registry.NotifyScenesChanged(scenes);

        scenesListAware.Verify(x => x.OnScenesChanged(scenes), Times.Once);
    }

    // --- NotifyProfilesChanged ---

    [Fact]
    public void NotifyProfilesChanged_CallsOnlyIProfilesListAwareCommands()
    {
        var profilesListAware = new Mock<IProfilesListAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(profilesListAware.Object);
        this._registry.Register(nonAware.Object);

        var profiles = new[] { "Profile 1", "Profile 2" };
        this._registry.NotifyProfilesChanged(profiles, "Profile 1");

        profilesListAware.Verify(x => x.OnProfilesChanged(profiles, "Profile 1"), Times.Once);
    }

    // --- NotifySourceVisibilityChanged ---

    [Fact]
    public void NotifySourceVisibilityChanged_CallsOnlyISourceVisibilityAwareCommands()
    {
        var visibilityAware = new Mock<ISourceVisibilityAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(visibilityAware.Object);
        this._registry.Register(nonAware.Object);

        this._registry.NotifySourceVisibilityChanged("Scene 1", "Source 1");

        visibilityAware.Verify(x => x.OnSourceVisibilityChanged("Scene 1", "Source 1"), Times.Once);
    }

    // --- NotifyInputMuteChanged ---

    [Fact]
    public void NotifyInputMuteChanged_CallsOnlyIInputMuteAwareCommands()
    {
        var muteAware = new Mock<IInputMuteAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(muteAware.Object);
        this._registry.Register(nonAware.Object);

        this._registry.NotifyInputMuteChanged("Microphone");

        muteAware.Verify(x => x.OnInputMuteChanged("Microphone"), Times.Once);
    }

    // --- NotifyInputVolumeChanged ---

    [Fact]
    public void NotifyInputVolumeChanged_CallsOnlyIInputVolumeAwareCommands()
    {
        var volumeAware = new Mock<IInputVolumeAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(volumeAware.Object);
        this._registry.Register(nonAware.Object);

        this._registry.NotifyInputVolumeChanged("Desktop Audio");

        volumeAware.Verify(x => x.OnInputVolumeChanged("Desktop Audio"), Times.Once);
    }

    // --- NotifyInputsChanged ---

    [Fact]
    public void NotifyInputsChanged_CallsOnlyIInputsListAwareCommands()
    {
        var inputsListAware = new Mock<IInputsListAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(inputsListAware.Object);
        this._registry.Register(nonAware.Object);

        var inputs = new[] { "Mic", "Desktop" };
        this._registry.NotifyInputsChanged(inputs);

        inputsListAware.Verify(x => x.OnInputsChanged(inputs), Times.Once);
    }

    // --- NotifyVirtualCameraStateChanged ---

    [Fact]
    public void NotifyVirtualCameraStateChanged_CallsOnlyIVirtualCameraAwareCommands()
    {
        var vcamAware = new Mock<IVirtualCameraAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(vcamAware.Object);
        this._registry.Register(nonAware.Object);

        this._registry.NotifyVirtualCameraStateChanged();

        vcamAware.Verify(x => x.OnVirtualCameraStateChanged(), Times.Once);
    }

    // --- NotifyReplayBufferStateChanged ---

    [Fact]
    public void NotifyReplayBufferStateChanged_CallsOnlyIReplayBufferAwareCommands()
    {
        var replayAware = new Mock<IReplayBufferAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(replayAware.Object);
        this._registry.Register(nonAware.Object);

        this._registry.NotifyReplayBufferStateChanged();

        replayAware.Verify(x => x.OnReplayBufferStateChanged(), Times.Once);
    }

    // --- NotifyStudioModeStateChanged ---

    [Fact]
    public void NotifyStudioModeStateChanged_CallsOnlyIStudioModeAwareCommands()
    {
        var studioModeAware = new Mock<IStudioModeAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(studioModeAware.Object);
        this._registry.Register(nonAware.Object);

        this._registry.NotifyStudioModeStateChanged();

        studioModeAware.Verify(x => x.OnStudioModeStateChanged(), Times.Once);
    }

    // --- NotifyInputMonitorTypeChanged ---

    [Fact]
    public void NotifyInputMonitorTypeChanged_CallsOnlyIInputMonitorAwareCommands()
    {
        var monitorAware = new Mock<IInputMonitorAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(monitorAware.Object);
        this._registry.Register(nonAware.Object);

        this._registry.NotifyInputMonitorTypeChanged("Microphone");

        monitorAware.Verify(x => x.OnInputMonitorTypeChanged("Microphone"), Times.Once);
    }

    [Fact]
    public void NotifyInputMonitorTypeChanged_DoesNotCallNonMonitorAwareCommands()
    {
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(nonAware.Object);

        var exception = Record.Exception(() => this._registry.NotifyInputMonitorTypeChanged("Mic"));

        Assert.Null(exception);
    }

    // --- NotifySceneSourcesChanged ---

    [Fact]
    public void NotifySceneSourcesChanged_CallsOnlyISceneSourcesAwareCommands()
    {
        var sceneSourcesAware = new Mock<ISceneSourcesAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(sceneSourcesAware.Object);
        this._registry.Register(nonAware.Object);

        var sources = new[] { "Camera", "Screen" };
        var audioSources = new[] { "Mic", "Desktop" };
        this._registry.NotifySceneSourcesChanged("Scene 1", sources, audioSources);

        sceneSourcesAware.Verify(x => x.OnSceneSourcesChanged("Scene 1", sources, audioSources), Times.Once);
    }

    [Fact]
    public void NotifySceneSourcesChanged_DoesNotCallNonSceneSourcesAwareCommands()
    {
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(nonAware.Object);

        var exception = Record.Exception(() => this._registry.NotifySceneSourcesChanged("Scene 1", new String[0], new String[0]));

        Assert.Null(exception);
    }

    // --- NotifyReplayBufferSaved ---

    [Fact]
    public void NotifyReplayBufferSaved_CallsOnlyIReplayBufferSavedAwareCommands()
    {
        var savedAware = new Mock<IReplayBufferSavedAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(savedAware.Object);
        this._registry.Register(nonAware.Object);

        this._registry.NotifyReplayBufferSaved("/path/to/replay.mkv");

        savedAware.Verify(x => x.OnReplayBufferSaved("/path/to/replay.mkv"), Times.Once);
    }

    // --- Multiple interface support ---

    [Fact]
    public void Notify_CommandImplementingMultipleInterfaces_ReceivesAllRelevantNotifications()
    {
        var multiAware = new Mock<IMultiAwareCommand>();
        this._registry.Register(multiAware.Object);

        this._registry.NotifyConnected();
        this._registry.NotifySceneChanged("Scene 1");
        this._registry.NotifyInputMuteChanged("Mic");

        multiAware.Verify(x => x.OnConnected(), Times.Once);
        multiAware.Verify(x => x.OnSceneChanged("Scene 1"), Times.Once);
        multiAware.Verify(x => x.OnInputMuteChanged("Mic"), Times.Once);
    }

    // Helper interface combining multiple for testing
    public interface IMultiAwareCommand : ISceneAwareCommand, IInputMuteAwareCommand
    {
    }
}
