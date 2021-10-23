using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public interface ICramEncoding<T>
    {
        Codec CodecId { get; }
        BitArray Encode(T item);
        T Decode(BitArray bits);
    }
}