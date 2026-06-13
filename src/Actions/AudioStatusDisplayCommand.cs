namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class AudioStatusDisplayCommand : ActionEditorCommand, IObsCommand, IInputMuteAwareCommand, IInputVolumeAwareCommand
    {
        private const String InputNameControlName = "InputName";

        public static AudioStatusDisplayCommand Instance { get; private set; }

        public AudioStatusDisplayCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Name = "AudioStatusDisplay";
            this.DisplayName = "Audio Source Status (User defined)";
            this.GroupName = "99. User Defined Actions";
            this.Description = "Display mute state, volume, and monitoring mode for a specific audio source";
            this.IsWidget = true;

            this.ActionEditor.AddControlEx(new ActionEditorTextbox(InputNameControlName, "Audio Source Name (required)"));
        }

        protected override Boolean OnLoad() => true;

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
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

            return AudioHelpers.RenderAudioStateImage(inputName, imageWidth, imageHeight);
        }

        public void RefreshImage()
        {
            try { this.ActionImageChanged(); } catch { }
        }

        public void OnConnected()
        {
            try { this.ActionImageChanged(); } catch { }
        }

        public void OnDisconnected()
        {
            try { this.ActionImageChanged(); } catch { }
        }

        public void OnInputMuteChanged(String inputName)
        {
            try { this.ActionImageChanged(); } catch { }
        }

        public void OnInputVolumeChanged(String inputName)
        {
            try { this.ActionImageChanged(); } catch { }
        }
    }
}
