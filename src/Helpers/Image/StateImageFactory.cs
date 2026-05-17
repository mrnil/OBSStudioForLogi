namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Concurrent;

    internal class StateImageFactory : IActionImageFactory<StateImageData>
    {
        private readonly ConcurrentDictionary<String, BitmapImage> iconCache;

        public StateImageFactory()
        {
            this.iconCache = new ConcurrentDictionary<String, BitmapImage>();
        }

        public IActionImageFactory<StateImageData> Create()
        {
            return new StateImageFactory();
        }

        public BitmapImage DrawBitmapImage(StateImageData data, PluginImageSize imageSize)
        {
            String iconPath = data.IsActive ? data.ActiveIconPath : data.InactiveIconPath;
            String cacheKey = $"{iconPath}_{imageSize}";

            return this.iconCache.GetOrAdd(cacheKey, (key) =>
            {
                BitmapImage image = EmbeddedResources.ReadImage(iconPath);
                if (image == null)
                {
                    PluginLog.Warning($"Failed to load icon: '{iconPath}'");
                    BitmapHelper.GetImageSize(imageSize, out Int32 width, out Int32 height);
                    return BitmapHelper.ToBitmapImage(BitmapHelper.CreateBlackBitmap(width, height));
                }
                return image;
            });
        }
    }
}
