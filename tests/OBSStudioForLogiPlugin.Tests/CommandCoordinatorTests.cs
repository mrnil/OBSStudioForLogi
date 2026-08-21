namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System;
using Moq;

public class CommandCoordinatorTests
{
    private readonly CommandCoordinator _coordinator;

    public CommandCoordinatorTests()
    {
        this._coordinator = new CommandCoordinator();
    }

    // --- RegisterCommand ---

    [Fact]
    public void RegisterCommand_NullCommand_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._coordinator.RegisterCommand(null));

        Assert.Null(exception);
    }

    // --- NotifyConnected / NotifyDisconnected ---

    [Fact]
    public void NotifyConnected_CallsOnConnectedOnAllCommands()
    {
        var mock1 = new Mock<IObsCommand>();
        var mock2 = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(mock1.Object);
        this._coordinator.RegisterCommand(mock2.Object);

        this._coordinator.NotifyConnected();

        mock1.Verify(x => x.OnConnected(), Times.Once);
        mock2.Verify(x => x.OnConnected(), Times.Once);
    }

    [Fact]
    public void NotifyDisconnected_CallsOnDisconnectedOnAllCommands()
    {
        var mock1 = new Mock<IObsCommand>();
        var mock2 = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(mock1.Object);
        this._coordinator.RegisterCommand(mock2.Object);

        this._coordinator.NotifyDisconnected();

        mock1.Verify(x => x.OnDisconnected(), Times.Once);
        mock2.Verify(x => x.OnDisconnected(), Times.Once);
    }

    [Fact]
    public void NotifyConnected_WithNoCommands_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._coordinator.NotifyConnected());

        Assert.Null(exception);
    }

    // --- Per-command exception isolation (Assessment #6) ---

    [Fact]
    public void NotifyConnected_OneCommandThrows_StillNotifiesRemainingCommands()
    {
        var throwing = new Mock<IObsCommand>();
        throwing.Setup(x => x.OnConnected()).Throws(new InvalidOperationException("boom"));
        var healthy = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(throwing.Object);
        this._coordinator.RegisterCommand(healthy.Object);

        this._coordinator.NotifyConnected();

        healthy.Verify(x => x.OnConnected(), Times.Once);
    }

    [Fact]
    public void NotifyConnected_CommandThrows_DoesNotPropagateException()
    {
        var throwing = new Mock<IObsCommand>();
        throwing.Setup(x => x.OnConnected()).Throws(new InvalidOperationException("boom"));
        this._coordinator.RegisterCommand(throwing.Object);

        var exception = Record.Exception(() => this._coordinator.NotifyConnected());

        Assert.Null(exception);
    }

    [Fact]
    public void NotifySceneChanged_OneCommandThrows_StillNotifiesRemainingCommands()
    {
        var throwing = new Mock<ISceneAwareCommand>();
        throwing.Setup(x => x.OnSceneChanged(It.IsAny<String>())).Throws(new InvalidOperationException("boom"));
        var healthy = new Mock<ISceneAwareCommand>();
        this._coordinator.RegisterCommand(throwing.Object);
        this._coordinator.RegisterCommand(healthy.Object);

        this._coordinator.NotifySceneChanged("Scene 1");

        healthy.Verify(x => x.OnSceneChanged("Scene 1"), Times.Once);
    }

    // --- NotifyProfileChanged ---

    [Fact]
    public void NotifyProfileChanged_CallsOnlyIProfileAwareCommands()
    {
        var profileAware = new Mock<IProfileAwareCommand>();
        var nonProfileAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(profileAware.Object);
        this._coordinator.RegisterCommand(nonProfileAware.Object);

        this._coordinator.NotifyProfileChanged("old", "new");

        profileAware.Verify(x => x.OnProfileChanged("old", "new"), Times.Once);
    }

    // --- NotifySceneCollectionChanged ---

    [Fact]
    public void NotifySceneCollectionChanged_CallsOnlyISceneCollectionAwareCommands()
    {
        var sceneCollectionAware = new Mock<ISceneCollectionAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(sceneCollectionAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        this._coordinator.NotifySceneCollectionChanged("old", "new");

        sceneCollectionAware.Verify(x => x.OnSceneCollectionChanged("old", "new"), Times.Once);
    }

    // --- NotifySceneChanged ---

    [Fact]
    public void NotifySceneChanged_CallsOnlyISceneAwareCommands()
    {
        var sceneAware = new Mock<ISceneAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(sceneAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        this._coordinator.NotifySceneChanged("Scene 1");

        sceneAware.Verify(x => x.OnSceneChanged("Scene 1"), Times.Once);
    }

    // --- NotifyScenesChanged ---

    [Fact]
    public void NotifyScenesChanged_CallsOnlyIScenesListAwareCommands()
    {
        var scenesListAware = new Mock<IScenesListAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(scenesListAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        var scenes = new[] { "Scene 1", "Scene 2" };
        this._coordinator.NotifyScenesChanged(scenes);

        scenesListAware.Verify(x => x.OnScenesChanged(scenes), Times.Once);
    }

    // --- NotifyProfilesChanged ---

    [Fact]
    public void NotifyProfilesChanged_CallsOnlyIProfilesListAwareCommands()
    {
        var profilesListAware = new Mock<IProfilesListAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(profilesListAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        var profiles = new[] { "Profile 1", "Profile 2" };
        this._coordinator.NotifyProfilesChanged(profiles, "Profile 1");

        profilesListAware.Verify(x => x.OnProfilesChanged(profiles, "Profile 1"), Times.Once);
    }

    // --- NotifySourceVisibilityChanged ---

    [Fact]
    public void NotifySourceVisibilityChanged_CallsOnlyISourceVisibilityAwareCommands()
    {
        var visibilityAware = new Mock<ISourceVisibilityAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(visibilityAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        this._coordinator.NotifySourceVisibilityChanged("Scene 1", "Source 1");

        visibilityAware.Verify(x => x.OnSourceVisibilityChanged("Scene 1", "Source 1"), Times.Once);
    }

    // --- NotifyInputMuteChanged ---

    [Fact]
    public void NotifyInputMuteChanged_CallsOnlyIInputMuteAwareCommands()
    {
        var muteAware = new Mock<IInputMuteAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(muteAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        this._coordinator.NotifyInputMuteChanged("Microphone");

        muteAware.Verify(x => x.OnInputMuteChanged("Microphone"), Times.Once);
    }

    // --- NotifyInputVolumeChanged ---

    [Fact]
    public void NotifyInputVolumeChanged_CallsOnlyIInputVolumeAwareCommands()
    {
        var volumeAware = new Mock<IInputVolumeAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(volumeAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        this._coordinator.NotifyInputVolumeChanged("Desktop Audio");

        volumeAware.Verify(x => x.OnInputVolumeChanged("Desktop Audio"), Times.Once);
    }

    // --- NotifyInputsChanged ---

    [Fact]
    public void NotifyInputsChanged_CallsOnlyIInputsListAwareCommands()
    {
        var inputsListAware = new Mock<IInputsListAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(inputsListAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        var inputs = new[] { "Mic", "Desktop" };
        this._coordinator.NotifyInputsChanged(inputs);

        inputsListAware.Verify(x => x.OnInputsChanged(inputs), Times.Once);
    }

    // --- NotifyVirtualCameraStateChanged ---

    [Fact]
    public void NotifyVirtualCameraStateChanged_CallsOnlyIVirtualCameraAwareCommands()
    {
        var vcamAware = new Mock<IVirtualCameraAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(vcamAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        this._coordinator.NotifyVirtualCameraStateChanged();

        vcamAware.Verify(x => x.OnVirtualCameraStateChanged(), Times.Once);
    }

    // --- NotifyReplayBufferStateChanged ---

    [Fact]
    public void NotifyReplayBufferStateChanged_CallsOnlyIReplayBufferAwareCommands()
    {
        var replayAware = new Mock<IReplayBufferAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(replayAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        this._coordinator.NotifyReplayBufferStateChanged();

        replayAware.Verify(x => x.OnReplayBufferStateChanged(), Times.Once);
    }

    // --- NotifyReplayBufferSaved ---

    [Fact]
    public void NotifyReplayBufferSaved_CallsOnlyIReplayBufferSavedAwareCommands()
    {
        var savedAware = new Mock<IReplayBufferSavedAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(savedAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        this._coordinator.NotifyReplayBufferSaved("/path/to/replay.mkv");

        savedAware.Verify(x => x.OnReplayBufferSaved("/path/to/replay.mkv"), Times.Once);
    }

    // --- NotifyStudioModeStateChanged ---

    [Fact]
    public void NotifyStudioModeStateChanged_CallsOnlyIStudioModeAwareCommands()
    {
        var studioModeAware = new Mock<IStudioModeAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(studioModeAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        this._coordinator.NotifyStudioModeStateChanged();

        studioModeAware.Verify(x => x.OnStudioModeStateChanged(), Times.Once);
    }

    // --- NotifyInputMonitorTypeChanged ---

    [Fact]
    public void NotifyInputMonitorTypeChanged_CallsOnlyIInputMonitorAwareCommands()
    {
        var monitorAware = new Mock<IInputMonitorAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(monitorAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        this._coordinator.NotifyInputMonitorTypeChanged("Microphone");

        monitorAware.Verify(x => x.OnInputMonitorTypeChanged("Microphone"), Times.Once);
    }

    // --- NotifySceneSourcesChanged ---

    [Fact]
    public void NotifySceneSourcesChanged_CallsOnlyISceneSourcesAwareCommands()
    {
        var sceneSourcesAware = new Mock<ISceneSourcesAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._coordinator.RegisterCommand(sceneSourcesAware.Object);
        this._coordinator.RegisterCommand(nonAware.Object);

        var sources = new[] { "Camera", "Screen" };
        var audioSources = new[] { "Mic", "Desktop" };
        this._coordinator.NotifySceneSourcesChanged("Scene 1", sources, audioSources);

        sceneSourcesAware.Verify(x => x.OnSceneSourcesChanged("Scene 1", sources, audioSources), Times.Once);
    }

    // --- Multiple interface support ---

    [Fact]
    public void Notify_CommandImplementingMultipleInterfaces_ReceivesAllRelevantNotifications()
    {
        var multiAware = new Mock<IMultiAwareCommand>();
        this._coordinator.RegisterCommand(multiAware.Object);

        this._coordinator.NotifyConnected();
        this._coordinator.NotifySceneChanged("Scene 1");
        this._coordinator.NotifyInputMuteChanged("Mic");

        multiAware.Verify(x => x.OnConnected(), Times.Once);
        multiAware.Verify(x => x.OnSceneChanged("Scene 1"), Times.Once);
        multiAware.Verify(x => x.OnInputMuteChanged("Mic"), Times.Once);
    }

    // Helper interface combining multiple for testing
    public interface IMultiAwareCommand : ISceneAwareCommand, IInputMuteAwareCommand
    {
    }
}
