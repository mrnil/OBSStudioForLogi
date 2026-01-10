namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public interface IOBSWebsocket
    {
        Boolean IsConnected { get; }
        void SetCurrentProgramScene(String sceneName);
        void StartRecord();
        void StopRecord();
        void ToggleRecord();
        void PauseRecord();
        void ResumeRecord();
        String[] GetProfileList();
        void SetCurrentProfile(String profileName);
        String[] GetSceneCollectionList();
        void SetCurrentSceneCollection(String sceneCollectionName);
        String[] GetSceneList();
    }
}
