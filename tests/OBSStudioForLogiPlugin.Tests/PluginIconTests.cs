namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Drawing;
using System.IO;
using System.Runtime.Versioning;

/// <summary>
/// The plugin icon (metadata/Icon256x256.png) is shown in Options+, Loupedeck and the Logitech
/// Marketplace. The SDK requires a 256x256 PNG whose artwork fits the centred 192x192 area, with
/// everything around it transparent. The plugin also loads this file at runtime for Info.Icon256x256.
/// </summary>
[SupportedOSPlatform("windows")]
public class PluginIconTests
{
    private const Int32 Size = 256;
    private const Int32 SafeArea = 192;

    private static String IconPath()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "OBSStudioForLogiPlugin.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine(directory!.FullName, "src", "package", "metadata", "Icon256x256.png");
    }

    [Fact]
    public void PluginIcon_IsA256By256Png()
    {
        Byte[] header = new Byte[8];
        using (FileStream stream = File.OpenRead(IconPath()))
        {
            stream.ReadExactly(header);
        }

        Assert.Equal(new Byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, header);

        using Bitmap icon = new Bitmap(IconPath());
        Assert.Equal(Size, icon.Width);
        Assert.Equal(Size, icon.Height);
    }

    [Fact]
    public void PluginIcon_ArtworkFitsTheCentredSafeAreaWithTransparentPadding()
    {
        using Bitmap icon = new Bitmap(IconPath());
        Int32 low = (Size - SafeArea) / 2;
        Int32 high = low + SafeArea;
        Int32 outside = 0;

        for (Int32 y = 0; y < icon.Height; y++)
        {
            for (Int32 x = 0; x < icon.Width; x++)
            {
                Boolean inSafeArea = x >= low && x < high && y >= low && y < high;
                if (!inSafeArea && icon.GetPixel(x, y).A > 0)
                {
                    outside++;
                }
            }
        }

        Assert.True(outside == 0, $"{outside} non-transparent pixels outside the centred {SafeArea}x{SafeArea} area");
    }

    [Fact]
    public void PluginPackage_HasNoOtherPluginIcons()
    {
        String[] pngs = Directory.GetFiles(Path.GetDirectoryName(IconPath())!, "*.png");

        Assert.Equal(new[] { "Icon256x256.png" }, Array.ConvertAll(pngs, Path.GetFileName));
    }
}
