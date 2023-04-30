using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GenomeTools.Studies.GenomeAnalysis
{
    public class GenomeSequenceVariant
    {
        public GenomeSequenceVariant() {}
        public GenomeSequenceVariant(
            string personId, 
            string chromosome, 
            int referenceStartIndex,
            int referenceEndIndex, 
            string referenceSequence, 
            string primaryVariantSequence,
            string secondaryVariantSequence)
        {
            Id = ObjectId.GenerateNewId();
            PersonId = personId;
            Chromosome = chromosome;
            ReferenceStartIndex = referenceStartIndex;
            ReferenceEndIndex = referenceEndIndex;
            ReferenceSequence = referenceSequence;
            PrimaryVariantSequence = primaryVariantSequence;
            SecondaryVariantSequence = secondaryVariantSequence;
        }

        [BsonId]
        public ObjectId Id { get; set; }
        public string PersonId { get; set; }
        public string Chromosome { get; set; }
        public int ReferenceStartIndex { get; set; }
        public int ReferenceEndIndex { get; set; }
        public string ReferenceSequence { get; set; }
        public string PrimaryVariantSequence { get; set; }
        public string SecondaryVariantSequence { get; set; }
        [BsonElement]
        public bool IsHeterogenous => SecondaryVariantSequence != null && PrimaryVariantSequence != SecondaryVariantSequence;
    }
}
