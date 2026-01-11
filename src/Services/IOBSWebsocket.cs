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
        void StartStream();
        void StopStream();
        void ToggleStream();
        String[] GetProfileList();
        void SetCurrentProfile(String profileName);
        String[] GetSceneCollectionList();
        void SetCurrentSceneCollection(String sceneCollectionName);
        String[] GetSceneList();
        void SaveSourceScreenshot(String sourceName, String imageFormat, String imageFilePath, Int32 imageWidth, Int32 imageHeight);
        String[] GetSceneItemList(String sceneName);
        Boolean GetSceneItemEnabled(String sceneName, String sourceName);
        void SetSceneItemEnabled(String sceneName, String sourceName, Boolean enabled);
    }
}
