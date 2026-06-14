namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StatsDisplay : PluginDynamicCommand
    {
        public static StatsDisplay Instance { get; private set; }

        public StatsDisplay()
            : base(displayName: "OBS Stats", description: "Shows OBS performance stats (CPU, FPS, Dropped frames)", groupName: "1. OBS")
        {
            Instance = this;
            this.IsWidget = true;
            this.AddParameter("", "OBS Stats", groupName: "1. OBS");
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var stats = OBSStudioForLogiPlugin.Instance?.GetCurrentStats();

            if (stats == null)
            {
                return ButtonTextRenderer.RenderText("Stats\nN/A", imageSize, BitmapColor.Black, new BitmapColor(128, 128, 128));
            }

            var fps = stats.Fps;
            var cpu = stats.CpuUsage;
            var dropped = stats.TotalDroppedFrames;

            Boolean hasIssue = fps < 25 || cpu > 80 || dropped > 0;
            BitmapColor textColor = hasIssue ? new BitmapColor(255, 80, 80) : new BitmapColor(80, 255, 80);
            String text = $"FPS: {fps:F0}\nCPU: {cpu:F1}%\nDrop: {dropped}";

            return ButtonTextRenderer.RenderText(text, imageSize, BitmapColor.Black, textColor);
        }

        protected override void RunCommand(String actionParameter)
        {
            // Display only
        }

        public void UpdateDisplay()
        {
            this.ActionImageChanged("");
        }
    }
}
