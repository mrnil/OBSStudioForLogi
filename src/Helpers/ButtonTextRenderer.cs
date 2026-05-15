namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public static class ButtonTextRenderer
    {
        public static BitmapImage RenderText(String text, PluginImageSize imageSize, BitmapColor? backgroundColor = null, BitmapColor? textColor = null)
        {
            var bgColor = backgroundColor ?? BitmapColor.Black;
            var fgColor = textColor ?? BitmapColor.White;
            var fontSize = GetLargeFontSize(imageSize);
            
            using (var builder = new BitmapBuilder(imageSize))
            {
                builder.Clear(bgColor);
                builder.DrawText(text, fgColor, fontSize);
                return builder.ToImage();
            }
        }

        public static BitmapImage RenderIconWithText(String iconResourceName, String text, PluginImageSize imageSize, BitmapColor? textColor = null)
        {
            var fgColor = textColor ?? BitmapColor.White;
            var fontSize = GetLargeFontSize(imageSize);
            
            PluginLog.Info($"ButtonTextRenderer.RenderIconWithText - iconResourceName: '{iconResourceName}', text: '{text}', imageSize: {imageSize}, fontSize: {fontSize}");
            
            var iconImage = PluginResources.ReadImage(iconResourceName);
            
            if (iconImage != null)
            {
                PluginLog.Info($"Icon loaded successfully: {iconImage.Width}x{iconImage.Height}");
            }
            else
            {
                PluginLog.Warning($"Failed to load icon: '{iconResourceName}'");
            }
            
            using (var builder = new BitmapBuilder(imageSize))
            {
                if (iconImage != null)
                {
                    builder.DrawImage(iconImage);
                }
                else
                {
                    builder.Clear(BitmapColor.Black);
                }
                
                builder.DrawText(text, fgColor, fontSize);
                var result = builder.ToImage();
                PluginLog.Info($"Rendered image: {result.Width}x{result.Height}");
                return result;
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
            return imageSize == PluginImageSize.Width90 ? 18 : 16;
        }
    }
}
