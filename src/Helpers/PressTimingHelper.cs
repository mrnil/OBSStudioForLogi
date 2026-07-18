namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class DoubleTapHelper
    {
        private const Int32 DoubleTapThreshold = 500; // ms between taps
        private readonly Dictionary<String, TapState> _tapStates = new Dictionary<String, TapState>();

        private class TapState
        {
            public DateTime LastTapTime { get; set; }
            public CancellationTokenSource Cancellation { get; set; }
        }

        public void OnTap(String actionParameter, Action<String> onSingleTap, Action<String> onDoubleTap)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            DateTime now = DateTime.UtcNow;
            CancellationTokenSource cancellationToDispose = null;

            lock (this._tapStates)
            {
                if (this._tapStates.TryGetValue(actionParameter, out TapState state))
                {
                    Double elapsed = (now - state.LastTapTime).TotalMilliseconds;

                    if (elapsed < DoubleTapThreshold)
                    {
                        state.Cancellation.Cancel();
                        cancellationToDispose = state.Cancellation;
                        this._tapStates.Remove(actionParameter);
                        onDoubleTap?.Invoke(actionParameter);
                        cancellationToDispose.Dispose();
                        return;
                    }
                }

                var cancellation = new CancellationTokenSource();
                this._tapStates[actionParameter] = new TapState
                {
                    LastTapTime = now,
                    Cancellation = cancellation
                };

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(DoubleTapThreshold, cancellation.Token);

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
