namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System;

public class VuMeterRendererTests
{
    // --- Colour zone thresholds ---

    [Fact]
    public void GetBarColor_BelowYellowThreshold_ReturnsGreen()
    {
        var color = VuMeterRenderer.GetBarColor(0.20f);
        Assert.Equal(VuMeterRenderer.GreenZone, color);
    }

    [Fact]
    public void GetBarColor_AtYellowThreshold_ReturnsYellow()
    {
        var color = VuMeterRenderer.GetBarColor(0.25f);
        Assert.Equal(VuMeterRenderer.YellowZone, color);
    }

    [Fact]
    public void GetBarColor_BetweenYellowAndRed_ReturnsYellow()
    {
        var color = VuMeterRenderer.GetBarColor(0.50f);
        Assert.Equal(VuMeterRenderer.YellowZone, color);
    }

    [Fact]
    public void GetBarColor_AtRedThreshold_ReturnsRed()
    {
        var color = VuMeterRenderer.GetBarColor(0.71f);
        Assert.Equal(VuMeterRenderer.RedZone, color);
    }

    [Fact]
    public void GetBarColor_AboveRedThreshold_ReturnsRed()
    {
        var color = VuMeterRenderer.GetBarColor(0.95f);
        Assert.Equal(VuMeterRenderer.RedZone, color);
    }

    [Fact]
    public void GetBarColor_Zero_ReturnsGreen()
    {
        var color = VuMeterRenderer.GetBarColor(0f);
        Assert.Equal(VuMeterRenderer.GreenZone, color);
    }

    [Fact]
    public void GetBarColor_Max_ReturnsRed()
    {
        var color = VuMeterRenderer.GetBarColor(1.0f);
        Assert.Equal(VuMeterRenderer.RedZone, color);
    }

    // --- Bar height calculation ---

    [Fact]
    public void GetBarHeight_Zero_ReturnsZero()
    {
        var height = VuMeterRenderer.GetBarHeight(0f, 80);
        Assert.Equal(0, height);
    }

    [Fact]
    public void GetBarHeight_Max_ReturnsFullHeight()
    {
        var height = VuMeterRenderer.GetBarHeight(1.0f, 80);
        Assert.Equal(80, height);
    }

    [Fact]
    public void GetBarHeight_Half_ReturnsHalfHeight()
    {
        var height = VuMeterRenderer.GetBarHeight(0.5f, 80);
        Assert.Equal(40, height);
    }

    [Fact]
    public void GetBarHeight_NegativeValue_ClampsToZero()
    {
        var height = VuMeterRenderer.GetBarHeight(-0.5f, 80);
        Assert.Equal(0, height);
    }

    [Fact]
    public void GetBarHeight_OverMax_ClampsToFull()
    {
        var height = VuMeterRenderer.GetBarHeight(1.5f, 80);
        Assert.Equal(80, height);
    }
}
