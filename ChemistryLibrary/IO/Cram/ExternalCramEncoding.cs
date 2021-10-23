using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class ExternalCramEncoding<T> : ICramEncoding<T>
    {
        public ExternalCramEncoding(int blockContentId)
        {
            BlockContentId = blockContentId;
        }

        public Codec CodecId => Codec.External;
        public int BlockContentId { get; }

        public BitArray Encode(T item)
        {
            throw new System.NotImplementedException();
        }

        public T Decode(BitArray bits)
        {
            throw new System.NotImplementedException();
        }
    }
}