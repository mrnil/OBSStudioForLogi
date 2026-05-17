namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingToggleCommand : PluginDynamicCommand
    {
        private readonly ActionImageStore<StateImageData> imageStore;

        public RecordingToggleCommand()
            : base(displayName: "Toggle Recording", description: "Start/stop OBS recording", groupName: "3. Recording")
        {
            this.imageStore = new ActionImageStore<StateImageData>(new StateImageFactory());
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.ToggleRecording();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;

            StateImageData imageData = new StateImageData
            {
                Id = "recording-toggle",
                IsActive = isRecording,
                ActiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingOff.svg",
                InactiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingOn.svg"
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
