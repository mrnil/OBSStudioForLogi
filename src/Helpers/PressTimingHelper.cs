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
            
            if (this._tapStates.TryGetValue(actionParameter, out TapState state))
            {
                Double elapsed = (now - state.LastTapTime).TotalMilliseconds;
                
                if (elapsed < DoubleTapThreshold)
                {
                    // Double tap detected
                    state.Cancellation?.Cancel();
                    this._tapStates.Remove(actionParameter);
                    onDoubleTap?.Invoke(actionParameter);
                    return;
                }
            }
            
            // First tap or too long since last tap
            var cancellation = new CancellationTokenSource();
            this._tapStates[actionParameter] = new TapState
            {
                LastTapTime = now,
                Cancellation = cancellation
            };

            // Wait to see if a second tap comes
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(DoubleTapThreshold, cancellation.Token);
                    
                    if (!cancellation.Token.IsCancellationRequested)
                    {
                        // No second tap came - execute single tap
                        this._tapStates.Remove(actionParameter);
                        onSingleTap?.Invoke(actionParameter);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Expected when double tap occurs
                }
            });
        }

        public void Reset()
        {
            foreach (var state in this._tapStates.Values)
            {
                state.Cancellation?.Cancel();
            }
            this._tapStates.Clear();
        }
    }
}
