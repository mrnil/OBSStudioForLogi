namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public static class ButtonImageHelper
    {
        // Backgrounds for status keys, whose colour carries the state (Reconnect, Connection Status,
        // Toggle Recording). Draw a white glyph on them (the *White.svg icons): blue doesn't read.
        public static readonly BitmapColor StatusGreen = new BitmapColor(0, 128, 0);
        public static readonly BitmapColor StatusRed = new BitmapColor(128, 0, 0);

        public static BitmapImage Icon(String iconResourceName)
        {
            return PluginResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconResourceName}");
        }

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
    }
}
