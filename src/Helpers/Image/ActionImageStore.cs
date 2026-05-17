namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    internal class ActionImageStore<T> where T : IActionImageData
    {
        private readonly IActionImageFactory<T> actionImageFactory;
        private readonly ConcurrentDictionary<String, T> actionImageData;
        private readonly ConcurrentDictionary<String, IActionImageFactory<T>> actionImageFactories;
        private readonly ConcurrentDictionary<String, BitmapImage> actionWidth60Images;
        private readonly ConcurrentDictionary<String, BitmapImage> actionWidth90Images;

        public ICollection<String> ActionImageIds
        {
            get
            {
                return this.actionImageData.Keys;
            }
        }

        public ActionImageStore(IActionImageFactory<T> actionImageFactory)
        {
            this.actionImageFactory = actionImageFactory;
            this.actionImageData = new ConcurrentDictionary<String, T>();
            this.actionImageFactories = new ConcurrentDictionary<String, IActionImageFactory<T>>();
            this.actionWidth60Images = new ConcurrentDictionary<String, BitmapImage>();
            this.actionWidth90Images = new ConcurrentDictionary<String, BitmapImage>();
        }

        public Boolean TryGetImage(String id, PluginImageSize imageSize, [NotNullWhen(true)] out BitmapImage bitmapImage)
        {
            ConcurrentDictionary<String, BitmapImage> actionImages;
            if (imageSize == PluginImageSize.Width60)
            {
                actionImages = this.actionWidth60Images;
            }
            else
            {
                actionImages = this.actionWidth90Images;
            }
            Boolean exist = actionImages.TryGetValue(id, out BitmapImage storedBitmapImage);
            if (exist && storedBitmapImage is not null)
            {
                bitmapImage = BitmapImage.FromArray(storedBitmapImage.ToArray());
                return true;
            }
            else
            {
                bitmapImage = null;
                return false;
            }
        }

        public Boolean UpdateImage(String id, T newActionData)
        {
            Boolean exist = this.actionImageData.TryGetValue(id, out T lastActionData);
            PluginLog.Info($"ActionImageStore.UpdateImage: id='{id}', exist={exist}");
            
            if (exist && lastActionData != null)
            {
                PluginLog.Info($"  Last data: {lastActionData}");
                PluginLog.Info($"  New data: {newActionData}");
                PluginLog.Info($"  Equals: {newActionData.Equals(lastActionData)}");
            }
            
            if (!exist || !newActionData.Equals(lastActionData))
            {
                PluginLog.Info($"  Regenerating images for '{id}'");
                IActionImageFactory<T> factory = this.actionImageFactories.GetOrAdd(id, this.actionImageFactory.Create());
                this.actionImageData.AddOrUpdate(id, newActionData, (key, oldValue) => newActionData);

                BitmapImage bitmapWidth60Image = factory.DrawBitmapImage(newActionData, PluginImageSize.Width60);
                this.actionWidth60Images.AddOrUpdate(id, bitmapWidth60Image, (key, oldValue) => bitmapWidth60Image);

                BitmapImage bitmapWidth90Image = factory.DrawBitmapImage(newActionData, PluginImageSize.Width90);
                this.actionWidth90Images.AddOrUpdate(id, bitmapWidth90Image, (key, oldValue) => bitmapWidth90Image);

                return true;
            }
            PluginLog.Info($"  Skipping regeneration - data unchanged");
            return false;
        }

        public Boolean UpdateImage(T newActionData)
        {
            return this.UpdateImage(newActionData.Id, newActionData);
        }
    }
}
