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
    }
}
