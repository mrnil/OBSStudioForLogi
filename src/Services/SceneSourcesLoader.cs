namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    // Loads a scene's source and audio source lists in the background. Running the requests on the
    // OBS event thread held up every other event (visibility, media, mute) behind them - see
    // assessment.md #18. Only the most recent load is delivered, so quick scene switches can't
    // finish out of order and show an older scene's sources.
    //
    // Only the scene source folders use these lists, so nothing is fetched unless one of them is
    // open: each folder registers as a viewer while it is open (assessment.md #25).
    public class SceneSourcesLoader
    {
        private readonly Func<String, (String[] Sources, String[] AudioSources)> _fetch;
        private readonly Func<Boolean> _isConnected;
        private readonly Action<Action> _runInBackground;
        private readonly Object _deliveryLock = new Object();
        private readonly HashSet<String> _viewers = new HashSet<String>();
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

        public Boolean HasViewers
        {
            get
            {
                lock (this._viewers)
                {
                    return this._viewers.Count > 0;
                }
            }
        }

        public void AddViewer(String owner)
        {
            if (String.IsNullOrEmpty(owner))
            {
                return;
            }

            lock (this._viewers)
            {
                if (this._viewers.Add(owner))
                {
                    PluginLog.Info($"Scene sources wanted by '{owner}' ({this._viewers.Count} viewer(s))");
                }
            }
        }

        public void RemoveViewer(String owner)
        {
            if (String.IsNullOrEmpty(owner))
            {
                return;
            }

            lock (this._viewers)
            {
                if (this._viewers.Remove(owner))
                {
                    PluginLog.Info($"Scene sources no longer wanted by '{owner}' ({this._viewers.Count} viewer(s))");
                }
            }
        }

        // Returns true if a load was started, false if it was skipped because the scene name is
        // empty or no folder is open to show the result.
        public Boolean Load(String sceneName, Action<String, String[], String[]> callback)
        {
            if (String.IsNullOrEmpty(sceneName))
            {
                PluginLog.Warning("Cannot update sources - scene name is empty");
                return false;
            }

            if (!this.HasViewers)
            {
                PluginLog.Debug($"Skipping source load for scene '{sceneName}' - no scene source folder is open");
                return false;
            }

            Int64 generation = Interlocked.Increment(ref this._generation);
            PluginLog.Debug($"Loading sources for scene '{sceneName}'");
            this._runInBackground(() => this.LoadInBackground(sceneName, generation, callback));
            return true;
        }

        private void LoadInBackground(String sceneName, Int64 generation, Action<String, String[], String[]> callback)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                (String[] sources, String[] audioSources) = this._fetch(sceneName);
                sources = sources ?? new String[0];
                audioSources = audioSources ?? new String[0];
                PluginLog.Info($"Loaded {sources.Length} sources and {audioSources.Length} audio sources for scene '{sceneName}' in {stopwatch.ElapsedMilliseconds} ms");

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

                    callback?.Invoke(sceneName, sources, audioSources);
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to update sources for scene '{sceneName}': {ex.Message}");
            }
        }
    }
}
