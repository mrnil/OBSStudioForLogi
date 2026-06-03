namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using Loupedeck.Devices.Loupedeck2Devices;

    public class AudioVolumeWheelTool : WheelTool
    {
        public AudioVolumeWheelTool() : base("OBS Volume", "OBS Studio") { }

        protected override void OnEncoderEvent(DeviceEncoderEvent e)
        {
            var sel = AudioSelectionState.SelectedInput;
            if (String.IsNullOrEmpty(sel))
            {
                PluginLog.Info("Wheel tool: No audio source selected");
                return;
            }

            var cur = OBSStudioForLogiPlugin.Instance?.GetInputVolume(sel) ?? 1.0f;
            var target = Math.Clamp(cur + e.Clicks * 0.01f, 0f, 20f);
            OBSStudioForLogiPlugin.Instance?.SetInputVolume(sel, target);
            PluginLog.Info($"Wheel tool adjusted volume for '{sel}': {(Int32)(target * 100)}%");
            this.DrawDelayed();
        }

        protected override BitmapImage CreateImage()
        {
            using (var bb = this.CreateBitmapBuilder())
            {
                bb.Clear(BitmapColor.Black);
                var sel = AudioSelectionState.SelectedInput;
                var vol = OBSStudioForLogiPlugin.Instance?.GetInputVolume(sel) ?? 1.0f;
                bb.DrawText(String.IsNullOrEmpty(sel) ? "No source" : $"{sel}\n{(Int32)(vol * 100)}%", BitmapColor.White);
                return bb.ToImage();
            }
        }
    }
}
