namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingPauseToggleCommand : PluginDynamicCommand
    {
        private readonly ActionImageStore<StateImageData> imageStore;

        public RecordingPauseToggleCommand()
            : base(displayName: "Recording Pause", description: "Pause/resume OBS recording", groupName: "3. Recording")
        {
            this.imageStore = new ActionImageStore<StateImageData>(new StateImageFactory());
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleRecordingPause();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isPaused = OBSStudioForLogiPlugin.Instance?.IsRecordingPaused ?? false;

            StateImageData imageData = new StateImageData
            {
                Id = "recording-pause-toggle",
                IsActive = isPaused,
                ActiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingPause.svg",
                InactiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingResume.svg"
            };

            this.imageStore.UpdateImage(imageData.Id, imageData);

            if (this.imageStore.TryGetImage(imageData.Id, imageSize, out BitmapImage image))
            {
                return image;
            }

            return EmbeddedResources.ReadImage(imageData.InactiveIconPath);
        }
    }
}
