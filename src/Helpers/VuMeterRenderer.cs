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

        // Bar layout is verified against BitmapBuilder.FillRectangle's real signature, already used
        // by ButtonTextRenderer.RenderTextWithBorder in this codebase. No name label is drawn here -
        // the folder relies on the SDK's own button title (GetCommandDisplayName not overridden) for that.
        public static BitmapImage Render(Single[] channelPeaks, PluginImageSize imageSize)
        {
            using (var builder = new BitmapBuilder(imageSize))
            {
                builder.Clear(BitmapColor.Black);

                Int32 channelCount = channelPeaks?.Length ?? 0;
                Int32 barWidth = CalculateBarWidth(builder.Width, channelCount);

                for (Int32 i = 0; i < channelCount; i++)
                {
                    Single db = LinearToDb(channelPeaks[i]);
                    ColorZone zone = GetColorZone(db);
                    BitmapColor color = GetBitmapColor(zone);
                    Single fraction = CalculateMeterFraction(db);
                    Int32 barHeight = CalculateBarHeight(fraction, builder.Height);

                    Int32 x = BarMargin + i * (barWidth + BarMargin);
                    Int32 y = builder.Height - barHeight;

                    builder.FillRectangle(x, y, barWidth, barHeight, color);
                }

                return builder.ToImage();
            }
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
