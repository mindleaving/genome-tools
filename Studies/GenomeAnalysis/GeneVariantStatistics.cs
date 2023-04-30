using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.IO.Vcf;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GenomeTools.Studies.GenomeAnalysis
{
    public class GeneVariantStatistics
    {
        public GeneVariantStatistics() {}
        public GeneVariantStatistics(
            string personId,
            GeneParentalOrigin parentalOrigin,
            GenePosition genePosition, 
            List<VcfVariantEntry> variants)
            : this(personId, parentalOrigin, genePosition)
        {
            VariantCount = variants.Count;
            VariantPositions = variants.Select(x => x.Position).ToList();
            DeletionCount = variants.Count(x => x.IsDeletion);
            DeletionCountRatio = variants.Count > 0 ? DeletionCount / (double)variants.Count : 0;
            DeletionLength = variants.Where(x => x.IsDeletion).Sum(x => x.LongestDeletionLength);
            DeletionLengthRatio = DeletionLength / (double) GeneLength;
            InsertionCount = variants.Count(x => x.IsInsertion);
            InsertionCountRatio = variants.Count > 0 ? InsertionCount / (double)variants.Count : 0;
            InsertionLength = variants.Where(x => x.IsInsertion).Sum(x => x.LongestInsertionLength);
            InsertionLengthRatio = InsertionLength / (double) GeneLength;
            SNPCount = variants.Count(x => x.IsSNP);
            SNPRatio = variants.Count > 0 ? SNPCount / (double)variants.Count : 0;
            HeterogenousCount = variants.Count(x => x.OtherFields[personId][0] != x.OtherFields[personId][2]); // TODO: Makes a lot of assumptions
            HeterogenousCountRatio = variants.Count > 0 ? HeterogenousCount / (double)variants.Count : 0;
            TotalVariantLength = SNPCount + InsertionLength + DeletionLength;
            TotalVariantLengthRatio = TotalVariantLength / (double)GeneLength;
        }

        public GeneVariantStatistics(
            string personId, 
            GeneParentalOrigin parentalOrigin,
            GenePosition genePosition)
        {
            Id = ObjectId.GenerateNewId();
            PersonId = personId;
            ParentalOrigin = parentalOrigin;
            GeneSymbol = genePosition.GeneSymbol;
            Chromosome = genePosition.Chromosome;
            StartIndex = genePosition.Position.From;
            EndIndex = genePosition.Position.To;
            GeneLength = genePosition.Position.To - genePosition.Position.From + 1;
            VariantPositions = new List<int>();
        }

        public void UpdateRatios()
        {
            DeletionCountRatio = VariantCount > 0 ? DeletionCount / (double)VariantCount : 0;
            DeletionLengthRatio = DeletionLength / (double) GeneLength;
            InsertionCountRatio = VariantCount > 0 ? InsertionCount / (double)VariantCount : 0;
            InsertionLengthRatio = InsertionLength / (double) GeneLength;
            SNPRatio = VariantCount > 0 ? SNPCount / (double)VariantCount : 0;
            HeterogenousCountRatio = VariantCount > 0 ? HeterogenousCount / (double)VariantCount : 0;
            TotalVariantLengthRatio = TotalVariantLength / (double)GeneLength;
        }

        [BsonId]
        public ObjectId Id { get; private set; }
        public string PersonId { get; private set; }
        public GeneParentalOrigin ParentalOrigin { get; private set; }
        public string GeneSymbol { get; private set; }
        public string Chromosome { get; private set; }
        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
        public int GeneLength { get; private set; }

        public List<int> VariantPositions { get; private set; }
        public int VariantCount { get; set; }
        public int DeletionCount { get; set; }
        public double DeletionCountRatio { get; private set; }
        public int DeletionLength { get; set; }
        public double DeletionLengthRatio { get; private set; }
        public int InsertionCount { get; set; }
        public double InsertionCountRatio { get; private set; }
        public int InsertionLength { get; set; }
        public double InsertionLengthRatio { get; private set; }
        public int SNPCount { get; set; }
        public double SNPRatio { get; private set; }
        public int HeterogenousCount { get; set; }
        public double HeterogenousCountRatio { get; private set; }
        public int TotalVariantLength { get; set; }
        public double TotalVariantLengthRatio { get; private set; }
    }

    public enum GeneParentalOrigin
    {
        Unknown = 0,
        Parent1 = 1,
        Parent2 = 2
    }
}