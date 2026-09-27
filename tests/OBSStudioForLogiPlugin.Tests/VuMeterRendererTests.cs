namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

public class VuMeterRendererTests
{
    // --- LinearToDb ---

    [Fact]
    public void LinearToDb_FullScale_ReturnsZeroDb()
    {
        var result = VuMeterRenderer.LinearToDb(1.0f);

        Assert.Equal(0.0f, result, 3);
    }

    [Fact]
    public void LinearToDb_Zero_ReturnsNegativeInfinity()
    {
        var result = VuMeterRenderer.LinearToDb(0.0f);

        Assert.True(Single.IsNegativeInfinity(result));
    }

    [Fact]
    public void LinearToDb_OneTenth_ReturnsApproximatelyMinusTwentyDb()
    {
        var result = VuMeterRenderer.LinearToDb(0.1f);

        Assert.Equal(-20.0f, result, 1);
    }

    // --- GetColorZone ---

    [Fact]
    public void GetColorZone_WellBelowYellowThreshold_ReturnsGreen()
    {
        var result = VuMeterRenderer.GetColorZone(-40f);

        Assert.Equal(VuMeterRenderer.ColorZone.Green, result);
    }

    [Fact]
    public void GetColorZone_AtYellowThreshold_ReturnsYellow()
    {
        var result = VuMeterRenderer.GetColorZone(-20f);

        Assert.Equal(VuMeterRenderer.ColorZone.Yellow, result);
    }

    [Fact]
    public void GetColorZone_BetweenYellowAndRedThresholds_ReturnsYellow()
    {
        var result = VuMeterRenderer.GetColorZone(-15f);

        Assert.Equal(VuMeterRenderer.ColorZone.Yellow, result);
    }

    [Fact]
    public void GetColorZone_AtRedThreshold_ReturnsRed()
    {
        var result = VuMeterRenderer.GetColorZone(-10f);

        Assert.Equal(VuMeterRenderer.ColorZone.Red, result);
    }

    [Fact]
    public void GetColorZone_AboveRedThreshold_ReturnsRed()
    {
        var result = VuMeterRenderer.GetColorZone(0f);

        Assert.Equal(VuMeterRenderer.ColorZone.Red, result);
    }

    [Fact]
    public void GetColorZone_AtSilenceFloor_ReturnsGreen()
    {
        var result = VuMeterRenderer.GetColorZone(-60f);

        Assert.Equal(VuMeterRenderer.ColorZone.Green, result);
    }

    // --- CalculateMeterFraction ---

    [Fact]
    public void CalculateMeterFraction_ZeroDb_ReturnsOne()
    {
        var result = VuMeterRenderer.CalculateMeterFraction(0f);

        Assert.Equal(1.0f, result, 3);
    }

    [Fact]
    public void CalculateMeterFraction_SilenceFloor_ReturnsZero()
    {
        var result = VuMeterRenderer.CalculateMeterFraction(-60f);

        Assert.Equal(0.0f, result, 3);
    }

    [Fact]
    public void CalculateMeterFraction_Midpoint_ReturnsHalf()
    {
        var result = VuMeterRenderer.CalculateMeterFraction(-30f);

        Assert.Equal(0.5f, result, 3);
    }

    [Fact]
    public void CalculateMeterFraction_TypicalSpeechLevel_ReturnsSubstantialFraction()
    {
        // -20dB (~0.1 linear) is normal speech - should read as roughly two-thirds up the meter,
        // not the ~10% a raw-linear mapping would have given it (the bug this fixes).
        var result = VuMeterRenderer.CalculateMeterFraction(-20f);

        Assert.Equal(40f / 60f, result, 3);
    }

    [Fact]
    public void CalculateMeterFraction_BelowSilenceFloor_ClampsToZero()
    {
        var result = VuMeterRenderer.CalculateMeterFraction(-80f);

        Assert.Equal(0.0f, result, 3);
    }

    [Fact]
    public void CalculateMeterFraction_NegativeInfinity_ClampsToZero()
    {
        var result = VuMeterRenderer.CalculateMeterFraction(Single.NegativeInfinity);

        Assert.Equal(0.0f, result, 3);
    }

    [Fact]
    public void CalculateMeterFraction_AboveZeroDb_ClampsToOne()
    {
        var result = VuMeterRenderer.CalculateMeterFraction(6f);

        Assert.Equal(1.0f, result, 3);
    }

    // --- CalculateBarHeight ---

    [Fact]
    public void CalculateBarHeight_ZeroFraction_ReturnsZero()
    {
        var result = VuMeterRenderer.CalculateBarHeight(0.0f, 100);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateBarHeight_FullFraction_ReturnsMaxHeight()
    {
        var result = VuMeterRenderer.CalculateBarHeight(1.0f, 100);

        Assert.Equal(100, result);
    }

    [Fact]
    public void CalculateBarHeight_HalfFraction_ReturnsHalfMaxHeight()
    {
        var result = VuMeterRenderer.CalculateBarHeight(0.5f, 100);

        Assert.Equal(50, result);
    }

    [Fact]
    public void CalculateBarHeight_FractionAboveOne_ClampsToMaxHeight()
    {
        var result = VuMeterRenderer.CalculateBarHeight(1.5f, 100);

        Assert.Equal(100, result);
    }

    [Fact]
    public void CalculateBarHeight_NegativeFraction_ClampsToZero()
    {
        var result = VuMeterRenderer.CalculateBarHeight(-0.5f, 100);

        Assert.Equal(0, result);
    }

    // --- CalculateBarWidth ---

    [Fact]
    public void CalculateBarWidth_MonoChannel_ReturnsWidthMinusMargins()
    {
        var result = VuMeterRenderer.CalculateBarWidth(90, 1);

        Assert.Equal(82, result); // 90 - 4*(1+1) = 82
    }

    [Fact]
    public void CalculateBarWidth_StereoChannels_SplitsRemainingWidthEvenly()
    {
        var result = VuMeterRenderer.CalculateBarWidth(90, 2);

        Assert.Equal(39, result); // (90 - 4*3) / 2 = 39
    }

    [Fact]
    public void CalculateBarWidth_ZeroChannels_ReturnsZero()
    {
        var result = VuMeterRenderer.CalculateBarWidth(90, 0);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateBarWidth_NegativeChannelCount_ReturnsZero()
    {
        var result = VuMeterRenderer.CalculateBarWidth(90, -1);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateBarWidth_MarginsExceedTotalWidth_ReturnsZeroNotNegative()
    {
        var result = VuMeterRenderer.CalculateBarWidth(5, 2);

        Assert.Equal(0, result);
    }

    // --- GetDisplayPeaks ---

    [Fact]
    public void GetDisplayPeaks_LiveInputWithNoChannels_ReturnsOneSilentChannel()
    {
        var levels = new Models.AudioMeterLevels { IsLive = true };

        Assert.Equal(new[] { 0.0f }, VuMeterRenderer.GetDisplayPeaks(levels));
    }

    [Fact]
    public void GetDisplayPeaks_NotLiveWithNoChannels_ReturnsEmpty()
    {
        Assert.Empty(VuMeterRenderer.GetDisplayPeaks(Models.AudioMeterLevels.Empty));
    }

    [Fact]
    public void GetDisplayPeaks_WithChannels_ReturnsChannelsUnchanged()
    {
        var levels = new Models.AudioMeterLevels { ChannelPeaks = new[] { 0.2f, 0.4f }, IsLive = true };

        Assert.Equal(new[] { 0.2f, 0.4f }, VuMeterRenderer.GetDisplayPeaks(levels));
    }

    [Fact]
    public void GetDisplayPeaks_NullLevels_ReturnsEmpty()
    {
        Assert.Empty(VuMeterRenderer.GetDisplayPeaks(null));
    }

    // --- ResolveTileState ---

    [Fact]
    public void ResolveTileState_NullLevels_ReturnsInactive()
    {
        Assert.Equal(VuMeterRenderer.TileState.Inactive, VuMeterRenderer.ResolveTileState(null));
    }

    [Fact]
    public void ResolveTileState_NotLive_ReturnsInactive()
    {
        var levels = new Models.AudioMeterLevels { IsLive = false };

        Assert.Equal(VuMeterRenderer.TileState.Inactive, VuMeterRenderer.ResolveTileState(levels));
    }

    [Fact]
    public void ResolveTileState_NotLiveAndMuted_ReturnsInactive()
    {
        var levels = new Models.AudioMeterLevels { IsLive = false, IsMuted = true };

        Assert.Equal(VuMeterRenderer.TileState.Inactive, VuMeterRenderer.ResolveTileState(levels));
    }

    [Fact]
    public void ResolveTileState_LiveAndMuted_ReturnsMuted()
    {
        var levels = new Models.AudioMeterLevels { IsLive = true, IsMuted = true, ChannelPeaks = new[] { 0.5f } };

        Assert.Equal(VuMeterRenderer.TileState.Muted, VuMeterRenderer.ResolveTileState(levels));
    }

    [Fact]
    public void ResolveTileState_LiveAndUnmuted_ReturnsMeter()
    {
        var levels = new Models.AudioMeterLevels { IsLive = true, ChannelPeaks = new[] { 0.5f } };

        Assert.Equal(VuMeterRenderer.TileState.Meter, VuMeterRenderer.ResolveTileState(levels));
    }

    [Fact]
    public void ResolveTileState_LiveWithNoChannels_ReturnsMeter()
    {
        var levels = new Models.AudioMeterLevels { IsLive = true };

        Assert.Equal(VuMeterRenderer.TileState.Meter, VuMeterRenderer.ResolveTileState(levels));
    }
}
