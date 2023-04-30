using System;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Extensions
{
    public static class NucleotideExtensions
    {
        public static Nucleotide ToComplement(this Nucleotide nucleotide)
        {
            switch (nucleotide)
            {
                case Nucleotide.Z:
                    return Nucleotide.Z;
                case Nucleotide.A:
                    return Nucleotide.T;
                case Nucleotide.C:
                    return Nucleotide.G;
                case Nucleotide.G:
                    return Nucleotide.C;
                case Nucleotide.T:
                    return Nucleotide.A;
                case Nucleotide.W:
                    return Nucleotide.W;
                case Nucleotide.S:
                    return Nucleotide.S;
                case Nucleotide.M:
                    return Nucleotide.K;
                case Nucleotide.K:
                    return Nucleotide.M;
                case Nucleotide.R:
                    return Nucleotide.Y;
                case Nucleotide.Y:
                    return Nucleotide.R;
                case Nucleotide.B:
                    return Nucleotide.V;
                case Nucleotide.D:
                    return Nucleotide.H;
                case Nucleotide.H:
                    return Nucleotide.D;
                case Nucleotide.V:
                    return Nucleotide.B;
                case Nucleotide.N:
                    return Nucleotide.N;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nucleotide), nucleotide, null);
            }
        }

        public static bool IsComplementaryMatch(
            Nucleotide nucleotide1,
            Nucleotide nucleotide2)
        {
            var complement = nucleotide1.ToComplement();
            return nucleotide2 == complement;
        }
    }
}
