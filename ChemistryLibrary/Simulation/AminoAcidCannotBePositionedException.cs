using System;
using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Simulation
{
    public class AminoAcidCannotBePositionedException : Exception
    {
        public AminoAcidCannotBePositionedException(AminoAcidName aminoAcidName,
            int sequenceNumber)
        {
            AminoAcidName = aminoAcidName;
            SequenceNumber = sequenceNumber;
        }

        public AminoAcidName AminoAcidName { get; }
        public int SequenceNumber { get; }
    }
}