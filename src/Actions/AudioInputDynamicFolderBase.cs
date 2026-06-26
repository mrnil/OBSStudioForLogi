namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;

    public abstract class AudioInputDynamicFolderBase : PluginDynamicFolder, IInputMuteAwareCommand, IInputVolumeAwareCommand, IInputMonitorAwareCommand
    {
        protected String[] AudioInputs = new String[0];
        private readonly DoubleTapHelper _doubleTapHelper = new DoubleTapHelper();

        protected AudioInputDynamicFolderBase()
        {
            AudioSelectionState.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnSelectionChanged(String previousInput, String newInput)
        {
            if (!String.IsNullOrEmpty(previousInput) && this.AudioInputs.Contains(previousInput))
            {
                this.CommandImageChanged(previousInput);
            }

            if (!String.IsNullOrEmpty(newInput) && this.AudioInputs.Contains(newInput))
            {
                this.CommandImageChanged(newInput);
            }
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
            PluginLog.Trace($"[AudioFolder] GetCommandDisplayName called - parameter: '{actionParameter}', imageSize: {imageSize}");
            return String.Empty;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            PluginLog.Trace($"[AudioFolder] GetCommandImage action parameter: '{actionParameter}'");

            if (actionParameter == "cycle-monitor")
            {
                String text = "Cycle Monitor";
                return ButtonTextRenderer.RenderTextWithBorder(text, imageSize, BitmapColor.White, false);
            }

            return AudioHelpers.RenderAudioStateImage(actionParameter, imageSize);
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            PluginLog.Debug($"[AudioFolder] RunCommand called - parameter: '{actionParameter}'");

            // Encoder press sends the command name from GetEncoderPressActionNames
            if (actionParameter == "cycle-monitor")
            {
                String selected = AudioSelectionState.SelectedInput;
                PluginLog.Debug($"[AudioFolder] Encoder press: cycle-monitor, selected: '{selected ?? "(none)"}'");
                if (!String.IsNullOrEmpty(selected))
                {
                    OBSStudioForLogiPlugin.Instance?.CycleInputAudioMonitorType(selected);
                }
                else
                {
                    PluginLog.Warning("[AudioFolder] Cannot cycle monitoring - no source selected");
                }
                return;
            }

            // Button tap/double-tap for audio input buttons
            this._doubleTapHelper.OnTap(actionParameter,
                onSingleTap: (input) =>
                {
                    this.HandleSingleTap(input);
                },
                onDoubleTap: (input) =>
                {
                    OBSStudioForLogiPlugin.Instance?.ToggleInputMute(input);
                });
        }


        private void HandleSingleTap(String inputName)
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

        public void OnInputMonitorTypeChanged(String inputName)
        {
            if (this.AudioInputs.Contains(inputName))
            {
                this.CommandImageChanged(inputName);
            }
        }

        public override IEnumerable<String> GetEncoderRotateActionNames(DeviceType deviceType)
        {
            PluginLog.Trace($"[AudioFolder] GetEncoderRotateActionNames called for device type: {deviceType}");
            return new[] { this.CreateAdjustmentName("volume-adjust") };
        }

        public override IEnumerable<String> GetEncoderPressActionNames(DeviceType deviceType)
        {
            PluginLog.Trace($"[AudioFolder] GetEncoderPressActionNames called for device type: {deviceType}");
            return new[] { this.CreateCommandName("cycle-monitor") };
        }

        public override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            PluginLog.Debug($"[AudioFolder] ApplyAdjustment called - parameter: '{actionParameter}', diff: {diff}");

            String selected = AudioSelectionState.SelectedInput;
            PluginLog.Debug($"[AudioFolder] Currently selected input: '{selected ?? "(none)"}'");

            if (String.IsNullOrEmpty(selected))
            {
                PluginLog.Warning("[AudioFolder] Cannot adjust volume - no source selected");
                return;
            }

            Single current = OBSStudioForLogiPlugin.Instance?.GetInputVolume(selected) ?? 1.0f;
            Single step = diff * 0.01f;
            Single target = Math.Clamp(current + step, 0.0f, 20.0f);
            OBSStudioForLogiPlugin.Instance?.SetInputVolume(selected, target);
            PluginLog.Debug($"[AudioFolder] Adjusting volume for '{selected}': {(Int32)(current * 100)}% -> {(Int32)(target * 100)}%");
            this.AdjustmentValueChanged(actionParameter);
        }

        public override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            // For encoder display (volume-adjust), show selected input info
            if (actionParameter == "volume-adjust")
            {
                String selected = AudioSelectionState.SelectedInput;
                if (String.IsNullOrEmpty(selected))
                {
                    return "No source\nselected";
                }

                var volume = OBSStudioForLogiPlugin.Instance?.GetInputVolume(selected) ?? 1.0f;
                var volumePercent = (Int32)(volume * 100);
                String boostIndicator = volume > 1.0f ? "+" : "";

                return $"{boostIndicator}{volumePercent}%\n{selected}";
            }

            // For button adjustments, return empty - image handles display
            return String.Empty;
        }

        public override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize)
        {
            if (actionParameter == "volume-adjust")
            {
                return null;
            }

            Boolean isMuted = OBSStudioForLogiPlugin.Instance?.GetInputMute(actionParameter) ?? false;
            Single volumeLevel = OBSStudioForLogiPlugin.Instance?.GetInputVolume(actionParameter) ?? 1.0f;

            Int32 volumePercent = (Int32)(volumeLevel * 100);
            String text = $"{actionParameter}\n\n{volumePercent}%";

            Boolean isSelected = AudioSelectionState.IsSelected(actionParameter);

            return ButtonTextRenderer.RenderTextWithBorder(text, imageSize, !isMuted ? BitmapColor.Green : BitmapColor.Red, isSelected);
        }

    }
}
