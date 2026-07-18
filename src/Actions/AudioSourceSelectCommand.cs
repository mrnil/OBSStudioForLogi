namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Linq;

    public class AudioSourceSelectCommand : PluginMultistateDynamicCommand, IObsCommand, IInputMuteAwareCommand, IInputsListAwareCommand
    {
        private const Int16 SOURCE_UNSELECTED = 0;
        private const Int16 SOURCE_SELECTED = 1;

        public static AudioSourceSelectCommand Instance { get; private set; }

        private String[] _audioInputs = new String[0];

        public AudioSourceSelectCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Description = "Select an audio source to control its mute state";
            this.GroupName = "8. Audio###Available Sources";
            this.AddState("", "Source unselected");
            this.AddState("", "Source selected");
        }

        protected override Boolean OnLoad()
        {
            this.IsEnabled = false;
            this.ResetParameters(false);
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.ToggleInputMute(actionParameter);
        }

        private void ResetParameters(Boolean readContent)
        {
            this.RemoveAllParameters();

            if (readContent)
            {
                foreach (String input in this._audioInputs)
                {
                    this.AddParameter(input, input, this.GroupName).Description = $"Toggle mute for \"{input}\"";
                    this.SetCurrentState(input, SOURCE_UNSELECTED);
                }
            }

            this.ParametersChanged();
            this.ActionImageChanged();
        }

        public void OnInputsChanged(String[] inputs)
        {
            this._audioInputs = inputs ?? new String[0];
            this.ResetParameters(this._audioInputs.Length > 0);
        }

        public void OnInputMuteChanged(String inputName)
        {
            if (this._audioInputs.Contains(inputName))
            {
                this.ActionImageChanged(inputName);
            }
        }

        public void OnConnected()
        {
            this.IsEnabled = true;
            this._audioInputs = OBSStudioForLogiPlugin.Instance?.GetInputList() ?? new String[0];
            this.ResetParameters(true);
        }

        public void OnDisconnected()
        {
            this.IsEnabled = false;
            this._audioInputs = new String[0];
            this.ResetParameters(false);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, Int32 stateIndex, PluginImageSize imageSize)
        {
            return AudioHelpers.RenderAudioStateImage(actionParameter, imageSize);
        }
    }
}
