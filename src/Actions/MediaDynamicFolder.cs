namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

            var state = OBSStudioForLogiPlugin.Instance?.GetMediaInputStatus(actionParameter) ?? "OBS_MEDIA_STATE_NONE";
            String stateLabel;
            BitmapColor color;

            switch (state)
            {
                case "OBS_MEDIA_STATE_PLAYING":
                    stateLabel = "Playing";
                    color = new BitmapColor(80, 255, 80);
                    break;
                case "OBS_MEDIA_STATE_PAUSED":
                    stateLabel = "Paused";
                    color = new BitmapColor(255, 200, 0);
                    break;
                case "OBS_MEDIA_STATE_STOPPED":
                    stateLabel = "Stopped";
                    color = new BitmapColor(128, 128, 128);
                    break;
                case "OBS_MEDIA_STATE_ENDED":
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

            this._doubleTapHelper.OnTap(actionParameter,
                onSingleTap: (input) =>
                {
                    var state = OBSStudioForLogiPlugin.Instance?.GetMediaInputStatus(input) ?? "OBS_MEDIA_STATE_NONE";
                    if (state == "OBS_MEDIA_STATE_PLAYING")
                    {
                        OBSStudioForLogiPlugin.Instance?.TriggerMediaInputAction(input, "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PAUSE");
                    }
                    else if (state == "OBS_MEDIA_STATE_PAUSED")
                    {
                        OBSStudioForLogiPlugin.Instance?.TriggerMediaInputAction(input, "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY");
                    }
                    else
                    {
                        OBSStudioForLogiPlugin.Instance?.TriggerMediaInputAction(input, "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART");
                    }
                    this.RefreshAfterDelay(input);
                },
                onDoubleTap: (input) =>
                {
                    OBSStudioForLogiPlugin.Instance?.TriggerMediaInputAction(input, "OBS_WEBSOCKET_MEDIA_INPUT_ACTION_STOP");
                    this.RefreshAfterDelay(input);
                });
        }

        private void RefreshAfterDelay(String inputName)
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(200);
                this.CommandImageChanged(inputName);
            });
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

        public void OnConnected()
        {
            this._mediaInputs = OBSStudioForLogiPlugin.Instance?.GetMediaInputList() ?? new String[0];
            PluginLog.Debug($"MediaDynamicFolder: Loaded {this._mediaInputs.Length} media inputs");
            this.ButtonActionNamesChanged();
        }

        public void OnDisconnected()
        {
            this._mediaInputs = new String[0];
            this.ButtonActionNamesChanged();
        }
    }
}
