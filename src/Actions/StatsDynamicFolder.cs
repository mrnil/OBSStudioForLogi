namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Loupedeck.OBSStudioForLogiPlugin.Models;

    public class StatsDynamicFolder : PluginDynamicFolder
    {
        public static StatsDynamicFolder Instance { get; private set; }

        private static readonly String[] StatKeys = new[]
        {
            "fps", "cpu", "memory", "render_missed", "output_skipped", "total_dropped", "disk_space", "frame_time"
        };

        public StatsDynamicFolder()
        {
            Instance = this;
            this.DisplayName = "OBS Stats Folder";
            this.GroupName = "1. OBS";
            this.Description = "Folder showing individual OBS performance statistics";
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
            var stats = OBSStudioForLogiPlugin.Instance?.GetCurrentStats() ?? Models.OBSStats.Empty;

            String text;
            BitmapColor color;

            switch (actionParameter)
            {
                case "fps":
                    var fps = stats.Fps;
                    color = fps < 25 ? new BitmapColor(255, 80, 80) : fps < 50 ? new BitmapColor(255, 200, 0) : new BitmapColor(80, 255, 80);
                    text = $"FPS\n{fps:F1}";
                    break;
                case "cpu":
                    var cpu = stats.CpuUsage;
                    color = cpu > 80 ? new BitmapColor(255, 80, 80) : cpu > 50 ? new BitmapColor(255, 200, 0) : new BitmapColor(80, 255, 80);
                    text = $"CPU\n{cpu:F1}%";
                    break;
                case "memory":
                    var mem = stats.MemoryUsage;
                    color = new BitmapColor(180, 180, 255);
                    text = $"Memory\n{mem:F0} MB";
                    break;
                case "render_missed":
                    var missed = stats.RenderMissedFrames;
                    var renderPct = stats.RenderLagPercent;
                    color = missed > 0 ? new BitmapColor(255, 80, 80) : new BitmapColor(80, 255, 80);
                    text = $"Render\nMissed\n{missed} ({renderPct:F1}%)";
                    break;
                case "output_skipped":
                    var skipped = stats.OutputSkippedFrames;
                    var encodePct = stats.EncodingLagPercent;
                    color = skipped > 0 ? new BitmapColor(255, 80, 80) : new BitmapColor(80, 255, 80);
                    text = $"Encode\nSkipped\n{skipped} ({encodePct:F1}%)";
                    break;
                case "total_dropped":
                    var total = stats.TotalDroppedFrames;
                    color = total > 0 ? new BitmapColor(255, 80, 80) : new BitmapColor(80, 255, 80);
                    text = $"Total\nDropped\n{total}";
                    break;
                case "disk_space":
                    var diskGb = stats.FreeDiskSpace / 1024.0;
                    color = diskGb < 1 ? new BitmapColor(255, 80, 80) : diskGb < 10 ? new BitmapColor(255, 200, 0) : new BitmapColor(180, 180, 255);
                    text = $"Disk\n{diskGb:F1} GB";
                    break;
                case "frame_time":
                    var ft = stats.AverageFrameTime;
                    color = ft > 10 ? new BitmapColor(255, 80, 80) : ft > 5 ? new BitmapColor(255, 200, 0) : new BitmapColor(80, 255, 80);
                    text = $"Render\nTime\n{ft:F2}ms";
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
                case "fps": return "FPS";
                case "cpu": return "CPU";
                case "memory": return "Memory";
                case "render_missed": return "Render\nMissed";
                case "output_skipped": return "Encode\nSkipped";
                case "total_dropped": return "Total\nDropped";
                case "disk_space": return "Disk";
                case "frame_time": return "Render\nTime";
                default: return key;
            }
        }
    }
}
