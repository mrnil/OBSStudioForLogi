namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class AudioMuteAdjustableCommand : ActionEditorCommand, IObsCommand, IInputMuteAwareCommand
    {
        private const String InputNameControlName = "InputName";

        public static AudioMuteAdjustableCommand Instance { get; private set; }

        public AudioMuteAdjustableCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Name = "AudioMuteAdjustable";
            this.DisplayName = "Toggle Audio Mute (User defined)";
            this.GroupName = "99. User Defined Actions";
            this.Description = "Toggle mute/unmute for a specific audio source";
            this.IsWidget = true;

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

        protected override String GetCommandDisplayName(ActionEditorActionParameters actionParameters)
        {
            return null;
        }

        protected override BitmapImage GetCommandImage(ActionEditorActionParameters actionParameters, Int32 imageWidth, Int32 imageHeight)
        {
            if (!actionParameters.TryGetString(InputNameControlName, out var inputName) || String.IsNullOrEmpty(inputName))
                return null;

            Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(inputName) ?? false;
            Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(inputName) ?? 1.0f;
            Int32 volumePercent = (Int32)(volumeLevel * 100);
            String text = $"{this.DisplayName}\n\n{(isMuted ? "Muted" : "Unmuted")}\n{volumePercent}%";

            return ButtonImageHelper.StateTextWithBorder(text, imageWidth, imageHeight, !isMuted,
                BitmapColor.Green, BitmapColor.Red, false);
        }

        public void OnConnected()
        {
        }

        public void OnDisconnected()
        {
        }

        public void OnInputMuteChanged(String inputName)
        {
            this.ActionImageChanged();
        }
    }
}
