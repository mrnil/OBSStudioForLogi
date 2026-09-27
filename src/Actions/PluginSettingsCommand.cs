namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using Loupedeck.OBSStudioForLogiPlugin.Models;
    using Loupedeck.OBSStudioForLogiPlugin.Services;

    public class PluginSettingsCommand : ActionEditorCommand
    {
        private const String UseLocalObsControlName = "UseLocalObs";
        private const String IpAddressControlName = "IpAddress";
        private const String PortControlName = "Port";
        private const String PasswordControlName = "Password";
        private const String PollingIntervalControlName = "PollingInterval";
        private const String MeterRefreshIntervalControlName = "MeterRefreshInterval";

        public static PluginSettingsCommand Instance { get; private set; }

        public PluginSettingsCommand()
        {
            Instance = this;
            this.Name = "PluginSettings";
            this.DisplayName = "Plugin Settings";
            this.GroupName = "1. OBS";
            this.Description = "Configure plugin settings including OBS connection and stats polling. Press to save and apply.";

            var localSettings = OBSStudioForLogiPlugin.Instance?.GetLocalOBSSettings();
            var defaultIp = localSettings?.IpAddress ?? "127.0.0.1";
            var defaultPort = localSettings?.Port.ToString() ?? "4455";

            this.ActionEditor.AddControlEx(new ActionEditorCheckbox(UseLocalObsControlName, "Use Local OBS").SetDefaultValue(true));
            this.ActionEditor.AddControlEx(new ActionEditorTextbox(IpAddressControlName, $"IP Address (detected: {defaultIp})").SetRequired());
            this.ActionEditor.AddControlEx(new ActionEditorTextbox(PortControlName, $"Port (detected: {defaultPort})").SetRequired());
            this.ActionEditor.AddControlEx(new ActionEditorTextbox(PasswordControlName, "Password (sensitive)"));

            var pollingListbox = new ActionEditorListbox(PollingIntervalControlName, "Stats Polling Interval");
            this.ActionEditor.AddControlEx(pollingListbox);

            var meterRefreshListbox = new ActionEditorListbox(MeterRefreshIntervalControlName, "Audio Meter Refresh Rate");
            this.ActionEditor.AddControlEx(meterRefreshListbox);

            this.ActionEditor.ListboxItemsRequested += this.OnListboxItemsRequested;

            PluginLog.Debug("PluginSettingsCommand: Initialized");
        }

        private void OnListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
        {
            if (e.ControlName.EqualsNoCase(PollingIntervalControlName))
            {
                e.AddItem("2000", "2 seconds", "Poll OBS stats every 2 seconds");
                e.AddItem("5000", "5 seconds", "Poll OBS stats every 5 seconds");
                e.AddItem("10000", "10 seconds", "Poll OBS stats every 10 seconds");
                e.SetSelectedItemName("5000");
            }
            else if (e.ControlName.EqualsNoCase(MeterRefreshIntervalControlName))
            {
                e.AddItem("50", "20 fps", "Refresh audio meters every 50ms");
                e.AddItem("100", "10 fps", "Refresh audio meters every 100ms");
                e.AddItem("200", "5 fps", "Refresh audio meters every 200ms");
                e.SetSelectedItemName("100");
            }
        }

        protected override Boolean OnLoad() => true;

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            PluginLog.Info("PluginSettings: RunCommand called - saving settings");

            actionParameters.TryGetBoolean(UseLocalObsControlName, out var useLocal);
            actionParameters.TryGetString(IpAddressControlName, out var ipAddress);
            actionParameters.TryGetString(PortControlName, out var portStr);
            actionParameters.TryGetString(PasswordControlName, out var password);
            actionParameters.TryGetString(PollingIntervalControlName, out var pollingStr);
            actionParameters.TryGetString(MeterRefreshIntervalControlName, out var meterRefreshStr);

            if (!Int32.TryParse(portStr?.Trim(), out var port) || port < 1 || port > 65535)
            {
                PluginLog.Warning($"PluginSettings: Invalid port '{portStr}', using default 4455");
                port = 4455;
            }

            if (!Int32.TryParse(pollingStr, out var pollingInterval) || (pollingInterval != 2000 && pollingInterval != 5000 && pollingInterval != 10000))
            {
                pollingInterval = 5000;
            }

            if (!Int32.TryParse(meterRefreshStr, out var meterRefreshInterval) || (meterRefreshInterval != 50 && meterRefreshInterval != 100 && meterRefreshInterval != 200))
            {
                meterRefreshInterval = 100;
            }

            var config = new PluginConfig
            {
                LogLevel = PluginLog.CurrentLevel,
                UseLocalObs = useLocal,
                RemoteIpAddress = ipAddress?.Trim() ?? "127.0.0.1",
                RemotePort = port,
                RemotePassword = password ?? "",
                StatsPollingInterval = pollingInterval,
                AudioMeterRefreshInterval = meterRefreshInterval
            };

            var configReader = new PluginConfigReader();
            if (configReader.SaveConfig(config))
            {
                PluginLog.Info($"PluginSettings: Config saved - UseLocal={useLocal}, IP={config.RemoteIpAddress}, Port={port}, Polling={pollingInterval}ms, MeterRefresh={meterRefreshInterval}ms");
                OBSStudioForLogiPlugin.Instance?.ApplyConnectionConfig(config);
            }
            else
            {
                PluginLog.Error("PluginSettings: Failed to save config");
            }

            return true;
        }
    }
}
