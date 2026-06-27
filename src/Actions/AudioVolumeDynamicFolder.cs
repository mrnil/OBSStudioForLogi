namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AudioVolumeDynamicFolder : PluginDynamicFolder, IObsCommand, IInputVolumeAwareCommand, IInputMuteAwareCommand, IInputsListAwareCommand
    {
        public static AudioVolumeDynamicFolder Instance { get; private set; }

        private String[] _audioInputs = new String[0];

        public AudioVolumeDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Audio Volume";
            this.GroupName = "8. Audio";
            this.Description = "Displays a folder of all audio soures. Tapping a source will arm the wheel, then turn to adjust volume. [MX Console / Loupedeck CT]";
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
            return this._audioInputs.Select(this.CreateAdjustmentName);
        }

        public override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            Single vol = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
            return $"{actionParameter}\n{VolumeConverter.FormatDb(vol)}";
        }

        public override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
            Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
            String text = $"{actionParameter}\n\n{VolumeConverter.FormatDb(volumeLevel)}";

            return ButtonTextRenderer.RenderText(text, imageSize, BitmapColor.Black, !isMuted ? BitmapColor.Green : BitmapColor.Red);
        }

        public override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            Single current = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
            Single target = Math.Clamp(current + diff * 0.01f, 0f, 20f);
            OBSStudioForLogiPlugin.Instance?.SetInputVolume(actionParameter, target);
            this.AdjustmentValueChanged(actionParameter);
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
            this.ButtonActionNamesChanged();
        }

        public void OnInputsChanged(String[] inputs)
        {
            this._audioInputs = inputs ?? new String[0];
            this.ButtonActionNamesChanged();
        }

        public void OnInputVolumeChanged(String inputName)
        {
            if (this._audioInputs.Contains(inputName))
            {
                this.AdjustmentValueChanged(inputName);
            }
        }

        public void OnInputMuteChanged(String inputName)
        {
            if (this._audioInputs.Contains(inputName))
            {
                this.AdjustmentValueChanged(inputName);
            }
        }
    }
}
