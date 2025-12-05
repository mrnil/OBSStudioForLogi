namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading.Tasks;
    using OBSWebsocketDotNet;

    public class OBSWebSocketManager : IDisposable
    {
        private readonly OBSWebsocket _obs;

        public Boolean IsConnected => this._obs?.IsConnected ?? false;

        public OBSWebSocketManager()
        {
            this._obs = new OBSWebsocket();
        }

        public async Task ConnectAsync(String url, String password)
        {
            await Task.Run(() =>
            {
                this._obs.ConnectAsync(url, password);
            });
        }

        public void Disconnect()
        {
            this._obs?.Disconnect();
        }

        public void Dispose()
        {
            this.Disconnect();
        }
    }
}
