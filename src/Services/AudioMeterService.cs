namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;

    public class AudioMeterService
    {
        private readonly Dictionary<String, Models.AudioMeterLevels> _levels = new Dictionary<String, Models.AudioMeterLevels>();
        private readonly Object _lock = new Object();

        public void UpdateLevels(String inputName, Models.AudioMeterLevels levels)
        {
            if (String.IsNullOrEmpty(inputName) || levels == null)
                return;

            lock (this._lock)
            {
                this._levels[inputName] = levels;
            }
        }

        public Models.AudioMeterLevels GetLevels(String inputName)
        {
            if (String.IsNullOrEmpty(inputName))
                return Models.AudioMeterLevels.Empty;

            lock (this._lock)
            {
                return this._levels.TryGetValue(inputName, out var levels) ? levels : Models.AudioMeterLevels.Empty;
            }
        }

        public void Clear()
        {
            lock (this._lock)
            {
                this._levels.Clear();
            }
        }
    }
}
