namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public static class AudioSelectionState
    {
        private static String _selectedInput = null;

        public static event Action<String, String> SelectionChanged;

        public static String SelectedInput => _selectedInput;

        public static Boolean IsSelected(String inputName)
        {
            return !String.IsNullOrEmpty(_selectedInput) && _selectedInput == inputName;
        }

        public static void Select(String inputName)
        {
            var previous = _selectedInput;
            PluginLog.Debug($"AudioSelectionState: Selecting '{inputName}' for dial control");
            _selectedInput = inputName;
            SelectionChanged?.Invoke(previous, inputName);
        }

        public static void Deselect()
        {
            var previous = _selectedInput;
            PluginLog.Debug($"AudioSelectionState: Deselecting '{_selectedInput}' from dial control");
            _selectedInput = null;
            SelectionChanged?.Invoke(previous, null);
        }

        // Keeps the dial on an input OBS has renamed. Without this, the refreshed input list no
        // longer contains the old name and the folders drop the selection.
        public static void RenameIfMatches(String oldInputName, String newInputName)
        {
            if (String.IsNullOrEmpty(oldInputName) || String.IsNullOrEmpty(newInputName) || _selectedInput != oldInputName)
            {
                return;
            }

            PluginLog.Debug($"AudioSelectionState: Selected input renamed from '{oldInputName}' to '{newInputName}'");
            _selectedInput = newInputName;
            SelectionChanged?.Invoke(oldInputName, newInputName);
        }

        public static void DeselectIfMatches(String inputName)
        {
            if (_selectedInput == inputName)
            {
                Deselect();
            }
        }
    }
}
