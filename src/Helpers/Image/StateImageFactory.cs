namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Concurrent;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    internal class StateImageFactory : IActionImageFactory<StateImageData>
    {
        private readonly Object locker;
        private readonly ConcurrentDictionary<String, Bitmap> iconCache;
        private readonly Bitmap imageWidth50;
        private readonly Bitmap imageWidth80;
        private readonly Graphics graphicsWidth50;
        private readonly Graphics graphicsWidth80;

        public StateImageFactory()
        {
            this.locker = new Object();
            this.iconCache = new ConcurrentDictionary<String, Bitmap>();
            this.imageWidth50 = new Bitmap(50, 50);
            this.graphicsWidth50 = Graphics.FromImage(this.imageWidth50);
            this.imageWidth80 = new Bitmap(80, 80);
            this.graphicsWidth80 = Graphics.FromImage(this.imageWidth80);
            
            this.graphicsWidth50.SmoothingMode = SmoothingMode.AntiAlias;
            this.graphicsWidth50.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.graphicsWidth50.PixelOffsetMode = PixelOffsetMode.HighQuality;
            
            this.graphicsWidth80.SmoothingMode = SmoothingMode.AntiAlias;
            this.graphicsWidth80.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.graphicsWidth80.PixelOffsetMode = PixelOffsetMode.HighQuality;
        }

        public IActionImageFactory<StateImageData> Create()
        {
            return new StateImageFactory();
        }

        private Bitmap GetIcon(String iconPath)
        {
            return this.iconCache.GetOrAdd(iconPath, (key) =>
            {
                BitmapImage image = EmbeddedResources.ReadImage(iconPath);
                if (image == null)
                {
                    PluginLog.Warning($"Failed to load icon: '{iconPath}'");
                    return BitmapHelper.CreateBlackBitmap(50, 50);
                }
                
                return BitmapHelper.ToBitmap(image);
            });
        }

        public BitmapImage DrawBitmapImage(StateImageData data, PluginImageSize imageSize)
        {
            lock (this.locker)
            {
                String iconPath = data.IsActive ? data.ActiveIconPath : data.InactiveIconPath;
                Bitmap icon = this.GetIcon(iconPath);
                
                if (imageSize == PluginImageSize.Width60)
                {
                    this.graphicsWidth50.Clear(Color.Black);
                    Int32 x = (this.imageWidth50.Width - icon.Width) / 2;
                    Int32 y = (this.imageWidth50.Height - icon.Height) / 2;
                    this.graphicsWidth50.DrawImage(icon, x, y, icon.Width, icon.Height);
                    return BitmapHelper.ToBitmapImage(this.imageWidth50);
                }
                else
                {
                    this.graphicsWidth80.Clear(Color.Black);
                    Int32 x = (this.imageWidth80.Width - icon.Width) / 2;
                    Int32 y = (this.imageWidth80.Height - icon.Height) / 2;
                    this.graphicsWidth80.DrawImage(icon, x, y, icon.Width, icon.Height);
                    return BitmapHelper.ToBitmapImage(this.imageWidth80);
                }
            }
        }
    }
}
