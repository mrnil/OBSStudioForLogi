namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Concurrent;

    internal class SimpleIconImageFactory : IActionImageFactory<SimpleIconImageData>
    {
        private readonly Object locker;
        private readonly ConcurrentDictionary<String, BitmapImage> iconCache;

        public SimpleIconImageFactory()
        {
            this.locker = new Object();
            this.iconCache = new ConcurrentDictionary<String, BitmapImage>();
        }

        public IActionImageFactory<SimpleIconImageData> Create()
        {
            return new SimpleIconImageFactory();
        }

        private BitmapImage GetIcon(String iconPath)
        {
            return this.iconCache.GetOrAdd(iconPath, (key) =>
            {
                BitmapImage image = EmbeddedResources.ReadImage(iconPath);
                if (image == null)
                {
                    PluginLog.Warning($"Failed to load icon: '{iconPath}'");
                }
                return image;
            });
        }

        public BitmapImage DrawBitmapImage(SimpleIconImageData data, PluginImageSize imageSize)
        {
            lock (this.locker)
            {
                BitmapImage icon = this.GetIcon(data.IconPath);
                if (icon == null)
                {
                    using (var builder = new BitmapBuilder(imageSize))
                    {
                        builder.Clear(BitmapColor.Black);
                        return builder.ToImage();
                    }
                }
                
                return icon;
            }
        }
    }
}
