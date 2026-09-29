namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Stands in for a folder's buttons while its list loads: "Loading <what>..." drawn once across
    // TileCount side-by-side buttons, each button showing its own slice of the text. Folders show
    // these tiles instead of their normal buttons while IsLoading is set.
    //
    // The SDK always puts a dynamic folder's Back button top left, so a folder's first buttons
    // fill the rest of the top row. Two tiles keep the message on that row on every supported
    // device, including the MX Creative Console's three-wide keypad.
    public class LoadingTiles
    {
        public const Int32 TileCount = 2;

        private const String ParameterPrefix = "__loading_";

        private static readonly BitmapColor TextColor = new BitmapColor(128, 128, 128);

        private readonly String _message;
        private volatile Boolean _isLoading;

        public LoadingTiles(String what)
        {
            this._message = $"Loading {what}...";
        }

        public String Message => this._message;

        public Boolean IsLoading => this._isLoading;

        // The action parameters for the tiles, in display order.
        public IEnumerable<String> ActionParameters => Enumerable.Range(0, TileCount).Select(index => ParameterPrefix + index);

        public void Begin()
        {
            this._isLoading = true;
        }

        // Returns true if the tiles were showing, so the caller knows the button list changed.
        public Boolean End()
        {
            Boolean wasLoading = this._isLoading;
            this._isLoading = false;
            return wasLoading;
        }

        public static Boolean IsTile(String actionParameter)
        {
            return actionParameter != null && actionParameter.StartsWith(ParameterPrefix, StringComparison.Ordinal);
        }

        // The tile's position in the row, or -1 if actionParameter isn't a tile.
        public static Int32 TileIndex(String actionParameter)
        {
            if (!IsTile(actionParameter)
                || !Int32.TryParse(actionParameter.Substring(ParameterPrefix.Length), out Int32 index)
                || index < 0
                || index >= TileCount)
            {
                return -1;
            }

            return index;
        }

        // Draws the message across a strip TileCount buttons wide and shifted left by this tile's
        // offset, so the builder's bounds clip it to this tile's slice.
        public BitmapImage Render(String actionParameter, PluginImageSize imageSize)
        {
            Int32 index = Math.Max(0, TileIndex(actionParameter));

            using (BitmapBuilder builder = new BitmapBuilder(imageSize))
            {
                Int32 width = builder.Width;
                Int32 height = builder.Height;
                Int32 fontSize = FontSize(this._message, width);

                PluginLog.Debug($"Rendering loading tile {index} of '{this._message}' at {width}x{height}, font {fontSize}");

                builder.Clear(BitmapColor.Black);
                builder.DrawText(this._message, -index * width, 0, width * TileCount, height, TextColor, fontSize);
                return builder.ToImage();
            }
        }

        // The largest size, up to a fifth of a button's width, at which the message fits on one
        // line across the tiles. Characters average a bit over half the font size in width.
        internal static Int32 FontSize(String message, Int32 tileWidth)
        {
            Int32 fitsStrip = (Int32)(tileWidth * TileCount / (Math.Max(1, message?.Length ?? 0) * 0.6));
            return Math.Max(8, Math.Min(tileWidth / 5, fitsStrip));
        }
    }
}
