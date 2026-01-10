namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using OBSWebsocketDotNet;

    public class OBSWebsocketAdapter : IOBSWebsocket
    {
        private readonly OBSWebsocket _obs;

        public OBSWebsocketAdapter(OBSWebsocket obs)
        {
            this._obs = obs;
        }

        public Boolean IsConnected => this._obs?.IsConnected ?? false;

        public void SetCurrentProgramScene(String sceneName)
        {
            this._obs?.SetCurrentProgramScene(sceneName);
        }

        public void ToggleRecord()
        {
            this._obs?.ToggleRecord();
        }
    }
}
