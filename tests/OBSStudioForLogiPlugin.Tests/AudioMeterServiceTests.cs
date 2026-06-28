namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System;
using System.Collections.Generic;
using Loupedeck.OBSStudioForLogiPlugin.Services;

public class AudioMeterServiceTests
{
    private readonly AudioMeterService _service;

    public AudioMeterServiceTests()
    {
        this._service = new AudioMeterService();
    }

    [Fact]
    public void GetLevels_NoData_ReturnsZero()
    {
        var (peakL, peakR) = this._service.GetLevels("Microphone");
        Assert.Equal(0f, peakL);
        Assert.Equal(0f, peakR);
    }

    [Fact]
    public void UpdateLevels_StoresData()
    {
        var levels = new Dictionary<String, (Single peakL, Single peakR)>
        {
            { "Microphone", (0.5f, 0.6f) }
        };

        this._service.UpdateLevels(levels);

        var (peakL, peakR) = this._service.GetLevels("Microphone");
        Assert.Equal(0.5f, peakL);
        Assert.Equal(0.6f, peakR);
    }

    [Fact]
    public void UpdateLevels_MultipleInputs_StoresAll()
    {
        var levels = new Dictionary<String, (Single peakL, Single peakR)>
        {
            { "Microphone", (0.3f, 0.4f) },
            { "Desktop Audio", (0.7f, 0.8f) }
        };

        this._service.UpdateLevels(levels);

        var (micL, micR) = this._service.GetLevels("Microphone");
        Assert.Equal(0.3f, micL);
        Assert.Equal(0.4f, micR);

        var (deskL, deskR) = this._service.GetLevels("Desktop Audio");
        Assert.Equal(0.7f, deskL);
        Assert.Equal(0.8f, deskR);
    }

    [Fact]
    public void UpdateLevels_OverwritesPreviousData()
    {
        var levels1 = new Dictionary<String, (Single peakL, Single peakR)>
        {
            { "Microphone", (0.5f, 0.5f) }
        };
        this._service.UpdateLevels(levels1);

        var levels2 = new Dictionary<String, (Single peakL, Single peakR)>
        {
            { "Microphone", (0.9f, 0.8f) }
        };
        this._service.UpdateLevels(levels2);

        var (peakL, peakR) = this._service.GetLevels("Microphone");
        Assert.Equal(0.9f, peakL);
        Assert.Equal(0.8f, peakR);
    }

    [Fact]
    public void GetLevels_UnknownInput_ReturnsZero()
    {
        var levels = new Dictionary<String, (Single peakL, Single peakR)>
        {
            { "Microphone", (0.5f, 0.6f) }
        };
        this._service.UpdateLevels(levels);

        var (peakL, peakR) = this._service.GetLevels("Unknown");
        Assert.Equal(0f, peakL);
        Assert.Equal(0f, peakR);
    }

    [Fact]
    public void GetInputNames_ReturnsStoredNames()
    {
        var levels = new Dictionary<String, (Single peakL, Single peakR)>
        {
            { "Microphone", (0.5f, 0.5f) },
            { "Desktop Audio", (0.3f, 0.3f) }
        };
        this._service.UpdateLevels(levels);

        var names = this._service.GetInputNames();
        Assert.Contains("Microphone", names);
        Assert.Contains("Desktop Audio", names);
        Assert.Equal(2, names.Length);
    }

    [Fact]
    public void Clear_RemovesAllData()
    {
        var levels = new Dictionary<String, (Single peakL, Single peakR)>
        {
            { "Microphone", (0.5f, 0.5f) }
        };
        this._service.UpdateLevels(levels);

        this._service.Clear();

        var (peakL, peakR) = this._service.GetLevels("Microphone");
        Assert.Equal(0f, peakL);
        Assert.Equal(0f, peakR);
        Assert.Empty(this._service.GetInputNames());
    }

    [Fact]
    public void IsActive_DefaultFalse()
    {
        Assert.False(this._service.IsActive);
    }

    [Fact]
    public void Start_SetsActive()
    {
        this._service.Start();
        Assert.True(this._service.IsActive);
        this._service.Stop();
    }

    [Fact]
    public void Stop_ClearsActive()
    {
        this._service.Start();
        this._service.Stop();
        Assert.False(this._service.IsActive);
    }
}
