namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    // The Logi Plugin Service applies an icon template to button images, and its image colour
    // (white by default) overrides the colours of single-colour ("monochrome") SVGs. Every glyph
    // in this set is one colour, so on the device the blue, grey and red states all showed as
    // white and greyed-out buttons looked enabled. Drawing icons onto a bitmap first didn't
    // stop it. Every icon SVG carries a tiny second-colour marker in its top-right corner
    // (tools/icons/generate_icons.py, MARKER), so the service counts it as multicolour and keeps
    // the file's colours (device-checked 2026-10-13) - so Icon returns the SVG itself.
    public static class ButtonImageHelper
    {
        // Backgrounds for status keys, whose colour carries the state (Reconnect, Connection Status,
        // Toggle Recording). Draw a white glyph on them (the *White.svg icons): blue doesn't read.
        public static readonly BitmapColor StatusGreen = new BitmapColor(0, 128, 0);
        public static readonly BitmapColor StatusRed = new BitmapColor(128, 0, 0);

        // imageSize isn't used while icons are returned as SVGs; callers pass it so the bitmap
        // path can come back without touching them.
        public static BitmapImage Icon(String iconResourceName, PluginImageSize imageSize)
        {
            return ReadIcon(iconResourceName);
        }

        // For ActionEditorCommand images, which are sized in pixels.
        public static BitmapImage Icon(String iconResourceName, Int32 imageWidth, Int32 imageHeight)
        {
            return ReadIcon(iconResourceName);
        }

        public static BitmapImage IconWithBackground(String iconResourceName, PluginImageSize imageSize, BitmapColor backgroundColor)
        {
            using (BitmapBuilder builder = new BitmapBuilder(imageSize))
            {
                builder.Clear(backgroundColor);

                try
                {
                    builder.DrawImage(ReadIcon(iconResourceName), 0, 0, builder.Width, builder.Height);
                }
                catch (Exception ex)
                {
                    PluginLog.Warning($"Failed to load icon '{iconResourceName}': {ex.Message}");
                }

                return builder.ToImage();
            }
        }

        private static BitmapImage ReadIcon(String iconResourceName)
        {
            return PluginResources.ReadImage($"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconResourceName}");
        }
    }
}
