using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.Genomics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GenomeTools.Studies.GenomeAnalysis
{
    public class GeneVariant
    {
        public GeneVariant() {}
        public GeneVariant(
            string variantName,
            GenePosition genePosition,
            List<int> variantPositions)
        {
            Id = ObjectId.GenerateNewId();
            VariantName = variantName;
            GeneSymbol = genePosition.GeneSymbol;
            Chromosome = genePosition.Chromosome;
            StartIndex = genePosition.Position.From;
            EndIndex = genePosition.Position.To;
            VariantPositions = variantPositions;
        }

        [BsonId]
        public ObjectId Id { get; private set; }
        public string VariantName { get; private set; }
        public string GeneSymbol { get; private set; }
        public string Chromosome { get; private set; }
        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
        public List<int> VariantPositions { get; private set; }
    }
}
