namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class MediaDynamicFolder : PluginDynamicFolder, IObsCommand, IInputsListAwareCommand
    {
        public static MediaDynamicFolder Instance { get; private set; }

        private String[] _mediaInputs = new String[0];
        private readonly DoubleTapHelper _doubleTapHelper = new DoubleTapHelper();

        public MediaDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "Media Controls";
            this.GroupName = "9. Media";
            this.Description = "Folder of media sources with play/pause/stop controls";
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
            return this._mediaInputs.Select(this.CreateCommandName);
        }

        public override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            return String.Empty;
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return null;

            String state = OBSStudioForLogiPlugin.Instance?.GetMediaInputStatus(actionParameter) ?? MediaInputStates.None;
            String stateLabel;
            BitmapColor color;

            switch (state)
            {
                case MediaInputStates.Playing:
                    stateLabel = "Playing";
                    color = new BitmapColor(80, 255, 80);
                    break;
                case MediaInputStates.Paused:
                    stateLabel = "Paused";
                    color = new BitmapColor(255, 200, 0);
                    break;
                case MediaInputStates.Stopped:
                    stateLabel = "Stopped";
                    color = new BitmapColor(128, 128, 128);
                    break;
                case MediaInputStates.Ended:
                    stateLabel = "Ended";
                    color = new BitmapColor(128, 128, 128);
                    break;
                default:
                    stateLabel = "Idle";
                    color = new BitmapColor(128, 128, 128);
                    break;
            }

            String text = $"{actionParameter}\n\n{stateLabel}";
            return ButtonTextRenderer.RenderText(text, imageSize, BitmapColor.Black, color);
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            // No redraw here: OBS answers every media action with MediaInputActionTriggered,
            // which updates the media state cache and redraws the button.
            this._doubleTapHelper.OnTap(actionParameter,
                onSingleTap: (input) => OBSStudioForLogiPlugin.Instance?.ToggleMediaInputPlayback(input),
                onDoubleTap: (input) => OBSStudioForLogiPlugin.Instance?.TriggerMediaInputAction(input, MediaInputStates.ActionStop));
        }

        public void OnMediaStateChanged(String inputName)
        {
            if (this._mediaInputs.Contains(inputName))
            {
                this.CommandImageChanged(inputName);
            }
        }

        public void OnInputsChanged(String[] inputs)
        {
            this._mediaInputs = OBSStudioForLogiPlugin.Instance?.GetMediaInputList() ?? new String[0];
            PluginLog.Debug($"MediaDynamicFolder: Input list changed, reloaded {this._mediaInputs.Length} media inputs");
            this.ButtonActionNamesChanged();
        }

        // Media inputs are reloaded in OnInputsChanged, which the per-connection state load triggers.
        public void OnConnected()
        {
        }

        public void OnDisconnected()
        {
            this._mediaInputs = new String[0];
            this.ButtonActionNamesChanged();
        }
    }
}
