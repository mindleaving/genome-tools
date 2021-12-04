using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GenomeTools.Studies.GenomeAnalysis
{
    public class AggregatedGeneVariantStatistics
    {
        public AggregatedGeneVariantStatistics() {}
        public AggregatedGeneVariantStatistics(List<GeneVariantStatistics> variantStatistics)
        {
            if (!variantStatistics.Any())
                throw new ArgumentException("List of gene variant statistics was empty");
            if (variantStatistics.Any(x => x.GeneSymbol != variantStatistics[0].GeneSymbol))
                throw new ArgumentException("Variant statistics weren't all for same gene");

            Id = ObjectId.GenerateNewId();
            GeneSymbol = variantStatistics[0].GeneSymbol;
            Chromosome = variantStatistics[0].Chromosome;
            StartIndex = variantStatistics[0].StartIndex;
            EndIndex = variantStatistics[0].EndIndex;
            GeneLength = variantStatistics[0].GeneLength;

            VariantCount = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => (double)x.VariantCount));
            DeletionCount = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => (double)x.DeletionCount));
            DeletionCountRatio = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => x.DeletionCountRatio));
            DeletionLength = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => (double)x.DeletionLength));
            DeletionLengthRatio = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => x.DeletionLengthRatio));
            InsertionCount = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => (double)x.InsertionCount));
            InsertionCountRatio = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => x.InsertionCountRatio));
            InsertionLength = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => (double)x.InsertionLength));
            InsertionLengthRatio = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => x.InsertionLengthRatio));
            SNPCount = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => (double)x.SNPCount));
            SNPRatio = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => x.SNPRatio));
            HeterogenousCount = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => (double)x.HeterogenousCount));
            HeterogenousCountRatio = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => x.HeterogenousCountRatio));
            TotalVariantLength = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => (double)x.TotalVariantLength));
            TotalVariantLengthRatio = ValueDistributionStatistics.Calculate(variantStatistics.Select(x => x.TotalVariantLengthRatio));
        }

        [BsonId]
        public ObjectId Id { get; private set; }
        public string GeneSymbol { get; private set; }
        public string Chromosome { get; private set; }
        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
        public int GeneLength { get; private set; }

        public ValueDistributionStatistics VariantCount { get; set; }
        public ValueDistributionStatistics DeletionCount { get; set; }
        public ValueDistributionStatistics DeletionCountRatio { get; set; }
        public ValueDistributionStatistics DeletionLength { get; set; }
        public ValueDistributionStatistics DeletionLengthRatio { get; set; }
        public ValueDistributionStatistics InsertionCount { get; set; }
        public ValueDistributionStatistics InsertionCountRatio { get; set; }
        public ValueDistributionStatistics InsertionLength { get; set; }
        public ValueDistributionStatistics InsertionLengthRatio { get; set; }
        public ValueDistributionStatistics SNPCount { get; set; }
        public ValueDistributionStatistics SNPRatio { get; set; }
        public ValueDistributionStatistics HeterogenousCount { get; set; }
        public ValueDistributionStatistics HeterogenousCountRatio { get; set; }
        public ValueDistributionStatistics TotalVariantLength { get; set; }
        public ValueDistributionStatistics TotalVariantLengthRatio { get; set; }
    }
}