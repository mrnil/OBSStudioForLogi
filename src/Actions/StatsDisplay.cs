namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Timers;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;
    using Loupedeck.OBSStudioForLogiPlugin.Models;

    // Stats are only polled while something shows them. A plain command gets no
    // Activate/Deactivate from the SDK, so - as for AudioMeterCommand - visibility is inferred:
    // the SDK only asks for images of buttons it is showing, so each image request renews a lease
    // that keeps this button registered as a stats viewer. The probe timer invalidates the image
    // every second, so a visible button keeps renewing the lease and one scrolled into view asks
    // to be drawn, and drops the viewer once requests stop.
    public class StatsDisplay : PluginDynamicCommand, IObsCommand
    {
        public static StatsDisplay Instance { get; private set; }

        private const String ViewerName = nameof(StatsDisplay);

        private readonly Timer _probeTimer = new Timer(OBSTimings.StatsIdleProbeInterval);
        private readonly ActivityLease _renderLease = new ActivityLease(TimeSpan.FromMilliseconds(OBSTimings.StatsRenderLease));

        public StatsDisplay()
            : base(displayName: "Stats Summary", description: "Shows OBS performance summary (CPU, FPS, Dropped frames)", groupName: "1. OBS")
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.IsWidget = true;
            this.AddParameter("", "Stats Summary", groupName: "1. OBS");

            this._probeTimer.Elapsed += this.OnProbeTimer;
            this._probeTimer.AutoReset = true;
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (this._renderLease.Touch())
            {
                PluginLog.Debug("Stats summary button visible - requesting stats");
                OBSStudioForLogiPlugin.Instance?.AddStatsViewer(ViewerName);
            }

            var stats = OBSStudioForLogiPlugin.Instance?.GetCurrentStats() ?? Models.OBSStats.Empty;

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

        private void OnProbeTimer(Object sender, ElapsedEventArgs e)
        {
            if (this._renderLease.ExpireIfIdle())
            {
                PluginLog.Debug("Stats summary button no longer visible - releasing stats");
                OBSStudioForLogiPlugin.Instance?.RemoveStatsViewer(ViewerName);
            }

            this.ActionImageChanged("");
        }

        public void OnConnected()
        {
            this._probeTimer.Start();
        }

        public void OnDisconnected()
        {
            this._probeTimer.Stop();

            if (this._renderLease.Release())
            {
                OBSStudioForLogiPlugin.Instance?.RemoveStatsViewer(ViewerName);
            }

            this.ActionImageChanged("");
        }
    }
}
