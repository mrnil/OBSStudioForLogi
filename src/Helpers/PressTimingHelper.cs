namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Loupedeck.OBSStudioForLogiPlugin.Helpers;

    public class DoubleTapHelper
    {
        private readonly Func<Int32> _windowMs;
        private readonly Dictionary<String, TapState> _tapStates = new Dictionary<String, TapState>();

        public DoubleTapHelper()
            : this(() => OBSTimings.DoubleTapWindow)
        {
        }

        /// <param name="windowMs">Read on every tap, so a settings change applies straight away.
        /// 0 or less turns double-tap off: every tap runs the single-tap action at once.</param>
        public DoubleTapHelper(Func<Int32> windowMs)
        {
            this._windowMs = windowMs;
        }

        private class TapState
        {
            public DateTime LastTapTime { get; set; }
            public CancellationTokenSource Cancellation { get; set; }
        }

        public void OnTap(String actionParameter, Action<String> onSingleTap, Action<String> onDoubleTap)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            Int32 windowMs = this._windowMs();
            if (windowMs <= 0)
            {
                onSingleTap?.Invoke(actionParameter);
                return;
            }

            DateTime now = DateTime.UtcNow;
            CancellationTokenSource cancellationToDispose = null;

            lock (this._tapStates)
            {
                if (this._tapStates.TryGetValue(actionParameter, out TapState state))
                {
                    Double elapsed = (now - state.LastTapTime).TotalMilliseconds;

                    if (elapsed < windowMs)
                    {
                        state.Cancellation.Cancel();
                        cancellationToDispose = state.Cancellation;
                        this._tapStates.Remove(actionParameter);
                        onDoubleTap?.Invoke(actionParameter);
                        cancellationToDispose.Dispose();
                        return;
                    }
                }

                CancellationTokenSource cancellation = new CancellationTokenSource();
                this._tapStates[actionParameter] = new TapState
                {
                    LastTapTime = now,
                    Cancellation = cancellation
                };

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(windowMs, cancellation.Token);

                        lock (this._tapStates)
                        {
                            this._tapStates.Remove(actionParameter);
                        }

                        onSingleTap?.Invoke(actionParameter);
                    }
                    catch (TaskCanceledException)
                    {
                        // Expected when double tap occurs
                    }
                    finally
                    {
                        cancellation.Dispose();
                    }
                });
            }
        }

        public void Reset()
        {
            lock (this._tapStates)
            {
                foreach (var state in this._tapStates.Values)
                {
                    state.Cancellation.Cancel();
                    state.Cancellation.Dispose();
                }

                this._tapStates.Clear();
            }
        }
    }
}
