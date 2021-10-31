using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class NullCramEncoding<T> : ICramEncoding<T>
    {
        public Codec CodecId => Codec.Null;

        public void Encode(T item, BitStream stream)
        {
            throw new System.NotSupportedException();
        }

        public T Decode(BitStream bits)
        {
            throw new System.NotSupportedException();
        }
    }
}