namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Loupedeck.OBSStudioForLogiPlugin.Models;

public class AudioMeterServiceTests
{
    private readonly AudioMeterService _service;
    private DateTime _now = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    public AudioMeterServiceTests()
    {
        this._service = new AudioMeterService(() => this._now);
    }

    private void Advance(Int32 milliseconds) => this._now = this._now.AddMilliseconds(milliseconds);

    private static AudioMeterLevels Levels(params Single[] peaks) => new AudioMeterLevels { ChannelPeaks = peaks };

    // --- Staleness ---

    [Fact]
    public void GetLevels_WithinStaleThreshold_ReturnsStoredLevels()
    {
        this._service.UpdateLevels("Microphone", Levels(0.5f));

        this.Advance(Helpers.OBSTimings.AudioMeterStaleThreshold);

        Assert.True(this._service.GetLevels("Microphone").HasData);
    }

    [Fact]
    public void GetLevels_AfterStaleThreshold_ReturnsNoData()
    {
        this._service.UpdateLevels("Microphone", Levels(0.5f));

        this.Advance(Helpers.OBSTimings.AudioMeterStaleThreshold + 1);

        Assert.False(this._service.GetLevels("Microphone").HasData);
    }

    [Fact]
    public void GetLevels_StaleInputUpdatedAgain_ReturnsNewLevels()
    {
        this._service.UpdateLevels("Microphone", Levels(0.5f));
        this.Advance(Helpers.OBSTimings.AudioMeterStaleThreshold + 1);

        this._service.UpdateLevels("Microphone", Levels(0.25f));

        Assert.Equal(new[] { 0.25f }, this._service.GetLevels("Microphone").ChannelPeaks);
    }

    [Fact]
    public void GetLevels_ActiveButSilentInput_HasDataWithZeroPeaks()
    {
        this._service.UpdateLevels("Microphone", Levels(0.0f, 0.0f));

        var result = this._service.GetLevels("Microphone");

        Assert.True(result.HasData);
        Assert.Equal(new[] { 0.0f, 0.0f }, result.ChannelPeaks);
    }

    [Fact]
    public void UpdateLevels_WithNullChannelPeaks_StoresNoChannels()
    {
        this._service.UpdateLevels("Microphone", new AudioMeterLevels { ChannelPeaks = null });

        Assert.False(this._service.GetLevels("Microphone").HasData);
    }

    // --- GetLiveInputs ---

    [Fact]
    public void GetLiveInputs_WhenNothingReported_ReturnsEmpty()
    {
        Assert.Empty(this._service.GetLiveInputs());
    }

    [Fact]
    public void GetLiveInputs_ReturnsInputsInFirstSeenOrder()
    {
        this._service.UpdateLevels("Mic", Levels(0.1f));
        this._service.UpdateLevels("Desktop", Levels(0.1f));
        this._service.UpdateLevels("Music", Levels(0.1f));
        this._service.UpdateLevels("Mic", Levels(0.2f));

        Assert.Equal(new[] { "Mic", "Desktop", "Music" }, this._service.GetLiveInputs());
    }

    [Fact]
    public void GetLiveInputs_IncludesInputsReportedWithNoChannels()
    {
        this._service.UpdateLevels("Mic", Levels(0.1f));
        this._service.UpdateLevels("Browser", Levels());

        Assert.Equal(new[] { "Mic", "Browser" }, this._service.GetLiveInputs());
    }

    [Fact]
    public void GetLevels_InputReportedWithNoChannels_IsLiveWithoutData()
    {
        this._service.UpdateLevels("Browser", Levels());

        var result = this._service.GetLevels("Browser");

        Assert.True(result.IsLive);
        Assert.False(result.HasData);
    }

    [Fact]
    public void GetLevels_FreshInput_IsLive()
    {
        this._service.UpdateLevels("Mic", Levels(0.1f));

        Assert.True(this._service.GetLevels("Mic").IsLive);
    }

    [Fact]
    public void GetLevels_StaleInput_IsNotLive()
    {
        this._service.UpdateLevels("Mic", Levels(0.1f));

        this.Advance(Helpers.OBSTimings.AudioMeterStaleThreshold + 1);

        Assert.False(this._service.GetLevels("Mic").IsLive);
    }

    [Fact]
    public void GetLevels_NeverReported_IsNotLive()
    {
        Assert.False(this._service.GetLevels("Mic").IsLive);
    }

    [Fact]
    public void GetLiveInputs_ExcludesInputsThatStoppedReporting()
    {
        this._service.UpdateLevels("Mic", Levels(0.1f));
        this._service.UpdateLevels("Game", Levels(0.1f));

        this.Advance(Helpers.OBSTimings.AudioMeterStaleThreshold + 1);
        this._service.UpdateLevels("Mic", Levels(0.1f));

        Assert.Equal(new[] { "Mic" }, this._service.GetLiveInputs());
    }

    [Fact]
    public void GetLiveInputs_InputReturningAfterGoingStale_KeepsOriginalPosition()
    {
        this._service.UpdateLevels("Mic", Levels(0.1f));
        this._service.UpdateLevels("Game", Levels(0.1f));
        this.Advance(Helpers.OBSTimings.AudioMeterStaleThreshold + 1);
        this._service.UpdateLevels("Game", Levels(0.1f));

        this._service.UpdateLevels("Mic", Levels(0.1f));

        Assert.Equal(new[] { "Mic", "Game" }, this._service.GetLiveInputs());
    }

    [Fact]
    public void GetLiveInputs_AfterClear_ReturnsEmpty()
    {
        this._service.UpdateLevels("Mic", Levels(0.1f));

        this._service.Clear();

        Assert.Empty(this._service.GetLiveInputs());
    }

    // --- Mute state ---

    [Fact]
    public void GetLevels_WhenMuteStateUnknown_IsNotMuted()
    {
        this._service.UpdateLevels("Mic", Levels(0.1f));

        Assert.False(this._service.GetLevels("Mic").IsMuted);
    }

    [Fact]
    public void SetMuted_True_ReflectedInLevels()
    {
        this._service.UpdateLevels("Mic", Levels(0.1f));

        this._service.SetMuted("Mic", true);

        var result = this._service.GetLevels("Mic");
        Assert.True(result.IsMuted);
        Assert.True(result.HasData);
    }

    [Fact]
    public void SetMuted_ThenUnmuted_ReflectedInLevels()
    {
        this._service.SetMuted("Mic", true);

        this._service.SetMuted("Mic", false);

        Assert.False(this._service.GetLevels("Mic").IsMuted);
    }

    [Fact]
    public void TryBeginMuteLookup_UnknownInput_ReturnsTrueOnlyOnce()
    {
        Assert.True(this._service.TryBeginMuteLookup("Mic"));
        Assert.False(this._service.TryBeginMuteLookup("Mic"));
    }

    [Fact]
    public void TryBeginMuteLookup_WhenMuteStateAlreadyKnown_ReturnsFalse()
    {
        this._service.SetMuted("Mic", false);

        Assert.False(this._service.TryBeginMuteLookup("Mic"));
    }

    [Fact]
    public void TryBeginMuteLookup_EmptyName_ReturnsFalse()
    {
        Assert.False(this._service.TryBeginMuteLookup(String.Empty));
    }

    [Fact]
    public void CompleteMuteLookup_WhenPending_StoresResult()
    {
        this._service.TryBeginMuteLookup("Mic");

        this._service.CompleteMuteLookup("Mic", true);

        Assert.True(this._service.GetLevels("Mic").IsMuted);
    }

    [Fact]
    public void CompleteMuteLookup_WhenEventArrivedFirst_EventWins()
    {
        this._service.TryBeginMuteLookup("Mic");
        this._service.SetMuted("Mic", false);

        this._service.CompleteMuteLookup("Mic", true);

        Assert.False(this._service.GetLevels("Mic").IsMuted);
    }

    [Fact]
    public void CompleteMuteLookup_AfterClear_IsIgnored()
    {
        this._service.TryBeginMuteLookup("Mic");
        this._service.Clear();

        this._service.CompleteMuteLookup("Mic", true);

        Assert.False(this._service.GetLevels("Mic").IsMuted);
        Assert.True(this._service.TryBeginMuteLookup("Mic"));
    }

    [Fact]
    public void Clear_ForgetsMuteStates()
    {
        this._service.SetMuted("Mic", true);

        this._service.Clear();

        Assert.False(this._service.GetLevels("Mic").IsMuted);
    }

    [Fact]
    public void GetLevels_WhenNeverUpdated_ReturnsEmpty()
    {
        var result = this._service.GetLevels("Microphone");

        Assert.False(result.HasData);
        Assert.Empty(result.ChannelPeaks);
    }

    [Fact]
    public void GetLevels_WhenInputNameIsEmpty_ReturnsEmpty()
    {
        var result = this._service.GetLevels(String.Empty);

        Assert.False(result.HasData);
    }

    [Fact]
    public void GetLevels_WhenInputNameIsNull_ReturnsEmpty()
    {
        var result = this._service.GetLevels(null);

        Assert.False(result.HasData);
    }

    [Fact]
    public void UpdateLevels_ThenGetLevels_ReturnsStoredLevels()
    {
        var levels = new AudioMeterLevels { ChannelPeaks = new[] { 0.5f, 0.75f } };

        this._service.UpdateLevels("Microphone", levels);
        var result = this._service.GetLevels("Microphone");

        Assert.True(result.HasData);
        Assert.Equal(new[] { 0.5f, 0.75f }, result.ChannelPeaks);
    }

    [Fact]
    public void UpdateLevels_CalledAgain_OverwritesPreviousLevels()
    {
        this._service.UpdateLevels("Microphone", new AudioMeterLevels { ChannelPeaks = new[] { 0.1f } });
        this._service.UpdateLevels("Microphone", new AudioMeterLevels { ChannelPeaks = new[] { 0.9f } });

        var result = this._service.GetLevels("Microphone");

        Assert.Equal(new[] { 0.9f }, result.ChannelPeaks);
    }

    [Fact]
    public void UpdateLevels_WithNullLevels_DoesNotThrowAndDoesNotStore()
    {
        var exception = Record.Exception(() => this._service.UpdateLevels("Microphone", null));

        Assert.Null(exception);
        Assert.False(this._service.GetLevels("Microphone").HasData);
    }

    [Fact]
    public void UpdateLevels_WithEmptyInputName_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._service.UpdateLevels(String.Empty, new AudioMeterLevels()));

        Assert.Null(exception);
    }

    [Fact]
    public void UpdateLevels_TwoDistinctInputs_TrackedIndependently()
    {
        this._service.UpdateLevels("Microphone", new AudioMeterLevels { ChannelPeaks = new[] { 0.2f } });
        this._service.UpdateLevels("Desktop Audio", new AudioMeterLevels { ChannelPeaks = new[] { 0.8f } });

        Assert.Equal(new[] { 0.2f }, this._service.GetLevels("Microphone").ChannelPeaks);
        Assert.Equal(new[] { 0.8f }, this._service.GetLevels("Desktop Audio").ChannelPeaks);
    }

    [Fact]
    public void Clear_RemovesAllStoredLevels()
    {
        this._service.UpdateLevels("Microphone", new AudioMeterLevels { ChannelPeaks = new[] { 0.5f } });

        this._service.Clear();

        Assert.False(this._service.GetLevels("Microphone").HasData);
    }

    [Fact]
    public void Clear_WhenAlreadyEmpty_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._service.Clear());

        Assert.Null(exception);
    }
}
