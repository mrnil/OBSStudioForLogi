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
            PluginLog.Info($"AudioSelectionState: Selecting '{inputName}' for dial control");
            _selectedInput = inputName;
            SelectionChanged?.Invoke(previous, inputName);
        }

        public static void Deselect()
        {
            var previous = _selectedInput;
            PluginLog.Info($"AudioSelectionState: Deselecting '{_selectedInput}' from dial control");
            _selectedInput = null;
            SelectionChanged?.Invoke(previous, null);
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
