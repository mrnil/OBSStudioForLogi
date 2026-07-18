namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class AudioMuteAdjustableCommand : ActionEditorCommand, IObsCommand
    {
        private const String InputNameControlName = "InputName";

        public static AudioMuteAdjustableCommand Instance { get; private set; }

        public AudioMuteAdjustableCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Name = "AudioMuteAdjustable";
            this.DisplayName = "Toggle Audio Mute (User defined)";
            this.GroupName = "8. Audio###User Defined";
            this.Description = "Toggle mute/unmute for a specific audio source";

            this.ActionEditor.AddControlEx(new ActionEditorTextbox(InputNameControlName, "Audio Source Name (required)"));
        }

        protected override Boolean OnLoad() => true;

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            if (!actionParameters.TryGetString(InputNameControlName, out var inputName) || String.IsNullOrEmpty(inputName))
            {
                PluginLog.Warning("AudioMuteAdjustableCommand: Audio source name is required but not provided");
                return false;
            }

            Task.Run(() =>
            {
                try
                {
                    PluginLog.Info($"AudioMuteAdjustableCommand: Toggling mute for '{inputName}'");
                    OBSStudioForLogiPlugin.Instance?.ToggleInputMute(inputName);
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"AudioMuteAdjustableCommand: Failed to toggle mute for '{inputName}': {ex.Message}");
                }
            });

            return true;
        }

        public void OnConnected()
        {
        }

        public void OnDisconnected()
        {
        }
    }
}
