namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public static class ButtonImageHelper
    {
        // For simple icon buttons (static SVG)
        public static BitmapImage Icon(String iconResourceName)
        {
            return PluginResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconResourceName}");
        }

        // For icon with coloured background
        public static BitmapImage IconWithBackground(String iconResourceName, PluginImageSize imageSize, BitmapColor backgroundColor)
        {
            using (var builder = new BitmapBuilder(imageSize))
            {
                builder.Clear(backgroundColor);

                try
                {
                    var iconImage = PluginResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconResourceName}");
                    if (iconImage != null)
                    {
                        builder.DrawImage(iconImage, 0, 0, builder.Width, builder.Height);
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.Warning($"Failed to load icon '{iconResourceName}': {ex.Message}");
                }

                return builder.ToImage();
            }
        }
        
        // For state-based icons (on/off, active/inactive)
        public static BitmapImage StateIcon(Boolean isActive, String activeIcon, String inactiveIcon)
        {
            return Icon(isActive ? activeIcon : inactiveIcon);
        }
        
        // For text-only buttons
        public static BitmapImage Text(String text, PluginImageSize imageSize, 
            BitmapColor? backgroundColor = null, BitmapColor? textColor = null)
        {
            return ButtonTextRenderer.RenderText(text, imageSize, backgroundColor, textColor);
        }
        
        // For text with colored state
        public static BitmapImage StateText(String text, PluginImageSize imageSize, 
            Boolean isActive, BitmapColor activeColor, BitmapColor inactiveColor)
        {
            return Text(text, imageSize, BitmapColor.Black, isActive ? activeColor : inactiveColor);
        }
        
        // For text with background icon
        public static BitmapImage TextWithIcon(String text, PluginImageSize imageSize, 
            String iconResourceName, BitmapColor? textColor = null)
        {
            return ButtonTextRenderer.RenderTextWithIcon(text, imageSize, iconResourceName, textColor);
        }
        
        // For text with state-based background icon
        public static BitmapImage StateTextWithIcon(String text, PluginImageSize imageSize, 
            Boolean isActive, String activeIcon, String inactiveIcon, 
            BitmapColor activeColor, BitmapColor inactiveColor)
        {
            String icon = isActive ? activeIcon : inactiveIcon;
            BitmapColor color = isActive ? activeColor : inactiveColor;
            return ButtonTextRenderer.RenderTextWithIcon(text, imageSize, icon, color);
        }
        
        // For text with colored state and optional border
        public static BitmapImage StateTextWithBorder(String text, PluginImageSize imageSize, 
            Boolean isActive, BitmapColor activeColor, BitmapColor inactiveColor, Boolean showBorder)
        {
            return ButtonTextRenderer.RenderTextWithBorder(text, imageSize, 
                isActive ? activeColor : inactiveColor, showBorder);
        }

        // For text with colored state and optional border (raw dimensions for ActionEditorCommand)
        public static BitmapImage StateTextWithBorder(String text, Int32 imageWidth, Int32 imageHeight,
            Boolean isActive, BitmapColor activeColor, BitmapColor inactiveColor, Boolean showBorder)
        {
            return ButtonTextRenderer.RenderTextWithBorder(text, imageWidth, imageHeight,
                isActive ? activeColor : inactiveColor, showBorder);
        }
    }
}
