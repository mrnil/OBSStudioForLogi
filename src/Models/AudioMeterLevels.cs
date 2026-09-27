namespace Loupedeck.OBSStudioForLogiPlugin.Models
{
    using System;

    public class AudioMeterLevels
    {
        public Single[] ChannelPeaks { get; set; } = new Single[0];

        public Boolean IsMuted { get; set; }

        // OBS is currently reporting this input as active, even if it has no channel data.
        public Boolean IsLive { get; set; }

        public Boolean HasData => this.ChannelPeaks.Length > 0;

        public static AudioMeterLevels Empty => new AudioMeterLevels();
    }
}
