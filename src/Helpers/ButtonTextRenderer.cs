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

        public static BitmapImage RenderTextWithIcon(String text, PluginImageSize imageSize, String iconResourceName, BitmapColor? textColor = null)
        {
            BitmapColor fgColor = textColor ?? BitmapColor.White;
            Int32 fontSize = GetDynamicFontSize(text, imageSize);
            
            using (var builder = new BitmapBuilder(imageSize))
            {
                builder.Clear(BitmapColor.Black);
                
                if (!String.IsNullOrEmpty(iconResourceName))
                {
                    try
                    {
                        BitmapImage iconImage = PluginResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconResourceName}");
                        
                        if (iconImage != null)
                        {
                            builder.DrawImage(iconImage.ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                        // Icon failed to load, continue with text-only rendering
                        PluginLog.Warning($"Failed to load icon '{iconResourceName}': {ex.Message}");
                    }
                }
                
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

        public static BitmapImage RenderTextWithBorder(String text, PluginImageSize imageSize, BitmapColor textColor, Boolean showBorder)
        {
            Int32 fontSize = GetDynamicFontSize(text, imageSize);
            
            using (var builder = new BitmapBuilder(imageSize))
            {
                builder.Clear(BitmapColor.Black);
                
                if (showBorder)
                {
                    Int32 width = builder.Width;
                    Int32 height = builder.Height;
                    Int32 borderWidth = 3;
                    BitmapColor borderColor = BitmapColor.White;
                    
                    // Draw border rectangle
                    builder.FillRectangle(0, 0, width, borderWidth, borderColor); // Top
                    builder.FillRectangle(0, height - borderWidth, width, borderWidth, borderColor); // Bottom
                    builder.FillRectangle(0, 0, borderWidth, height, borderColor); // Left
                    builder.FillRectangle(width - borderWidth, 0, borderWidth, height, borderColor); // Right
                }
                
                builder.DrawText(text, textColor, fontSize);
                return builder.ToImage();
            }
        }

        public static BitmapImage RenderTextWithBorder(String text, Int32 imageWidth, Int32 imageHeight, BitmapColor textColor, Boolean showBorder)
        {
            Int32 fontSize = GetDynamicFontSizeFromDimensions(text, imageWidth);

            using (var builder = new BitmapBuilder(imageWidth, imageHeight))
            {
                builder.Clear(BitmapColor.Black);

                if (showBorder)
                {
                    Int32 borderWidth = 3;
                    BitmapColor borderColor = BitmapColor.White;

                    builder.FillRectangle(0, 0, imageWidth, borderWidth, borderColor);
                    builder.FillRectangle(0, imageHeight - borderWidth, imageWidth, borderWidth, borderColor);
                    builder.FillRectangle(0, 0, borderWidth, imageHeight, borderColor);
                    builder.FillRectangle(imageWidth - borderWidth, 0, borderWidth, imageHeight, borderColor);
                }

                builder.DrawText(text, textColor, fontSize);
                return builder.ToImage();
            }
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
            Int32 baseSize = imageSize == PluginImageSize.Width90 ? 90 : 60;
            return GetDynamicFontSizeFromDimensions(text, baseSize);
        }

        private static Int32 GetDynamicFontSizeFromDimensions(String text, Int32 width)
        {
            if (String.IsNullOrEmpty(text))
            {
                return width >= 90 ? 18 : 16;
            }

            Int32 textLength = text.Length;
            Int32 lineCount = text.Split('\n').Length;

            if (lineCount == 1)
            {
                if (textLength <= 8)
                    return width >= 90 ? 18 : 16;
                else if (textLength <= 12)
                    return width >= 90 ? 15 : 13;
                else if (textLength <= 16)
                    return width >= 90 ? 13 : 11;
                else if (textLength <= 20)
                    return width >= 90 ? 11 : 9;
                else
                    return width >= 90 ? 9 : 8;
            }
            else
            {
                return width >= 90 ? 13 : 11;
            }
        }
    }
}
