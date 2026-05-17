namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Concurrent;

    internal class AudioInputImageFactory : IActionImageFactory<AudioInputImageData>
    {
        private readonly Object locker;
        private readonly ConcurrentDictionary<String, BitmapImage> iconCache;

        public AudioInputImageFactory()
        {
            this.locker = new Object();
            this.iconCache = new ConcurrentDictionary<String, BitmapImage>();
        }

        public IActionImageFactory<AudioInputImageData> Create() => new AudioInputImageFactory();

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

        public BitmapImage DrawBitmapImage(AudioInputImageData data, PluginImageSize imageSize)
        {
            lock (this.locker)
            {
                BitmapColor textColor = data.IsMuted ? BitmapColor.Red : BitmapColor.Green;
                Int32 volumePercent = (Int32)(data.VolumeLevel * 100);
                String displayText = $"{data.InputName}\n{volumePercent}%";

                return ButtonTextRenderer.RenderIconWithText(data.IconPath, displayText, imageSize, textColor);
            }
        }
    }
}
