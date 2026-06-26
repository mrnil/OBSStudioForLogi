namespace Loupedeck.OBSStudioForLogiPlugin.Models
{
    using System;

    public class OBSStats
    {
        public Double Fps { get; set; }
        public Double CpuUsage { get; set; }
        public Double MemoryUsage { get; set; }
        public Double AverageFrameTime { get; set; }
        public Double FreeDiskSpace { get; set; }
        public Int64 RenderTotalFrames { get; set; }
        public Int64 RenderMissedFrames { get; set; }
        public Int64 OutputTotalFrames { get; set; }
        public Int64 OutputSkippedFrames { get; set; }

        public Double RenderLagPercent => this.RenderTotalFrames > 0 ? (Double)this.RenderMissedFrames / this.RenderTotalFrames * 100 : 0;
        public Double EncodingLagPercent => this.OutputTotalFrames > 0 ? (Double)this.OutputSkippedFrames / this.OutputTotalFrames * 100 : 0;
        public Int64 TotalDroppedFrames => this.RenderMissedFrames + this.OutputSkippedFrames;
    }
}
