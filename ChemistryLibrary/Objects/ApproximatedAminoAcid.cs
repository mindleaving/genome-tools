using System;
using System.Threading;
using Commons;

namespace ChemistryLibrary.Objects
{
    public class ApproximatedAminoAcid : IEquatable<ApproximatedAminoAcid>
    {
        private long lastId;

        public ApproximatedAminoAcid(AminoAcidName aminoAcidName)
        {
            Id = Interlocked.Increment(ref lastId);
            Name = aminoAcidName;
        }

        public long Id { get; }
        public UnitPoint3D NitrogenPosition { get; set; }
        public UnitPoint3D CarbonAlphaPosition { get; set; }
        public UnitPoint3D CarbonPosition { get; set; }
        public UnitValue OmegaAngle { get; set; }
        public UnitValue PhiAngle { get; set; }
        public UnitValue PsiAngle { get; set; }
        public AminoAcidName Name { get; }

        public bool IsFrozen { get; set; }

        public bool Equals(ApproximatedAminoAcid other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ApproximatedAminoAcid) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
