using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public interface ICramEncoding<T>
    {
        Codec CodecId { get; }
        void Encode(T item, BitStream stream);
        T Decode(BitStream bits);
    }
}