namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AudioSelectDynamicFolder : AudioInputDynamicFolderBase, IObsCommand, IInputsListAwareCommand
    {
        public static AudioSelectDynamicFolder Instance { get; private set; }

        public AudioSelectDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Audio Select";
            this.GroupName = "8. Audio";
            this.Description = "Select an audio source for global use by other audio actions";
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            return this.AudioInputs.Select(this.CreateCommandName);
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

        // No encoder support — selection only
        public override IEnumerable<String> GetEncoderRotateActionNames(DeviceType deviceType) => Array.Empty<String>();
        public override IEnumerable<String> GetEncoderPressActionNames(DeviceType deviceType) => Array.Empty<String>();

        public void OnInputsChanged(String[] inputs)
        {
            String previousSelection = AudioSelectionState.SelectedInput;
            this.AudioInputs = inputs ?? new String[0];

            if (!String.IsNullOrEmpty(previousSelection) && !this.AudioInputs.Contains(previousSelection))
            {
                AudioSelectionState.Deselect();
            }

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
