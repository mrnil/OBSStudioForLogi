namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class AudioInputImageData : IActionImageData, IEquatable<AudioInputImageData>
    {
        public String Id { get; set; }
        public String InputName { get; set; }
        public Boolean IsMuted { get; set; }
        public Single VolumeLevel { get; set; }
        public String IconPath { get; set; }

        public Boolean Equals(AudioInputImageData other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id
                && this.InputName == other.InputName
                && this.IsMuted == other.IsMuted
                && this.VolumeLevel == other.VolumeLevel
                && this.IconPath == other.IconPath;
        }

        public override Int32 GetHashCode()
        {
            return (this.Id, this.InputName, this.IsMuted, this.VolumeLevel, this.IconPath).GetHashCode();
        }

        public override Boolean Equals(Object obj)
        {
            return obj is AudioInputImageData other && this.Equals(other);
        }

        Boolean IEquatable<IActionImageData>.Equals(IActionImageData other)
        {
            return this.Equals(other as AudioInputImageData);
        }

        public static Boolean operator ==(AudioInputImageData left, AudioInputImageData right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static Boolean operator !=(AudioInputImageData left, AudioInputImageData right)
        {
            return !(left == right);
        }
    }
}
