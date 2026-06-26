namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    internal static class PluginLog
    {
        private static PluginLogFile _pluginLogFile;

#if DEBUG
        public static LogLevel CurrentLevel { get; set; } = LogLevel.Debug;
#else
        public static LogLevel CurrentLevel { get; set; } = LogLevel.Info;
#endif

        public static void Init(PluginLogFile pluginLogFile)
        {
            pluginLogFile.CheckNullArgument(nameof(pluginLogFile));
            PluginLog._pluginLogFile = pluginLogFile;
        }

        public static void Trace(String text)
        {
            if (CurrentLevel <= LogLevel.Trace)
                PluginLog._pluginLogFile?.Verbose($"[TRACE] {text}");
        }

        public static void Debug(String text)
        {
            if (CurrentLevel <= LogLevel.Debug)
                PluginLog._pluginLogFile?.Verbose($"[DEBUG] {text}");
        }

        public static void Verbose(String text) => PluginLog._pluginLogFile?.Verbose(text);

        public static void Verbose(Exception ex, String text) => PluginLog._pluginLogFile?.Verbose(ex, text);

        public static void Info(String text)
        {
            if (CurrentLevel <= LogLevel.Info)
                PluginLog._pluginLogFile?.Info(text);
        }

        public static void Info(Exception ex, String text)
        {
            if (CurrentLevel <= LogLevel.Info)
                PluginLog._pluginLogFile?.Info(ex, text);
        }

        public static void Warning(String text)
        {
            if (CurrentLevel <= LogLevel.Warning)
                PluginLog._pluginLogFile?.Warning(text);
        }

        public static void Warning(Exception ex, String text)
        {
            if (CurrentLevel <= LogLevel.Warning)
                PluginLog._pluginLogFile?.Warning(ex, text);
        }

        public static void Error(String text) => PluginLog._pluginLogFile?.Error(text);

        public static void Error(Exception ex, String text) => PluginLog._pluginLogFile?.Error(ex, text);
    }

    public class PluginLogAdapter : IPluginLog
    {
        public void Debug(String message) => PluginLog.Debug(message);
        public void Info(String message) => PluginLog.Info(message);
        public void Warning(String message) => PluginLog.Warning(message);
        public void Error(String message) => PluginLog.Error(message);
    }
}
