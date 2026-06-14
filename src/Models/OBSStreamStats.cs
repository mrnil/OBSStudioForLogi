namespace Loupedeck.OBSStudioForLogiPlugin.Models
{
    using System;

    public class OBSStreamStats
    {
        public Boolean IsActive { get; set; }
        public Int64 BytesSent { get; set; }
        public Int64 Duration { get; set; }
        public Double Congestion { get; set; }
        public Int64 SkippedFrames { get; set; }
        public Int64 TotalFrames { get; set; }

        public Double SkippedPercent => this.TotalFrames > 0 ? (Double)this.SkippedFrames / this.TotalFrames * 100 : 0;

        public String DurationFormatted
        {
            get
            {
                var ts = TimeSpan.FromMilliseconds(this.Duration);
                return ts.TotalHours >= 1 ? $"{(Int32)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}" : $"{ts.Minutes}:{ts.Seconds:D2}";
            }
        }
    }
}
