namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    internal class TextImageFactory : IActionImageFactory<TextImageData>
    {
        public IActionImageFactory<TextImageData> Create()
        {
            return new TextImageFactory();
        }

        public BitmapImage DrawBitmapImage(TextImageData data, PluginImageSize imageSize)
        {
            return ButtonTextRenderer.RenderText(
                data.DisplayText,
                imageSize,
                data.BackgroundColor,
                data.TextColor);
        }
    }
}
