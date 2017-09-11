using System;

namespace Msv.AutoMiner.Service.Data
{
    public struct BlockInfo : IEquatable<BlockInfo>
    {
        public long Timestamp { get; set; }
        public long Height { get; set; }
        public double? Reward { get; set; }

        public bool Equals(BlockInfo other)
        {
            return Timestamp == other.Timestamp && Height == other.Height && Reward.Equals(other.Reward);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BlockInfo && Equals((BlockInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                hashCode = (hashCode * 397) ^ Reward.GetHashCode();
                return hashCode;
            }
        }
    }
}
