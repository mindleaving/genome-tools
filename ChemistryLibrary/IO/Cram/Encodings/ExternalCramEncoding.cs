using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class ExternalCramEncoding<T> : ICramEncoding<T>
    {
        public ExternalCramEncoding(int blockContentId)
        {
            BlockContentId = blockContentId;
        }

        public Codec CodecId => Codec.External;
        public int BlockContentId { get; }

        public void Encode(T item, BitStream stream)
        {
            throw new System.NotImplementedException();
        }

        public T Decode(BitStream bits)
        {
            throw new System.NotImplementedException();
        }
    }
}