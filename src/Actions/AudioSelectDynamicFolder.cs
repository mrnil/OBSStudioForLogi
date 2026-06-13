namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AudioSelectDynamicFolder : PluginDynamicFolder, IObsCommand, IInputMuteAwareCommand, IInputVolumeAwareCommand, IInputsListAwareCommand
    {
        public static AudioSelectDynamicFolder Instance { get; private set; }

        private String[] _audioInputs = new String[0];

        public AudioSelectDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Audio Select";
            this.GroupName = "8. Audio";
            this.Description = "Select an audio source for global use by other audio actions";
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override BitmapImage GetButtonImage(PluginImageSize imageSize)
        {
            return ButtonImageHelper.Icon("AudioMediaFolder.svg");
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            return this._audioInputs.Select(this.CreateCommandName);
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            if (AudioSelectionState.IsSelected(actionParameter))
            {
                AudioSelectionState.Deselect();
            }
            else
            {
                var previousSelection = AudioSelectionState.SelectedInput;
                AudioSelectionState.Select(actionParameter);

                if (!String.IsNullOrEmpty(previousSelection))
                {
                    this.CommandImageChanged(previousSelection);
                }
            }

            this.CommandImageChanged(actionParameter);
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
            Boolean isSelected = AudioSelectionState.IsSelected(actionParameter);
            Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
            Int32 volumePercent = (Int32)(volumeLevel * 100);
            String monitorType = OBSStudioForLogiPlugin.Instance?.GetInputAudioMonitorType(actionParameter) ?? "Unknown";
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

            String text = $"{actionParameter}\n\n{volumePercent}%\n\n{mode}";

            return ButtonTextRenderer.RenderTextWithBorder(text, imageSize, !isMuted ? BitmapColor.Green : BitmapColor.Red, isSelected);
        }

        public void OnConnected()
        {
            String[] inputs = OBSStudioForLogiPlugin.Instance?.GetInputList() ?? new String[0];
            this._audioInputs = inputs;
            this.ButtonActionNamesChanged();
        }

        public void OnDisconnected()
        {
            this._audioInputs = new String[0];
            AudioSelectionState.Deselect();
            this.ButtonActionNamesChanged();
        }

        public void OnInputsChanged(String[] inputs)
        {
            String previousSelection = AudioSelectionState.SelectedInput;
            this._audioInputs = inputs ?? new String[0];

            if (!String.IsNullOrEmpty(previousSelection) && !this._audioInputs.Contains(previousSelection))
            {
                AudioSelectionState.Deselect();
            }

            this.ButtonActionNamesChanged();
        }

        public void OnInputMuteChanged(String inputName)
        {
            if (this._audioInputs.Contains(inputName))
            {
                this.CommandImageChanged(inputName);
            }
        }

        public void OnInputVolumeChanged(String inputName)
        {
            if (this._audioInputs.Contains(inputName))
            {
                this.CommandImageChanged(inputName);
            }
        }
    }
}
