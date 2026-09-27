namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public static class VuMeterRenderer
    {
        // Matches OBS's own VU meter: -60dB (silence floor) to 0dB (full scale), green below
        // -20dB, yellow from -20dB to -10dB, red at -10dB and above.
        private const Single MinDb = -60f;
        private const Single YellowThresholdDb = -20f;
        private const Single RedThresholdDb = -10f;
        private const Int32 BarMargin = 4;
        private const Int32 BaselineHeight = 2;
        private static readonly BitmapColor BaselineColor = new BitmapColor(80, 80, 80);

        public enum ColorZone
        {
            Green,
            Yellow,
            Red
        }

        // OBS reports peaks as a linear amplitude ratio (0.0-1.0, where 1.0 = 0dB) - the same
        // ratio VolumeConverter.MulToDb already converts for volume-fader displays.
        public static Single LinearToDb(Single linearPeak) => VolumeConverter.MulToDb(linearPeak);

        public static ColorZone GetColorZone(Single db)
        {
            if (db >= RedThresholdDb)
                return ColorZone.Red;

            if (db >= YellowThresholdDb)
                return ColorZone.Yellow;

            return ColorZone.Green;
        }

        // Maps a dB value onto the visible 0.0-1.0 meter range, using the full -60dB..0dB scale
        // rather than the raw linear amplitude - a linear mapping compresses normal speech
        // (typically around -20dB, i.e. ~0.1 linear) into a barely-visible sliver of the bar.
        public static Single CalculateMeterFraction(Single db)
        {
            Single clampedDb = Math.Clamp(db, MinDb, 0.0f);
            return (clampedDb - MinDb) / -MinDb;
        }

        public static Int32 CalculateBarHeight(Single fraction, Int32 maxHeight)
        {
            Single clamped = Math.Clamp(fraction, 0.0f, 1.0f);
            return (Int32)(clamped * maxHeight);
        }

        public static Int32 CalculateBarWidth(Int32 totalWidth, Int32 channelCount)
        {
            if (channelCount <= 0)
                return 0;

            Int32 available = totalWidth - BarMargin * (channelCount + 1);
            return Math.Max(available / channelCount, 0);
        }

        public enum TileState
        {
            Inactive,
            Muted,
            Meter
        }

        // Inactive (OBS isn't reporting the input) wins over muted: a muted input that isn't live
        // has nothing to meter either way, and "not live" is the more useful thing to show.
        public static TileState ResolveTileState(Models.AudioMeterLevels levels)
        {
            if (levels == null || !levels.IsLive)
            {
                return TileState.Inactive;
            }

            return levels.IsMuted ? TileState.Muted : TileState.Meter;
        }

        // No name label is drawn here - both meter UIs rely on the SDK's own button title for that.
        public static BitmapImage Render(Models.AudioMeterLevels levels, PluginImageSize imageSize)
        {
            switch (ResolveTileState(levels))
            {
                case TileState.Inactive:
                    return ButtonImageHelper.IconWithBackground("AudioMeterInactive.svg", imageSize, BitmapColor.Black);
                case TileState.Muted:
                    return ButtonImageHelper.IconWithBackground("AudioMeterMuted.svg", imageSize, BitmapColor.Black);
                default:
                    return RenderBars(GetDisplayPeaks(levels), imageSize);
            }
        }

        // Bar layout is verified against BitmapBuilder.FillRectangle's real signature, already used
        // by ButtonTextRenderer.RenderTextWithBorder in this codebase.
        private static BitmapImage RenderBars(Single[] channelPeaks, PluginImageSize imageSize)
        {
            using (var builder = new BitmapBuilder(imageSize))
            {
                builder.Clear(BitmapColor.Black);

                Int32 channelCount = channelPeaks.Length;
                Int32 barWidth = CalculateBarWidth(builder.Width, channelCount);

                for (Int32 i = 0; i < channelCount; i++)
                {
                    Single db = LinearToDb(channelPeaks[i]);
                    BitmapColor color = GetBitmapColor(GetColorZone(db));
                    Single fraction = CalculateMeterFraction(db);
                    Int32 barHeight = CalculateBarHeight(fraction, builder.Height);

                    Int32 x = BarMargin + i * (barWidth + BarMargin);
                    Int32 y = builder.Height - barHeight;

                    // A thin baseline under each channel shows the input is live even when silent -
                    // a 0-height bar is otherwise invisible on black. The bar, when present, draws over it.
                    builder.FillRectangle(x, builder.Height - BaselineHeight, barWidth, BaselineHeight, BaselineColor);
                    builder.FillRectangle(x, y, barWidth, barHeight, color);
                }

                return builder.ToImage();
            }
        }

        // A live input reported with no channels (e.g. a browser source with no audio flowing yet)
        // is drawn as a single silent channel, so it shows the same baseline as any other
        // live-but-silent input instead of an empty black tile.
        public static Single[] GetDisplayPeaks(Models.AudioMeterLevels levels)
        {
            Single[] channelPeaks = levels?.ChannelPeaks ?? new Single[0];

            if (channelPeaks.Length == 0 && (levels?.IsLive ?? false))
            {
                return new[] { 0.0f };
            }

            return channelPeaks;
        }

        private static BitmapColor GetBitmapColor(ColorZone zone)
        {
            switch (zone)
            {
                case ColorZone.Red:
                    return BitmapColor.Red;
                case ColorZone.Yellow:
                    return new BitmapColor(255, 200, 0);
                default:
                    return BitmapColor.Green;
            }
        }
    }
}
