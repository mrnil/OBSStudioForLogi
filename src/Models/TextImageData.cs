namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class TextImageData : IActionImageData, IEquatable<TextImageData>
    {
        public String Id { get; set; }
        public String DisplayText { get; set; }
        public BitmapColor BackgroundColor { get; set; }
        public BitmapColor TextColor { get; set; }

        public Boolean Equals(TextImageData other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id
                && this.DisplayText == other.DisplayText
                && this.BackgroundColor.Equals(other.BackgroundColor)
                && this.TextColor.Equals(other.TextColor);
        }

        public override Int32 GetHashCode()
        {
            return (this.Id, this.DisplayText, this.BackgroundColor, this.TextColor).GetHashCode();
        }

        public override Boolean Equals(Object obj)
        {
            return obj is TextImageData other && this.Equals(other);
        }

        Boolean IEquatable<IActionImageData>.Equals(IActionImageData other)
        {
            return this.Equals(other as TextImageData);
        }

        public static Boolean operator ==(TextImageData left, TextImageData right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static Boolean operator !=(TextImageData left, TextImageData right)
        {
            return !(left == right);
        }
    }
}
