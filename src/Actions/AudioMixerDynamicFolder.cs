namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AudioMixerDynamicFolder : AudioInputDynamicFolderBase, IObsCommand, IInputsListAwareCommand
    {
        public static AudioMixerDynamicFolder Instance { get; private set; }

        public AudioMixerDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Mixer for All Audio";
            this.GroupName = "8. Audio";
            this.Description = "Folder of all audio inputs with controls for mute, monitor and volume [Volume - Loupedeck only]";
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType) => this.AudioInputs.Select(this.CreateCommandName);

        public void OnInputsChanged(String[] inputs) => this.UpdateInputs(inputs);

        private void UpdateInputs(String[] inputs)
        {
            String previousSelection = AudioSelectionState.SelectedInput;
            
            this.AudioInputs = inputs ?? new String[0];


            // Deselect if selected input is no longer in the list
            if (!String.IsNullOrEmpty(previousSelection) && !this.AudioInputs.Contains(previousSelection))
            {
                AudioSelectionState.Deselect();
            }
            
            PluginLog.Debug($"AudioMixerDynamicFolder updated with {this.AudioInputs.Length} inputs");

            this.ButtonActionNamesChanged();
        }

        // Inputs arrive via OnInputsChanged from OBSWebSocketManager's per-connection state load.
        public override void OnConnected()
        {
        }

        public override void OnDisconnected()
        {
            this.AudioInputs = new String[0];
            AudioSelectionState.Deselect();
            this.ButtonActionNamesChanged();
        }
    }
}
