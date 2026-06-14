namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public interface IOBSWebsocket
    {
        Boolean IsConnected { get; }
        void SetCurrentProgramScene(String sceneName);
        void SetCurrentPreviewScene(String sceneName);
        void StartRecord();
        void StopRecord();
        void ToggleRecord();
        void PauseRecord();
        void ResumeRecord();
        void StartStream();
        void StopStream();
        void ToggleStream();
        void StartVirtualCam();
        void StopVirtualCam();
        void ToggleVirtualCam();
        void StartReplayBuffer();
        void StopReplayBuffer();
        void ToggleReplayBuffer();
        void SaveReplayBuffer();
        String[] GetProfileList();
        void SetCurrentProfile(String profileName);
        String[] GetSceneCollectionList();
        void SetCurrentSceneCollection(String sceneCollectionName);
        String[] GetSceneList();
        void SaveSourceScreenshot(String sourceName, String imageFormat, String imageFilePath, Int32 imageWidth, Int32 imageHeight);
        String[] GetSceneItemList(String sceneName);
        Boolean GetSceneItemEnabled(String sceneName, String sourceName);
        void SetSceneItemEnabled(String sceneName, String sourceName, Boolean enabled);
        String[] GetInputList();
        String GetInputKind(String inputName);
        Boolean GetInputMute(String inputName);
        void ToggleInputMute(String inputName);
        void SetInputMute(String inputName, Boolean muted);
        Single GetInputVolume(String inputName);
        void SetInputVolume(String inputName, Single volumeMul);
        String GetInputAudioMonitorType(String inputName);
        void SetInputAudioMonitorType(String inputName, String monitorType);
        String[] GetAudioSourcesInScene(String sceneName);
        String[] GetAudioInputsNotInAnyScene();
        String[] GetScenesForInput(String inputName);
        Boolean GetStudioModeEnabled();
        void SetStudioModeEnabled(Boolean enabled);
        void TriggerStudioModeTransition();
        Models.OBSStats GetStats();
    }
}
