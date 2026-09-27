namespace Loupedeck.OBSStudioForLogiPlugin.Models
{
    using System;

    public class AudioMeterLevels
    {
        public Single[] ChannelPeaks { get; set; } = new Single[0];

        public Boolean HasData => this.ChannelPeaks.Length > 0;

        public static AudioMeterLevels Empty => new AudioMeterLevels();
    }
}
