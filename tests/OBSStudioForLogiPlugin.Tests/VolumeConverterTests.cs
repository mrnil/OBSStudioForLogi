namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System;

public class VolumeConverterTests
{
    // --- MulToDb ---

    [Fact]
    public void MulToDb_ZeroVolume_ReturnsNegativeInfinity()
    {
        var result = VolumeConverter.MulToDb(0f);
        Assert.Equal(Single.NegativeInfinity, result);
    }

    [Fact]
    public void MulToDb_Unity_ReturnsZeroDb()
    {
        var result = VolumeConverter.MulToDb(1.0f);
        Assert.Equal(0f, result, 1);
    }

    [Fact]
    public void MulToDb_Half_ReturnsApproxNegative6Db()
    {
        var result = VolumeConverter.MulToDb(0.5f);
        Assert.InRange(result, -6.1f, -6.0f);
    }

    [Fact]
    public void MulToDb_Double_ReturnsApproxPositive6Db()
    {
        var result = VolumeConverter.MulToDb(2.0f);
        Assert.InRange(result, 6.0f, 6.1f);
    }

    [Fact]
    public void MulToDb_Max20_ReturnsApproxPositive26Db()
    {
        var result = VolumeConverter.MulToDb(20.0f);
        Assert.InRange(result, 26.0f, 26.1f);
    }

    // --- FormatDb ---

    [Fact]
    public void FormatDb_ZeroVolume_ReturnsNegativeInfinitySymbol()
    {
        var result = VolumeConverter.FormatDb(0f);
        Assert.Equal("-∞ dB", result);
    }

    [Fact]
    public void FormatDb_Unity_ReturnsZeroDb()
    {
        var result = VolumeConverter.FormatDb(1.0f);
        Assert.Equal("0.0 dB", result);
    }

    [Fact]
    public void FormatDb_AboveUnity_ShowsPlusSign()
    {
        var result = VolumeConverter.FormatDb(2.0f);
        Assert.StartsWith("+", result);
        Assert.EndsWith("dB", result);
    }

    [Fact]
    public void FormatDb_BelowUnity_ShowsMinusSign()
    {
        var result = VolumeConverter.FormatDb(0.5f);
        Assert.StartsWith("-", result);
        Assert.EndsWith("dB", result);
    }

    [Fact]
    public void FormatDb_OneDecimalPlace()
    {
        var result = VolumeConverter.FormatDb(1.0f);
        Assert.Equal("0.0 dB", result);
    }
}
