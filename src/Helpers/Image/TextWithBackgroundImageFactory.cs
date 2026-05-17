namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    internal class TextWithBackgroundImageFactory : IActionImageFactory<TextImageData>
    {
        private readonly Object locker;

        public TextWithBackgroundImageFactory()
        {
            this.locker = new Object();
        }

        public IActionImageFactory<TextImageData> Create()
        {
            return new TextWithBackgroundImageFactory();
        }

        public BitmapImage DrawBitmapImage(TextImageData data, PluginImageSize imageSize)
        {
            lock (this.locker)
            {
                using (var builder = new BitmapBuilder(imageSize))
                {
                    builder.Clear(data.BackgroundColor);
                    builder.DrawText(data.DisplayText, data.TextColor);
                    return builder.ToImage();
                }
            }
        }
    }
}
