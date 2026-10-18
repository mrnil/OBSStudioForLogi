namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System;
using Loupedeck.OBSStudioForLogiPlugin.Helpers;
using Loupedeck.OBSStudioForLogiPlugin.Models;
using Xunit;

public class PluginConfigTests
{
    [Fact]
    public void DoubleTapWindow_DefaultsToTimingsDefault()
    {
        PluginConfig config = new PluginConfig();

        Assert.Equal(OBSTimings.DoubleTapWindow, config.DoubleTapWindow);
        Assert.Equal(300, config.DoubleTapWindow);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(200)]
    [InlineData(300)]
    [InlineData(500)]
    public void NormalizeDoubleTapWindow_OfferedValue_ReturnsIt(Int32 windowMs)
    {
        Assert.Equal(windowMs, PluginConfig.NormalizeDoubleTapWindow(windowMs));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(150)]
    [InlineData(10000)]
    public void NormalizeDoubleTapWindow_OtherValue_ReturnsDefault(Int32 windowMs)
    {
        Assert.Equal(OBSTimings.DoubleTapWindow, PluginConfig.NormalizeDoubleTapWindow(windowMs));
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("200", 200)]
    [InlineData("500", 500)]
    [InlineData(" 500 ", 500)]
    public void ParseDoubleTapWindow_OfferedValue_ReturnsIt(String value, Int32 expected)
    {
        Assert.Equal(expected, PluginConfig.ParseDoubleTapWindow(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("fast")]
    [InlineData("250")]
    public void ParseDoubleTapWindow_InvalidValue_ReturnsDefault(String? value)
    {
        Assert.Equal(OBSTimings.DoubleTapWindow, PluginConfig.ParseDoubleTapWindow(value));
    }
}
