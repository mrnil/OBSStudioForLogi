namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    // Loads a scene's source and audio source lists in the background. A scene change costs
    // about ten OBS requests, and running them on the OBS event thread held up every other event
    // (visibility, media, mute) behind them - see assessment.md #18. Only the most recent load is
    // delivered, so quick scene switches can't finish out of order and show an older scene's
    // sources.
    public class SceneSourcesLoader
    {
        private readonly Func<String, (String[] Sources, String[] AudioSources)> _fetch;
        private readonly Func<Boolean> _isConnected;
        private readonly Action<Action> _runInBackground;
        private readonly Object _deliveryLock = new Object();
        private Int64 _generation;

        public SceneSourcesLoader(Func<String, (String[] Sources, String[] AudioSources)> fetch, Func<Boolean> isConnected)
            : this(fetch, isConnected, action => Task.Run(action))
        {
        }

        public SceneSourcesLoader(Func<String, (String[] Sources, String[] AudioSources)> fetch, Func<Boolean> isConnected, Action<Action> runInBackground)
        {
            this._fetch = fetch ?? throw new ArgumentNullException(nameof(fetch));
            this._isConnected = isConnected ?? (() => true);
            this._runInBackground = runInBackground ?? (action => Task.Run(action));
        }

        public void Load(String sceneName, Action<String, String[], String[]> callback)
        {
            if (String.IsNullOrEmpty(sceneName))
            {
                PluginLog.Warning("Cannot update sources - scene name is empty");
                return;
            }

            Int64 generation = Interlocked.Increment(ref this._generation);
            this._runInBackground(() => this.LoadInBackground(sceneName, generation, callback));
        }

        private void LoadInBackground(String sceneName, Int64 generation, Action<String, String[], String[]> callback)
        {
            try
            {
                (String[] sources, String[] audioSources) = this._fetch(sceneName);

                // Delivery is serialised so an older load that passed the check can't land after a
                // newer one.
                lock (this._deliveryLock)
                {
                    if (generation != Interlocked.Read(ref this._generation))
                    {
                        PluginLog.Debug($"Dropping sources for scene '{sceneName}' - a newer scene load superseded it");
                        return;
                    }

                    // A load that finishes after a disconnect would repopulate folders that
                    // OnDisconnected just cleared.
                    if (!this._isConnected())
                    {
                        PluginLog.Debug($"Dropping sources for scene '{sceneName}' - not connected");
                        return;
                    }

                    callback?.Invoke(sceneName, sources ?? new String[0], audioSources ?? new String[0]);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to update sources for scene '{sceneName}': {ex.Message}");
            }
        }
    }
}
