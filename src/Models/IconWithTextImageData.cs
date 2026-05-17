namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class IconWithTextImageData : IActionImageData, IEquatable<IconWithTextImageData>
    {
        public String Id { get; set; }
        public String IconPath { get; set; }
        public String DisplayText { get; set; }
        public BitmapColor TextColor { get; set; }

        public Boolean Equals(IconWithTextImageData other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id
                && this.IconPath == other.IconPath
                && this.DisplayText == other.DisplayText
                && this.TextColor.Equals(other.TextColor);
        }

        public override Int32 GetHashCode()
        {
            return (this.Id, this.IconPath, this.DisplayText, this.TextColor).GetHashCode();
        }

        public override Boolean Equals(Object obj)
        {
            return obj is IconWithTextImageData other && this.Equals(other);
        }

        Boolean IEquatable<IActionImageData>.Equals(IActionImageData other)
        {
            return this.Equals(other as IconWithTextImageData);
        }

        public static Boolean operator ==(IconWithTextImageData left, IconWithTextImageData right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static Boolean operator !=(IconWithTextImageData left, IconWithTextImageData right)
        {
            return !(left == right);
        }
    }
}
