using System;

namespace ChemistryLibrary.Objects
{
    [Flags]
    public enum Nucleotide : byte
    {
        Z = byte.MinValue,
        A = 1,
        C = 2,
        G = 4,
        T = 8,
        W = A | T,
        S = C | G,
        M = A | C,
        K = G | T,
        R = A | G,
        Y = C | T,
        B = C | G | T,
        D = A | G | T,
        H = A | C | T,
        V = A | C | G,
        N = A | C | G | T
    }
}
