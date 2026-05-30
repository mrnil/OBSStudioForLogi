namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;

    public abstract class AudioInputDynamicFolderBase : PluginDynamicFolder, IInputMuteAwareCommand, IInputVolumeAwareCommand
    {
        protected String[] AudioInputs = new String[0];
        private readonly DoubleTapHelper _doubleTapHelper = new DoubleTapHelper();

        protected AudioInputDynamicFolderBase()
        {
        }

        public abstract void OnConnected();
        public abstract void OnDisconnected();

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override BitmapImage GetButtonImage(PluginImageSize imageSize)
        {
            return ButtonImageHelper.Icon("AudioMediaFolder.svg");
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return String.Empty;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
            Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
            
            Int32 volumePercent = (Int32)(volumeLevel * 100);
            String text = $"{actionParameter}\n\n{volumePercent}%";
            
            Boolean isSelected = AudioSelectionState.IsSelected(actionParameter);
            
            return ButtonImageHelper.StateTextWithBorder(text, imageSize, !isMuted, 
                BitmapColor.Green, BitmapColor.Red, isSelected);
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            this._doubleTapHelper.OnTap(actionParameter,
                onSingleTap: (input) =>
                {
                    OBSStudioForLogiPlugin.Instance?.ToggleInputMute(input);
                },
                onDoubleTap: (input) =>
                {
                    HandleDoubleTap(input);
                });
        }

        private void HandleDoubleTap(String inputName)
        {
            if (AudioSelectionState.IsSelected(inputName))
            {
                AudioSelectionState.Deselect();
            }
            else
            {
                var previousSelection = AudioSelectionState.SelectedInput;
                AudioSelectionState.Select(inputName);
                
                if (!String.IsNullOrEmpty(previousSelection))
                {
                    this.CommandImageChanged(previousSelection);
                }
            }
            
            this.CommandImageChanged(inputName);
        }

        public void OnInputMuteChanged(String inputName)
        {
            if (this.AudioInputs.Contains(inputName))
            {
                this.CommandImageChanged(inputName);
            }
        }

        public void OnInputVolumeChanged(String inputName)
        {
            if (this.AudioInputs.Contains(inputName))
            {
                this.CommandImageChanged(inputName);
            }
        }
    }
}
