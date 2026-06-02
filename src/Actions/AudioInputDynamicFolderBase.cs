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
            PluginLog.Info($"AudioInputDynamicFolderBase: RunCommand called with actionParameter='{actionParameter}'");
            
            if (String.IsNullOrEmpty(actionParameter))
                return;

            // Encoder press sends the command name from GetEncoderPressActionNames
            if (actionParameter == "Cycle Monitor")
            {
                var selected = AudioSelectionState.SelectedInput;
                if (!String.IsNullOrEmpty(selected))
                {
                    var currentType = OBSStudioForLogiPlugin.Instance?.GetInputAudioMonitorType(selected) ?? "OBS_MONITORING_TYPE_NONE";
                    PluginLog.Info($"AudioInputDynamicFolderBase: Encoder press - cycling monitoring for '{selected}' from {currentType}");
                    OBSStudioForLogiPlugin.Instance?.CycleInputAudioMonitorType(selected);
                }
                else
                {
                    PluginLog.Warning("AudioInputDynamicFolderBase: Encoder press - no source selected");
                }
                return;
            }

            // Button tap/double-tap for audio input buttons
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
            PluginLog.Info($"AudioInputDynamicFolderBase: Double-tap detected on '{inputName}'");
            
            if (AudioSelectionState.IsSelected(inputName))
            {
                PluginLog.Info($"AudioInputDynamicFolderBase: '{inputName}' is already selected, deselecting");
                AudioSelectionState.Deselect();
            }
            else
            {
                var previousSelection = AudioSelectionState.SelectedInput;
                AudioSelectionState.Select(inputName);
                
                if (!String.IsNullOrEmpty(previousSelection))
                {
                    PluginLog.Info($"AudioInputDynamicFolderBase: Updating previous selection '{previousSelection}' button image");
                    this.CommandImageChanged(previousSelection);
                }
            }
            
            PluginLog.Info($"AudioInputDynamicFolderBase: Updating '{inputName}' button image");
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
            PluginLog.Info($"AudioInputDynamicFolderBase: Encoder Rotated {deviceType}");
            return new[] { this.CreateAdjustmentName("volume-adjust") };
        }
       
        public override IEnumerable<String> GetEncoderPressActionNames(DeviceType deviceType)
        {
            return new[] { this.CreateCommandName("Cycle Monitor") };
        }

        public override IEnumerable<String> GetWheelToolNames(DeviceType deviceType)
        {
            PluginLog.Info($"AudioInputDynamicFolderBase: Wheel Tool Names {deviceType}");
            return new[] { this.CreateAdjustmentName("volume-adjust") };
        }

        public override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            var selected = AudioSelectionState.SelectedInput;
            if (!String.IsNullOrEmpty(selected))
            {
                Single current = OBSStudioForLogiPlugin.Instance?.GetInputVolume(selected) ?? 1.0f;
                Single step = diff * 0.01f; // 1% per click
                Single target = Math.Clamp(current + step, 0.0f, 20.0f); // OBS supports 0.0-20.0 (0-2000%)
                OBSStudioForLogiPlugin.Instance?.SetInputVolume(selected, target);
                PluginLog.Info($"AudioInputDynamicFolderBase: Apply Adjustment {selected} - {target} ({(Int32)(target * 100)}%)");
                this.AdjustmentValueChanged(actionParameter);
            }
        }

        public override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            var selectedInput = AudioSelectionState.SelectedInput;
            
            if (String.IsNullOrEmpty(selectedInput))
            {
                return "No source\nselected";
            }
            
            var volume = OBSStudioForLogiPlugin.Instance?.GetInputVolume(selectedInput) ?? 1.0f;
            var volumePercent = (Int32)(volume * 100);
            
            // Show boost indicator for volumes > 100%
            String boostIndicator = volume > 1.0f ? "+" : "";
            
            return $"{boostIndicator}{volumePercent}%\n{selectedInput}";
        }



    }
}
