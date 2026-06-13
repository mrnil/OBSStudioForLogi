namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using Loupedeck.OBSStudioForLogiPlugin.Models;
    using Loupedeck.OBSStudioForLogiPlugin.Services;

    public class ConnectionConfigureCommand : ActionEditorCommand
    {
        private const String UseLocalObsControlName = "UseLocalObs";
        private const String IpAddressControlName = "IpAddress";
        private const String PortControlName = "Port";
        private const String PasswordControlName = "Password";

        public static ConnectionConfigureCommand Instance { get; private set; }

        public ConnectionConfigureCommand()
        {
            Instance = this;
            this.Name = "ConnectionConfigure";
            this.DisplayName = "Configure OBS Connection";
            this.GroupName = "1. OBS";
            this.Description = "Configure connection to local or remote OBS Studio. Press to save and reconnect.";

            var localSettings = OBSStudioForLogiPlugin.Instance?.GetLocalOBSSettings();
            var defaultIp = localSettings?.IpAddress ?? "127.0.0.1";
            var defaultPort = localSettings?.Port.ToString() ?? "4455";

            this.ActionEditor.AddControlEx(new ActionEditorTextbox(UseLocalObsControlName, "Use Local OBS (true/false)").SetRequired());
            this.ActionEditor.AddControlEx(new ActionEditorTextbox(IpAddressControlName, $"IP Address (local: {defaultIp})").SetRequired());
            this.ActionEditor.AddControlEx(new ActionEditorTextbox(PortControlName, $"Port (local: {defaultPort})").SetRequired());
            this.ActionEditor.AddControlEx(new ActionEditorTextbox(PasswordControlName, "Password"));

            PluginLog.Info("ConnectionConfigureCommand: Initialized");
        }

        protected override Boolean OnLoad() => true;

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            PluginLog.Info("ConnectionConfigureCommand: RunCommand called - saving connection settings");

            actionParameters.TryGetString(UseLocalObsControlName, out var useLocalStr);
            actionParameters.TryGetString(IpAddressControlName, out var ipAddress);
            actionParameters.TryGetString(PortControlName, out var portStr);
            actionParameters.TryGetString(PasswordControlName, out var password);

            var useLocal = !String.Equals(useLocalStr?.Trim(), "false", StringComparison.OrdinalIgnoreCase);

            if (!Int32.TryParse(portStr?.Trim(), out var port) || port < 1 || port > 65535)
            {
                PluginLog.Warning($"ConnectionConfigureCommand: Invalid port '{portStr}', using default 4455");
                port = 4455;
            }

            var config = new PluginConfig
            {
                LogLevel = PluginLog.CurrentLevel,
                UseLocalObs = useLocal,
                RemoteIpAddress = ipAddress?.Trim() ?? "127.0.0.1",
                RemotePort = port,
                RemotePassword = password ?? ""
            };

            var configReader = new PluginConfigReader();
            if (configReader.SaveConfig(config))
            {
                PluginLog.Info($"ConnectionConfigureCommand: Config saved - UseLocal={useLocal}, IP={config.RemoteIpAddress}, Port={port}");
                OBSStudioForLogiPlugin.Instance?.ApplyConnectionConfig(config);
            }
            else
            {
                PluginLog.Error("ConnectionConfigureCommand: Failed to save config");
            }

            return true;
        }
    }
}
