namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class StreamStatsDynamicFolder : PluginDynamicFolder
    {
        public static StreamStatsDynamicFolder Instance { get; private set; }

        private static readonly String[] StatKeys = new[]
        {
            "duration", "bytes", "congestion", "skipped", "total_frames"
        };

        public StreamStatsDynamicFolder()
        {
            Instance = this;
            this.DisplayName = "Stream Stats Folder";
            this.GroupName = "2. Streaming";
            this.Description = "Folder showing live streaming statistics";
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            return StatKeys.Select(key => this.CreateCommandName(key));
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return String.Empty;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var stats = OBSStudioForLogiPlugin.Instance?.GetCurrentStreamStats();

            if (stats == null || !stats.IsActive)
            {
                return ButtonTextRenderer.RenderText(GetLabel(actionParameter) + "\nOffline", imageSize, BitmapColor.Black, new BitmapColor(128, 128, 128));
            }

            String text;
            BitmapColor color;

            switch (actionParameter)
            {
                case "duration":
                    color = new BitmapColor(80, 255, 80);
                    text = $"Duration\n{stats.DurationFormatted}";
                    break;
                case "bytes":
                    var mb = stats.BytesSent / (1024.0 * 1024.0);
                    color = new BitmapColor(180, 180, 255);
                    text = mb >= 1024 ? $"Sent\n{mb / 1024:F2} GB" : $"Sent\n{mb:F1} MB";
                    break;
                case "congestion":
                    var congestionPct = stats.Congestion * 100;
                    color = congestionPct > 50 ? new BitmapColor(255, 80, 80) : congestionPct > 20 ? new BitmapColor(255, 200, 0) : new BitmapColor(80, 255, 80);
                    text = $"Network\nCongestion\n{congestionPct:F1}%";
                    break;
                case "skipped":
                    var skipped = stats.SkippedFrames;
                    var pct = stats.SkippedPercent;
                    color = skipped > 0 ? new BitmapColor(255, 80, 80) : new BitmapColor(80, 255, 80);
                    text = $"Skipped\nFrames\n{skipped} ({pct:F1}%)";
                    break;
                case "total_frames":
                    color = new BitmapColor(180, 180, 255);
                    text = $"Total\nFrames\n{stats.TotalFrames}";
                    break;
                default:
                    text = actionParameter;
                    color = BitmapColor.White;
                    break;
            }

            return ButtonTextRenderer.RenderText(text, imageSize, BitmapColor.Black, color);
        }

        public override void RunCommand(String actionParameter)
        {
            // Display only
        }

        public void UpdateDisplay()
        {
            foreach (var key in StatKeys)
            {
                this.CommandImageChanged(key);
            }
        }

        private static String GetLabel(String key)
        {
            switch (key)
            {
                case "duration": return "Duration";
                case "bytes": return "Sent";
                case "congestion": return "Congestion";
                case "skipped": return "Skipped";
                case "total_frames": return "Frames";
                default: return key;
            }
        }
    }
}
