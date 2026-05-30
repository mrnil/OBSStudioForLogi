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
            _selectedInput = inputName;
        }

        public static void Deselect()
        {
            _selectedInput = null;
        }

        public static void DeselectIfMatches(String inputName)
        {
            if (_selectedInput == inputName)
            {
                _selectedInput = null;
            }
        }
    }
}
