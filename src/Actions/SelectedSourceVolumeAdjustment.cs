namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class SelectedSourceVolumeAdjustment : PluginDynamicAdjustment, IObsCommand, IInputVolumeAwareCommand
    {
        public static SelectedSourceVolumeAdjustment Instance { get; private set; }

        public SelectedSourceVolumeAdjustment()
            : base("Selected Source Volume", "Volume of the currently selected OBS audio source", "8. Audio", hasReset: true)
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            var sel = AudioSelectionState.SelectedInput;
            if (String.IsNullOrEmpty(sel))
                return;

            Single current = OBSStudioForLogiPlugin.Instance?.GetInputVolume(sel) ?? 1.0f;
            Single target = Math.Clamp(current + diff * 0.01f, 0f, 20f);
            OBSStudioForLogiPlugin.Instance?.SetInputVolume(sel, target);
            this.AdjustmentValueChanged();
        }

        protected override void RunCommand(String actionParameter)
        {
            var sel = AudioSelectionState.SelectedInput;
            if (!String.IsNullOrEmpty(sel))
            {
                OBSStudioForLogiPlugin.Instance?.SetInputVolume(sel, 1.0f);
            }
            this.AdjustmentValueChanged();
        }

        protected override String GetAdjustmentValue(String actionParameter)
        {
            var sel = AudioSelectionState.SelectedInput;
            if (String.IsNullOrEmpty(sel))
                return "—";

            Single vol = OBSStudioForLogiPlugin.Instance?.GetInputVolume(sel) ?? 1.0f;
            return $"{(Int32)(vol * 100)}%\n{sel}";
        }

        public void OnConnected()
        {
            this.IsEnabled = true;
        }

        public void OnDisconnected()
        {
            this.IsEnabled = false;
        }

        public void OnInputVolumeChanged(String inputName)
        {
            if (inputName == AudioSelectionState.SelectedInput)
            {
                this.AdjustmentValueChanged();
            }
        }
    }
}
