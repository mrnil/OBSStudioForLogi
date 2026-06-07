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

            // Encoder press sends the command name from GetEncoderPressActionNames
            if (actionParameter == "cycle-monitor")
            {
                var selected = AudioSelectionState.SelectedInput;
                if (!String.IsNullOrEmpty(selected))
                {
                    PluginLog.Info($"Cycling audio monitoring for '{selected}'");
                    OBSStudioForLogiPlugin.Instance?.CycleInputAudioMonitorType(selected);
                }
                else
                {
                    PluginLog.Warning("Cannot cycle monitoring - no source selected");
                }
                return;
            }

            // Button tap/double-tap for audio input buttons
            this._doubleTapHelper.OnTap(actionParameter,
                onSingleTap: (input) =>
                {
                    HandleSingleTap(input);
                },
                onDoubleTap: (input) =>
                {
                    OBSStudioForLogiPlugin.Instance?.ToggleInputMute(input);
                });
        }

        private void HandleSingleTap(String inputName)
        {
            PluginLog.Info($"Audio source '{inputName}' selected");
            
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

        public override IEnumerable<String> GetEncoderRotateActionNames(DeviceType deviceType)
        {
            return new[] { this.CreateAdjustmentName("volume-adjust") };
        }
       
        public override IEnumerable<String> GetEncoderPressActionNames(DeviceType deviceType)
        {
            return new[] { this.CreateCommandName("cycle-monitor") };
        }

        public override void ApplyAdjustment(String actionParameter, Int32 diff)
        {

            Single current = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
            Single step = diff * 0.01f;
            Single target = Math.Clamp(current + step, 0.0f, 20.0f);
            OBSStudioForLogiPlugin.Instance?.SetInputVolume(actionParameter, target);
            PluginLog.Info($"Volume adjusted for '{actionParameter}': {(Int32)(target * 100)}%");
            this.AdjustmentValueChanged(actionParameter);
        }

        public override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize)
        {
 //           var selectedInput = AudioSelectionState.SelectedInput;
            
            if (String.IsNullOrEmpty(actionParameter))
            {
                return "No source\nselected";
            }
            
            var volume = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;
            var volumePercent = (Int32)(volume * 100);
            String boostIndicator = volume > 1.0f ? "+" : "";

            return $"{boostIndicator}{volumePercent}%\n{actionParameter}";
        }



    }
}
