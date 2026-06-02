namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public static class AudioSelectionState
    {
        private static String _selectedInput = null;

        public static String SelectedInput => _selectedInput;

        public static Boolean IsSelected(String inputName)
        {
            return !String.IsNullOrEmpty(_selectedInput) && _selectedInput == inputName;
        }

        public static void Select(String inputName)
        {
            PluginLog.Info($"AudioSelectionState: Selecting '{inputName}' for dial control");
            _selectedInput = inputName;
        }

        public static void Deselect()
        {
            PluginLog.Info($"AudioSelectionState: Deselecting '{_selectedInput}' from dial control");
            _selectedInput = null;
        }

        public static void DeselectIfMatches(String inputName)
        {
            if (_selectedInput == inputName)
            {
                PluginLog.Info($"AudioSelectionState: Deselecting '{inputName}' (matched current selection)");
                _selectedInput = null;
            }
        }
    }
}
