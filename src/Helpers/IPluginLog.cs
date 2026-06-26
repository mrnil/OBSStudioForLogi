namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public interface IPluginLog
    {
        void Debug(String message);
        void Info(String message);
        void Warning(String message);
        void Error(String message);
    }
}
