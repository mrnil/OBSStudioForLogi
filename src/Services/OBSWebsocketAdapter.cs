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

        public void StartRecord()
        {
            this._obs?.StartRecord();
        }

        public void StopRecord()
        {
            this._obs?.StopRecord();
        }

        public void PauseRecord()
        {
            this._obs?.PauseRecord();
        }

        public void ResumeRecord()
        {
            this._obs?.ResumeRecord();
        }

        public String[] GetProfileList()
        {
            var profiles = this._obs?.GetProfileList()?.Profiles;
            return profiles?.ToArray() ?? new String[0];
        }

        public void SetCurrentProfile(String profileName)
        {
            this._obs?.SetCurrentProfile(profileName);
        }

        public String[] GetSceneCollectionList()
        {
            var collections = this._obs?.GetSceneCollectionList();
            return collections?.ToArray() ?? new String[0];
        }

        public void SetCurrentSceneCollection(String sceneCollectionName)
        {
            this._obs?.SetCurrentSceneCollection(sceneCollectionName);
        }
    }
}
