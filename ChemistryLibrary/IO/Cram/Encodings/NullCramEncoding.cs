using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class NullCramEncoding<T> : ICramEncoding<T>
    {
        public Codec CodecId => Codec.Null;

        public BitArray Encode(T item)
        {
            throw new System.NotSupportedException();
        }

        public T Decode(BitArray bits)
        {
            throw new System.NotSupportedException();
        }
    }
}