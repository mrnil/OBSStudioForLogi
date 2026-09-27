namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Loupedeck.OBSStudioForLogiPlugin.Models;

public class AudioMeterServiceTests
{
    private readonly AudioMeterService _service;

    public AudioMeterServiceTests()
    {
        this._service = new AudioMeterService();
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
