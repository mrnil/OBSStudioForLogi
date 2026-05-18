namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public static class ButtonTextRenderer
    {
        public static BitmapImage RenderText(String text, PluginImageSize imageSize, BitmapColor? backgroundColor = null, BitmapColor? textColor = null)
        {
            var bgColor = backgroundColor ?? BitmapColor.Black;
            var fgColor = textColor ?? BitmapColor.White;
            var fontSize = GetDynamicFontSize(text, imageSize);
            
            using (var builder = new BitmapBuilder(imageSize))
            {
                builder.Clear(bgColor);
                builder.DrawText(text, fgColor, fontSize);
                return builder.ToImage();
            }
        }

        public static BitmapImage RenderIconWithText(String iconResourceName, String text, PluginImageSize imageSize, BitmapColor? textColor = null)
        {
            BitmapColor fgColor = textColor ?? BitmapColor.White;
            Int32 fontSize = GetLargeFontSize(imageSize);
            
            using (var builder = new BitmapBuilder(imageSize))
            {
                builder.Clear(BitmapColor.Black);
                builder.DrawText(text, fgColor, fontSize);
                return builder.ToImage();
            }
        }

        public static BitmapImage RenderConnectionStatus(Boolean isConnected, PluginImageSize imageSize)
        {
            var text = isConnected ? "Connected" : "Disconnected";
            var bgColor = isConnected ? new BitmapColor(0, 128, 0) : new BitmapColor(128, 0, 0);
            return RenderText(text, imageSize, bgColor, BitmapColor.White);
        }

        public static BitmapImage RenderNotConnected(PluginImageSize imageSize)
        {
            return RenderText("Not Connected", imageSize, BitmapColor.Black, new BitmapColor(128, 128, 128));
        }

        private static Int32 GetFontSize(PluginImageSize imageSize)
        {
            return imageSize == PluginImageSize.Width90 ? 13 : 11;
        }

        private static Int32 GetLargeFontSize(PluginImageSize imageSize)
        {
            return imageSize == PluginImageSize.Width90 ? 16 : 14;
        }

        private static Int32 GetDynamicFontSize(String text, PluginImageSize imageSize)
        {
            if (String.IsNullOrEmpty(text))
            {
                return GetLargeFontSize(imageSize);
            }

            Int32 textLength = text.Length;
            Int32 baseSize = imageSize == PluginImageSize.Width90 ? 90 : 60;
            
            // Count newlines to handle multi-line text
            Int32 lineCount = text.Split('\n').Length;
            
            // For single line text, scale based on length
            if (lineCount == 1)
            {
                if (textLength <= 8)
                {
                    return imageSize == PluginImageSize.Width90 ? 18 : 16;
                }
                else if (textLength <= 12)
                {
                    return imageSize == PluginImageSize.Width90 ? 15 : 13;
                }
                else if (textLength <= 16)
                {
                    return imageSize == PluginImageSize.Width90 ? 13 : 11;
                }
                else if (textLength <= 20)
                {
                    return imageSize == PluginImageSize.Width90 ? 11 : 9;
                }
                else
                {
                    return imageSize == PluginImageSize.Width90 ? 9 : 8;
                }
            }
            else
            {
                // Multi-line text - use smaller font
                return imageSize == PluginImageSize.Width90 ? 13 : 11;
            }
        }
    }
}
