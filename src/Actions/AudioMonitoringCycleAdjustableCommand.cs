namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class AudioMonitoringCycleAdjustableCommand : ActionEditorCommand, IObsCommand
    {
        private const String InputNameControlName = "InputName";

        public static AudioMonitoringCycleAdjustableCommand Instance { get; private set; }

        public AudioMonitoringCycleAdjustableCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Name = "AudioMonitoringCycleAdjustable";
            this.DisplayName = "Cycle Audio Monitoring (User defined)";
            this.GroupName = "99. User Defined Actions";
            this.Description = "Cycle audio monitoring type: None → Monitor Only → Monitor & Output";
            this.IsWidget = true;

            this.ActionEditor.AddControlEx(new ActionEditorTextbox(InputNameControlName, "Audio Source Name (required)"));
        }

        protected override Boolean OnLoad() => true;

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            if (!actionParameters.TryGetString(InputNameControlName, out var inputName) || String.IsNullOrEmpty(inputName))
            {
                PluginLog.Warning("AudioMonitoringCycleAdjustableCommand: Audio source name is required but not provided");
                return false;
            }

            Task.Run(() =>
            {
                try
                {
                    PluginLog.Info($"AudioMonitoringCycleAdjustableCommand: Cycling monitoring for '{inputName}'");
                    OBSStudioForLogiPlugin.Instance?.CycleInputAudioMonitorType(inputName);
                    this.ActionImageChanged();
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"AudioMonitoringCycleAdjustableCommand: Failed to cycle monitoring for '{inputName}': {ex.Message}");
                }
            });

            return true;
        }

        protected override String GetCommandDisplayName(ActionEditorActionParameters actionParameters)
        {
            return null;
        }

        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight)
        {
            if (!actionParameters.TryGetString(InputNameControlName, out var inputName) || String.IsNullOrEmpty(inputName))
                return null;

            String monitorType = OBSStudioForLogiPlugin.Instance?.GetInputAudioMonitorType(inputName) ?? "OBS_MONITORING_TYPE_NONE";
            String mode = "";

            switch (monitorType)
            {
                case "OBS_MONITORING_TYPE_NONE":
                    mode = "Monitor off";
                    break;
                case "OBS_MONITORING_TYPE_MONITOR_ONLY":
                    mode = "Monitor only";
                    break;
                case "OBS_MONITORING_TYPE_MONITOR_AND_OUTPUT":
                    mode = "Monitor & output";
                    break;
            }

            String text = $"{this.DisplayName}\n\n{mode}";
            Boolean isMonitoring = monitorType != "OBS_MONITORING_TYPE_NONE";

            return ButtonImageHelper.StateTextWithBorder(text, imageWidth, imageHeight, isMonitoring,
                BitmapColor.Green, BitmapColor.White, false);
        }

        public void OnConnected()
        {
        }

        public void OnDisconnected()
        {
        }
    }
}
