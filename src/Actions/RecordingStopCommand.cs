namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class RecordingStopCommand : PluginDynamicCommand
    {
        private readonly ActionImageStore<StateImageData> imageStore;

        public RecordingStopCommand()
            : base(displayName: "Stop Recording", description: "Stop OBS recording", groupName: "3. Recording")
        {
            this.imageStore = new ActionImageStore<StateImageData>(new StateImageFactory());
        }

        protected override void RunCommand(String actionParameter)
        {
            OBSStudioForLogiPlugin.Instance?.StopRecording();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            Boolean isRecording = OBSStudioForLogiPlugin.Instance?.IsRecording ?? false;

            StateImageData imageData = new StateImageData
            {
                Id = "recording-stop",
                IsActive = isRecording,
                ActiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingStop.svg",
                InactiveIconPath = "Loupedeck.OBSStudioForLogiPlugin.Icons.RecordingStopDisabled.svg"
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
