namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    internal class IconWithTextImageFactory : IActionImageFactory<IconWithTextImageData>
    {
        public IActionImageFactory<IconWithTextImageData> Create()
        {
            return new IconWithTextImageFactory();
        }

        public BitmapImage DrawBitmapImage(IconWithTextImageData data, PluginImageSize imageSize)
        {
            return ButtonTextRenderer.RenderIconWithText(
                data.IconPath,
                data.DisplayText,
                imageSize,
                data.TextColor);
        }
    }
}
