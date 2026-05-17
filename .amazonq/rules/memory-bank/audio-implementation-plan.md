# Audio-Specific Implementation Plan

## Overview
This document records the planned implementation for audio mixer and scene audio buttons using the Factory + Store + Data pattern learned from AudioControlPlugin.

## Phase 2: Audio-Specific Implementation (DEFERRED)

### Step 2.1: Create Audio Image Data Model

```csharp
// Models/AudioInputImageData.cs
public class AudioInputImageData : IActionImageData, IEquatable<AudioInputImageData>
{
    public String Id { get; set; }
    public String InputName { get; set; }
    public Boolean IsMuted { get; set; }
    public String IconPath { get; set; }
    
    public Boolean Equals(AudioInputImageData other)
    {
        if (other is null) return false;
        return this.Id == other.Id 
            && this.InputName == other.InputName
            && this.IsMuted == other.IsMuted
            && this.IconPath == other.IconPath;
    }
    
    public override Int32 GetHashCode() 
        => (this.Id, this.InputName, this.IsMuted, this.IconPath).GetHashCode();
    
    public override Boolean Equals(Object obj) 
        => obj is AudioInputImageData other && this.Equals(other);
    
    Boolean IEquatable<IActionImageData>.Equals(IActionImageData other) 
        => this.Equals(other as AudioInputImageData);
}
```

### Step 2.2: Create Audio Image Factory

```csharp
// Helpers/Image/AudioInputImageFactory.cs
internal class AudioInputImageFactory : IActionImageFactory<AudioInputImageData>
{
    private readonly Object locker = new Object();
    private readonly ConcurrentDictionary<String, Bitmap> iconCache;
    private readonly Bitmap imageWidth50;
    private readonly Bitmap imageWidth80;
    private readonly Graphics graphicsWidth50;
    private readonly Graphics graphicsWidth80;
    private readonly Font font10;
    private readonly Font font12;
    private readonly Brush whiteBrush;
    private readonly Brush greenBrush;
    private readonly Brush redBrush;
    private readonly StringFormat centerFormat;
    
    public AudioInputImageFactory()
    {
        this.iconCache = new ConcurrentDictionary<String, Bitmap>();
        this.imageWidth50 = new Bitmap(50, 50);
        this.graphicsWidth50 = Graphics.FromImage(this.imageWidth50);
        this.imageWidth80 = new Bitmap(80, 80);
        this.graphicsWidth80 = Graphics.FromImage(this.imageWidth80);
        this.font10 = new Font("Calibri", 10, FontStyle.Regular);
        this.font12 = new Font("Calibri", 12, FontStyle.Regular);
        this.whiteBrush = new SolidBrush(Color.White);
        this.greenBrush = new SolidBrush(Color.LimeGreen);
        this.redBrush = new SolidBrush(Color.Red);
        this.centerFormat = new StringFormat 
        { 
            Alignment = StringAlignment.Center, 
            LineAlignment = StringAlignment.Center 
        };
    }
    
    public IActionImageFactory<AudioInputImageData> Create() 
        => new AudioInputImageFactory();
    
    private Bitmap GetIcon(String iconPath, Boolean isMuted)
    {
        String cacheKey = $"{iconPath}_{(isMuted ? "muted" : "unmuted")}";
        return this.iconCache.GetOrAdd(cacheKey, (key) =>
        {
            var image = EmbeddedResources.ReadImage(iconPath);
            if (image == null) return CreateBlackBitmap(32, 32);
            
            var bitmap = BitmapHelper.ToBitmap(image);
            return ScaleBitmap(bitmap, 32, 32);
        });
    }
    
    private Bitmap DrawWidth50(AudioInputImageData data)
    {
        this.graphicsWidth50.Clear(Color.Black);
        
        Bitmap icon = this.GetIcon(data.IconPath, data.IsMuted);
        Int32 iconX = (this.imageWidth50.Width - icon.Width) / 2;
        this.graphicsWidth50.DrawImage(icon, iconX, 5, icon.Width, icon.Height);
        
        Brush textBrush = data.IsMuted ? this.redBrush : this.greenBrush;
        this.graphicsWidth50.DrawString(
            data.InputName, 
            this.font10, 
            textBrush, 
            new Rectangle(0, this.imageWidth50.Height - 12, this.imageWidth50.Width, 12), 
            this.centerFormat);
        
        return this.imageWidth50;
    }
    
    private Bitmap DrawWidth80(AudioInputImageData data)
    {
        this.graphicsWidth80.Clear(Color.Black);
        
        Bitmap icon = this.GetIcon(data.IconPath, data.IsMuted);
        Int32 iconX = (this.imageWidth80.Width - icon.Width) / 2;
        Int32 iconY = 20;
        this.graphicsWidth80.DrawImage(icon, iconX, iconY, icon.Width, icon.Height);
        
        Brush textBrush = data.IsMuted ? this.redBrush : this.greenBrush;
        this.graphicsWidth80.DrawString(
            data.InputName, 
            this.font12, 
            textBrush, 
            new Rectangle(0, this.imageWidth80.Height - 15, this.imageWidth80.Width, 15), 
            this.centerFormat);
        
        return this.imageWidth80;
    }
    
    public BitmapImage DrawBitmapImage(AudioInputImageData data, PluginImageSize imageSize)
    {
        lock (this.locker)
        {
            Bitmap bitmap = imageSize == PluginImageSize.Width60 
                ? this.DrawWidth50(data) 
                : this.DrawWidth80(data);
            return BitmapHelper.ToBitmapImage(bitmap);
        }
    }
    
    private static Bitmap CreateBlackBitmap(Int32 width, Int32 height)
    {
        var bitmap = new Bitmap(width, height);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Black);
        }
        return bitmap;
    }
    
    private static Bitmap ScaleBitmap(Bitmap source, Int32 width, Int32 height)
    {
        var scaled = new Bitmap(width, height);
        using (var g = Graphics.FromImage(scaled))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(source, 0, 0, width, height);
        }
        return scaled;
    }
}
```

### Step 2.3: Refactor AudioInputDynamicFolderBase

```csharp
// Actions/AudioInputDynamicFolderBase.cs
public abstract class AudioInputDynamicFolderBase : PluginDynamicFolder
{
    protected String[] AudioInputs = new String[0];
    private readonly ActionImageStore<AudioInputImageData> imageStore;
    
    protected AudioInputDynamicFolderBase()
    {
        this.imageStore = new ActionImageStore<AudioInputImageData>(new AudioInputImageFactory());
    }
    
    public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
    {
        var isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
        var iconName = isMuted ? "AudioMixerMuted.svg" : "AudioMixerUnmuted.svg";
        var iconPath = $"Loupedeck.OBSStudioForLogiPlugin.Icons.{iconName}";
        
        var imageData = new AudioInputImageData
        {
            Id = actionParameter,
            InputName = actionParameter,
            IsMuted = isMuted,
            IconPath = iconPath
        };
        
        this.imageStore.UpdateImage(imageData.Id, imageData);
        
        return this.imageStore.TryGetImage(imageData.Id, imageSize, out var image)
            ? image
            : ButtonTextRenderer.RenderIconWithText(iconPath, actionParameter, imageSize, 
                isMuted ? BitmapColor.Red : BitmapColor.Green);
    }
    
    // Rest remains unchanged
}
```

## Optional Enhancement: Volume-Based Icon Selection

```csharp
// Helpers/AudioIconSelector.cs
internal static class AudioIconSelector
{
    public static String GetIconPath(Boolean isMuted, Single volumeLevel)
    {
        if (isMuted) return "AudioMixerMuted.svg";
        
        if (volumeLevel <= 0.0f) return "AudioMixerUnmuted0.svg";
        else if (volumeLevel < 0.33f) return "AudioMixerUnmuted1.svg";
        else if (volumeLevel < 0.66f) return "AudioMixerUnmuted2.svg";
        else return "AudioMixerUnmuted3.svg";
    }
}
```

## Benefits

- **90%+ reduction** in image generation for audio buttons
- **Consistent visual design** with pre-allocated Graphics objects
- **Icon caching** prevents repeated file I/O
- **Equality checking** prevents unnecessary regeneration
- **Centralized styling** for easy maintenance

## Implementation Trigger

Implement this phase after Phase 1 (Foundation) is complete and all existing action buttons have been migrated to use the ActionImageStore pattern.
