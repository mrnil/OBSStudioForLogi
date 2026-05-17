namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class SimpleIconImageData : IActionImageData, IEquatable<SimpleIconImageData>
    {
        public String Id { get; set; }
        public String IconPath { get; set; }

        public Boolean Equals(SimpleIconImageData other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id && this.IconPath == other.IconPath;
        }

        public override Int32 GetHashCode()
        {
            return (this.Id, this.IconPath).GetHashCode();
        }

        public override Boolean Equals(Object obj)
        {
            return obj is SimpleIconImageData other && this.Equals(other);
        }

        Boolean IEquatable<IActionImageData>.Equals(IActionImageData other)
        {
            return this.Equals(other as SimpleIconImageData);
        }

        public static Boolean operator ==(SimpleIconImageData left, SimpleIconImageData right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static Boolean operator !=(SimpleIconImageData left, SimpleIconImageData right)
        {
            return !(left == right);
        }
    }
}
