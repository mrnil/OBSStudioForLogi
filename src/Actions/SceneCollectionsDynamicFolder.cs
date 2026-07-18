namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SceneCollectionsDynamicFolder : PluginDynamicFolder, IObsCommand, ISceneCollectionAwareCommand
    {
        public static SceneCollectionsDynamicFolder Instance { get; private set; }

        private String[] _sceneCollections = new String[0];
        private String _currentSceneCollection = String.Empty;

        public SceneCollectionsDynamicFolder()
        {
            Instance = this;
            OBSStudioForLogiPlugin.Instance?.RegisterCommand(this);
            this.DisplayName = "OBS Scene Collections";
            this.GroupName = "7. Scenes###Collections";
            this.Description = "Folder of available OBS scene collections";
        }

        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _)
        {
            return PluginDynamicFolderNavigation.ButtonArea;
        }

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType deviceType)
        {
            return this._sceneCollections.Select(collection => this.CreateCommandName(collection));
        }

        public void OnSceneCollectionChanged(String oldSceneCollection, String newSceneCollection)
        {
            var old = this._currentSceneCollection;
            this._currentSceneCollection = newSceneCollection ?? String.Empty;

            if (!String.IsNullOrEmpty(old) && old != this._currentSceneCollection)
            {
                this.CommandImageChanged(old);
            }

            if (!String.IsNullOrEmpty(this._currentSceneCollection))
            {
                this.CommandImageChanged(this._currentSceneCollection);
            }
        }

        public void OnConnected()
        {
            this._sceneCollections = OBSStudioForLogiPlugin.Instance?.GetSceneCollectionList() ?? new String[0];
            this._currentSceneCollection = OBSStudioForLogiPlugin.Instance?.CurrentSceneCollection ?? String.Empty;
            PluginLog.Debug($"SceneCollectionsDynamicFolder loaded {this._sceneCollections.Length} collections, current: '{this._currentSceneCollection}'");
            this.ButtonActionNamesChanged();
        }

        public void OnDisconnected()
        {
            this._sceneCollections = new String[0];
            this._currentSceneCollection = String.Empty;
            this.ButtonActionNamesChanged();
        }

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isSelected = actionParameter == this._currentSceneCollection;
            return ButtonImageHelper.Icon(isSelected ? "ScenesCollectionsSelected.svg" : "ScenesCollectionsUnselected.svg");
        }

        public override void RunCommand(String actionParameter)
        {
            if (String.IsNullOrEmpty(actionParameter))
                return;

            OBSStudioForLogiPlugin.Instance?.SwitchSceneCollection(actionParameter);
        }
    }
}
