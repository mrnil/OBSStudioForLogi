namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class MediaActionCommand : ActionEditorCommand, IObsCommand
    {
        private const String SourceNameControlName = "SourceName";
        private const String ActionControlName = "Action";

        public static MediaActionCommand Instance { get; private set; }

        public MediaActionCommand()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.Name = "MediaAction";
            this.DisplayName = "Media Action (User Defined)";
            this.GroupName = "9. Media";
            this.Description = "Trigger a media action (Play, Pause, Stop, Restart, Next, Previous) on a named media source";

            this.ActionEditor.AddControlEx(new ActionEditorTextbox(SourceNameControlName, "Media Source Name").SetRequired());

            var actionListbox = new ActionEditorListbox(ActionControlName, "Action");
            this.ActionEditor.AddControlEx(actionListbox);

            this.ActionEditor.ListboxItemsRequested += this.OnListboxItemsRequested;
        }

        private void OnListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
        {
            if (e.ControlName.EqualsNoCase(ActionControlName))
            {
                e.AddItem("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PLAY", "Play", "Start or resume playback");
                e.AddItem("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PAUSE", "Pause", "Pause playback at current position");
                e.AddItem("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_STOP", "Stop", "Stop and reset to beginning");
                e.AddItem("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_RESTART", "Restart", "Restart from beginning");
                e.AddItem("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_NEXT", "Next", "Next item (VLC playlist)");
                e.AddItem("OBS_WEBSOCKET_MEDIA_INPUT_ACTION_PREVIOUS", "Previous", "Previous item (VLC playlist)");
            }
        }

        protected override Boolean OnLoad() => true;

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            if (!actionParameters.TryGetString(SourceNameControlName, out var sourceName) || String.IsNullOrEmpty(sourceName))
            {
                PluginLog.Warning("MediaActionCommand: Source name is required but not provided");
                return false;
            }

            if (!actionParameters.TryGetString(ActionControlName, out var action) || String.IsNullOrEmpty(action))
            {
                PluginLog.Warning("MediaActionCommand: Action is required but not provided");
                return false;
            }

            PluginLog.Info($"MediaActionCommand: Triggering '{action}' on '{sourceName}'");
            OBSStudioForLogiPlugin.Instance?.TriggerMediaInputAction(sourceName.Trim(), action);
            return true;
        }

        public void OnConnected() { }
        public void OnDisconnected() { }
    }
}
