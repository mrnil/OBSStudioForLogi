namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public interface IOBSWebsocket
    {
        Boolean IsConnected { get; }
        void SetCurrentProgramScene(String sceneName);
        void ToggleRecord();
    }
}
