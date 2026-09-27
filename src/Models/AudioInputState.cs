namespace Loupedeck.OBSStudioForLogiPlugin.Models
{
    using System;

    public class AudioInputState
    {
        public const String MonitorTypeNone = "OBS_MONITORING_TYPE_NONE";

        public Boolean IsMuted { get; set; }

        public Single VolumeMul { get; set; } = 1.0f;

        public String MonitorType { get; set; } = MonitorTypeNone;

        public static AudioInputState Default => new AudioInputState();
    }
}
