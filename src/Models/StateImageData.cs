namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class StateImageData : IActionImageData, IEquatable<StateImageData>
    {
        public String Id { get; set; }
        public Boolean IsActive { get; set; }
        public String ActiveIconPath { get; set; }
        public String InactiveIconPath { get; set; }

        public Boolean Equals(StateImageData other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id
                && this.IsActive == other.IsActive
                && this.ActiveIconPath == other.ActiveIconPath
                && this.InactiveIconPath == other.InactiveIconPath;
        }

        public override Int32 GetHashCode()
        {
            return (this.Id, this.IsActive, this.ActiveIconPath, this.InactiveIconPath).GetHashCode();
        }

        public override Boolean Equals(Object obj)
        {
            return obj is StateImageData other && this.Equals(other);
        }

        Boolean IEquatable<IActionImageData>.Equals(IActionImageData other)
        {
            return this.Equals(other as StateImageData);
        }

        public static Boolean operator ==(StateImageData left, StateImageData right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static Boolean operator !=(StateImageData left, StateImageData right)
        {
            return !(left == right);
        }
    }
}
