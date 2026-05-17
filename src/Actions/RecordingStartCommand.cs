namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingStartCommand : PluginDynamicCommand
    {
        private readonly ActionImageStore<StateImageData> imageStore;

        public RecordingStartCommand()
            : base(displayName: "Start Recording", description: "Start OBS recording", groupName: "3. Recording")
        {
            this.imageStore = new ActionImageStore<StateImageData>(new StateImageFactory());
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StartRecording();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;

            StateImageData imageData = new StateImageData
            {
                Id = "recording-start",
                IsActive = isRecording,
                ActiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingStartDisabled.svg",
                InactiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingStart.svg"
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
