namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public interface IPluginLog
    {
        void Info(String message);
        void Warning(String message);
        void Error(String message);
    }
}
