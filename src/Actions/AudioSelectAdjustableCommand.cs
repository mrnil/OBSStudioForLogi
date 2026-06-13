namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class AudioSelectAdjustableCommand : ActionEditorCommand, IObsCommand
    {
        private const String InputNameControlName = "InputName";

        public static AudioSelectAdjustableCommand Instance { get; private set; }

        public AudioSelectAdjustableCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Name = "AudioSelectAdjustable";
            this.DisplayName = "Select Audio Source (User defined)";
            this.GroupName = "99. User Defined Actions";
            this.Description = "Toggle selection of a specific audio source as the globally selected source";
            this.IsWidget = true;

            this.ActionEditor.AddControlEx(new ActionEditorTextbox(InputNameControlName, "Audio Source Name (required)"));
        }

        protected override Boolean OnLoad() => true;

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            if (!actionParameters.TryGetString(InputNameControlName, out var inputName) || String.IsNullOrEmpty(inputName))
            {
                PluginLog.Warning("AudioSelectAdjustableCommand: Audio source name is required but not provided");
                return false;
            }

            if (AudioSelectionState.IsSelected(inputName))
            {
                PluginLog.Info($"AudioSelectAdjustableCommand: Deselecting '{inputName}'");
                AudioSelectionState.Deselect();
            }
            else
            {
                PluginLog.Info($"AudioSelectAdjustableCommand: Selecting '{inputName}'");
                AudioSelectionState.Select(inputName);
            }

            this.ActionImageChanged();
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

            Boolean isSelected = AudioSelectionState.IsSelected(inputName);
            String text = $"{this.DisplayName}\n\n{(isSelected ? "Selected" : "Not selected")}";

            return ButtonImageHelper.StateTextWithBorder(text, imageWidth, imageHeight, isSelected,
                BitmapColor.White, new BitmapColor(128, 128, 128), isSelected);
        }

        public void OnConnected()
        {
        }

        public void OnDisconnected()
        {
        }
    }
}
