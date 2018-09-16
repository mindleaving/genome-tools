using System;
using System.Threading;
using Commons.Physics;

namespace ChemistryLibrary.Objects
{
    public class ApproximatedAminoAcid : IEquatable<ApproximatedAminoAcid>
    {
        private static long lastId;

        public ApproximatedAminoAcid(AminoAcidName aminoAcidName, int sequenceNumber)
        {
            Id = Interlocked.Increment(ref lastId);
            Name = aminoAcidName;
            SequenceNumber = sequenceNumber;
        }
        private ApproximatedAminoAcid(long id, AminoAcidName aminoAcidName, int sequenceNumber)
        {
            Id = id;
            Name = aminoAcidName;
            SequenceNumber = sequenceNumber;
        }

        public long Id { get; }
        public UnitPoint3D NitrogenPosition { get; set; }
        public UnitPoint3D CarbonAlphaPosition { get; set; }
        public UnitPoint3D CarbonPosition { get; set; }
        public UnitVector3D NitrogenVelocity { get; set; } = new UnitVector3D(Unit.MetersPerSecond, 0, 0, 0);
        public UnitVector3D CarbonAlphaVelocity { get; set; } = new UnitVector3D(Unit.MetersPerSecond, 0, 0, 0);
        public UnitVector3D CarbonVelocity { get; set; } = new UnitVector3D(Unit.MetersPerSecond, 0, 0, 0);
        public UnitValue OmegaAngle { get; set; }
        public UnitValue PhiAngle { get; set; }
        public UnitValue PsiAngle { get; set; }
        public AminoAcidName Name { get; }

        public bool IsFrozen { get; set; }
        public int SequenceNumber { get; set; }

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

        public override string ToString()
        {
            return Name.ToString();
        }

        public ApproximatedAminoAcid DeepClone()
        {
            return new ApproximatedAminoAcid(Id, Name, SequenceNumber)
            {
                NitrogenPosition = NitrogenPosition?.DeepClone(),
                CarbonAlphaPosition = CarbonAlphaPosition?.DeepClone(),
                CarbonPosition =  CarbonPosition?.DeepClone(),
                NitrogenVelocity = NitrogenVelocity?.DeepClone(),
                CarbonAlphaVelocity = CarbonAlphaVelocity?.DeepClone(),
                CarbonVelocity = CarbonVelocity?.DeepClone(),
                OmegaAngle = OmegaAngle?.DeepClone(),
                PhiAngle = PhiAngle?.DeepClone(),
                PsiAngle = PsiAngle?.DeepClone(),
                IsFrozen = IsFrozen
            };
        }
    }
}
