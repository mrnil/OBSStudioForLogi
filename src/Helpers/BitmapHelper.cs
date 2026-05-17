namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    internal static class BitmapHelper
    {
        public static Bitmap ToBitmap(BitmapImage image)
        {
            using (MemoryStream stream = new MemoryStream(image.ToArray()))
            {
                return new Bitmap(stream);
            }
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            ImageConverter converter = new ImageConverter();
            return BitmapImage.FromArray(converter.ConvertTo(bitmap, typeof(Byte[])) as Byte[]);
        }

        public static Bitmap CreateBlackBitmap(Int32 width, Int32 height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);
            }
            return bitmap;
        }

        public static Bitmap ScaleBitmap(Bitmap source, Int32 width, Int32 height)
        {
            Bitmap scaled = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(scaled))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, 0, 0, width, height);
            }
            return scaled;
        }

        public static void GetImageSize(PluginImageSize imageSize, out Int32 imageWidth, out Int32 imageHeight)
        {
            if (imageSize == PluginImageSize.Width60)
            {
                imageWidth = 50;
                imageHeight = 50;
            }
            else if (imageSize == PluginImageSize.Width90)
            {
                imageWidth = 80;
                imageHeight = 80;
            }
            else
            {
                imageWidth = 80;
                imageHeight = 80;
            }
        }
    }
}
