namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public static class VuMeterRenderer
    {
        // Linear thresholds: -12dB ≈ 0.25, -3dB ≈ 0.71
        private const Single YellowThreshold = 0.25f;
        private const Single RedThreshold = 0.71f;

        public static readonly BitmapColor GreenZone = new BitmapColor(80, 255, 80);
        public static readonly BitmapColor YellowZone = new BitmapColor(255, 200, 0);
        public static readonly BitmapColor RedZone = new BitmapColor(255, 60, 60);
        private static readonly BitmapColor BarBackground = new BitmapColor(40, 40, 40);

        public static BitmapColor GetBarColor(Single peak)
        {
            if (peak >= RedThreshold)
                return RedZone;
            if (peak >= YellowThreshold)
                return YellowZone;
            return GreenZone;
        }

        public static Int32 GetBarHeight(Single peak, Int32 maxHeight)
        {
            Single clamped = Math.Clamp(peak, 0f, 1f);
            return (Int32)(clamped * maxHeight);
        }

        public static BitmapImage Render(Single peakL, Single peakR, String inputName, PluginImageSize imageSize)
        {
            using (var builder = new BitmapBuilder(imageSize))
            {
                builder.Clear(BitmapColor.Black);

                Int32 width = builder.Width;
                Int32 height = builder.Height;

                // Reserve bottom area for text
                Int32 textHeight = 16;
                Int32 meterHeight = height - textHeight - 4; // 4px padding
                Int32 meterTop = 2;

                // Bar layout: two bars centred with gap
                Int32 barWidth = (width - 12) / 2; // 4px margin each side, 4px gap
                Int32 barLeftX = 4;
                Int32 barRightX = barLeftX + barWidth + 4;

                // Draw bar backgrounds
                builder.FillRectangle(barLeftX, meterTop, barWidth, meterHeight, BarBackground);
                builder.FillRectangle(barRightX, meterTop, barWidth, meterHeight, BarBackground);

                // Draw level bars (from bottom up)
                Int32 heightL = GetBarHeight(peakL, meterHeight);
                Int32 heightR = GetBarHeight(peakR, meterHeight);

                PluginLog.Debug($"[VuMeter] '{inputName}' peakL={peakL:F4} peakR={peakR:F4} heightL={heightL} heightR={heightR} meterH={meterHeight} barW={barWidth} imgW={width} imgH={height}");

                if (heightL > 0)
                {
                    BitmapColor colorL = GetBarColor(peakL);
                    Int32 yL = meterTop + meterHeight - heightL;
                    PluginLog.Debug($"[VuMeter] L: FillRect({barLeftX}, {yL}, {barWidth}, {heightL}, RGB({colorL.R},{colorL.G},{colorL.B}))");
                    builder.FillRectangle(barLeftX, yL, barWidth, heightL, colorL);
                }

                if (heightR > 0)
                {
                    BitmapColor colorR = GetBarColor(peakR);
                    Int32 yR = meterTop + meterHeight - heightR;
                    PluginLog.Debug($"[VuMeter] R: FillRect({barRightX}, {yR}, {barWidth}, {heightR}, RGB({colorR.R},{colorR.G},{colorR.B}))");
                    builder.FillRectangle(barRightX, yR, barWidth, heightR, colorR);
                }

                // Draw input name at bottom
                if (!String.IsNullOrEmpty(inputName))
                {
                    String displayName = inputName.Length > 10 ? inputName.Substring(0, 9) + "\u2026" : inputName;
                    builder.DrawText(displayName, 0, height - textHeight, width, textHeight, BitmapColor.White, 10);
                }

                return builder.ToImage();
            }
        }
    }
}
