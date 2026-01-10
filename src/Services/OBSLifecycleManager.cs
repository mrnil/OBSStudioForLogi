namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public class OBSLifecycleManager
    {
        private readonly IPluginLog _log;

        public OBSLifecycleManager() : this(new PluginLogAdapter())
        {
        }

        public OBSLifecycleManager(IPluginLog log)
        {
            this._log = log;
        }

        public async Task<Boolean> IsPortListeningAsync(String host, Int32 port)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(host, port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Boolean> WaitForPortAsync(String host, Int32 port, Int32 maxAttempts = 20, Int32 delayMs = 1000)
        {
            this._log.Info($"Waiting for port {port} to be listening (max {maxAttempts} attempts)");
            
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (await this.IsPortListeningAsync(host, port))
                {
                    this._log.Info($"Port {port} is now listening");
                    return true;
                }

                this._log.Info($"Port {port} not ready, attempt {attempt + 1}/{maxAttempts}");
                await Task.Delay(delayMs);
            }

            this._log.Warning($"Port {port} did not become available after {maxAttempts} attempts");
            return false;
        }
    }
}
