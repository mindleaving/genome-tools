using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.Genomics.Alignment;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.IO.Vcf;
using GenomeTools.Studies.GenomeAnalysis;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    public class HlaStudy
    {
        private const string VariantFilePath = @"F:\datasets\mygenome\genome-janscholtyssek.vcf";
        private const string AlignmentFilePath = @"F:\datasets\mygenome\genome-janscholtyssek.cram";
        private const string ReferenceSequenceFilePath = @"F:\datasets\mygenome\references\hg38.p13.fa";
        private const string GenePositionFilePath = @"F:\HumanGenome\gene_positions.csv";
        private const string HlaAllelDirectory = @"F:\datasets\mygenome\HLA\Allels";
        private const string OutputDirectory = @"F:\HumanGenome\HLA";
        private const string GeneVariantDatabaseName = "Genomics";

        [Test]
        [TestCase("HLA-A")]
        [TestCase("HLA-B")]
        [TestCase("HLA-C")]
        [TestCase("HLA-DMA")]
        [TestCase("HLA-DMB")]
        [TestCase("HLA-DOA")]
        [TestCase("HLA-DOB")]
        [TestCase("HLA-DPA1")]
        [TestCase("HLA-DPB1")]
        [TestCase("HLA-DQA1")]
        [TestCase("HLA-DQA2")]
        [TestCase("HLA-DQB1")]
        [TestCase("HLA-DQB2")]
        [TestCase("HLA-DRA")]
        [TestCase("HLA-DRB1")]
        [TestCase("HLA-DRB5")]
        [TestCase("HLA-E")]
        [TestCase("HLA-F")]
        [TestCase("HLA-G")]
        public async Task DetermineHlaAllels(string geneSymbol)
        {
            //var referenceAccessor = new GenomeSequenceAccessor(ReferenceSequenceFilePath, null);
            var genePositions = GenePositionStudy.ReadGenePositions(GenePositionFilePath);
            var hlaGenePosition = genePositions.First(x => x.GeneSymbol == geneSymbol);
            var sequenceNameTranslation = Enumerable.Range(1, 22).Select(x => x.ToString()).Concat(new[] { "X", "Y", "M" }).ToDictionary(x => $"chr{x}", x => x);
            var variantLoader = new VcfAccessor(VariantFilePath, sequenceNameTranslation);
            var geneVariantDb = new GeneVariantDb(GeneVariantDatabaseName);

            var myVariants = variantLoader.LoadInRange(hlaGenePosition);
            var myVariantPositions = myVariants.Variants.Select(x => x.Position).ToList();
            var hlaVariants = await geneVariantDb.GetGeneVariants(geneSymbol);
            var bestMatches = hlaVariants.Select(hlaVariant => new
                {
                    HlaVariant = hlaVariant,
                    NumberOfMatches = hlaVariant.VariantPositions.Intersect(myVariantPositions).Count()
                })
                .OrderByDescending(x => x.NumberOfMatches)
                .ToList();

            Console.WriteLine("My variant positions:");
            string FormatVariant(VcfVariantEntry variant)
            {
                var suffix = variant.IsDeletion ? "D"
                    : variant.IsInsertion ? "I"
                    : string.Empty;
                return $"{variant.Position}{suffix}";
            }
            Console.WriteLine(string.Join(", ", myVariants.Variants.Select(FormatVariant)));
            Console.WriteLine();
            Console.WriteLine("Best matching HLA allels:");
            foreach (var match in bestMatches)
            {
                Console.WriteLine($"{match.HlaVariant.VariantName}: {match.NumberOfMatches}");
            }
        }
        
        [Test]
        [TestCase("HLA-A")]
        [TestCase("HLA-B")]
        [TestCase("HLA-C")]
        [TestCase("HLA-DMA")]
        [TestCase("HLA-DMB")]
        [TestCase("HLA-DOA")]
        [TestCase("HLA-DOB")]
        [TestCase("HLA-DPA1")]
        [TestCase("HLA-DPB1")]
        [TestCase("HLA-DQA1")]
        [TestCase("HLA-DQA2")]
        [TestCase("HLA-DQB1")]
        [TestCase("HLA-DQB2")]
        [TestCase("HLA-DRA")]
        [TestCase("HLA-DRB1")]
        [TestCase("HLA-DRB5")]
        [TestCase("HLA-E")]
        [TestCase("HLA-F")]
        [TestCase("HLA-G")]
        public async Task ExtractReferenceHlaLociToFasta(string geneSymbol)
        {
            var referenceAccessor = new GenomeSequenceAccessor(ReferenceSequenceFilePath, null);
            var genePositions = GenePositionStudy.ReadGenePositions(GenePositionFilePath);
            var hlaGenePosition = genePositions.First(x => x.GeneSymbol == geneSymbol);
            var referenceSequence = referenceAccessor.GetSequenceByName($"chr{hlaGenePosition.Chromosome}", hlaGenePosition.Position.From, hlaGenePosition.Position.To);
            await File.WriteAllLinesAsync(
                Path.Combine(OutputDirectory, geneSymbol + ".fa"),
                new []
                {
                    $">{geneSymbol} chr{hlaGenePosition.Chromosome}:{hlaGenePosition.Position.From}:{hlaGenePosition.Position.To}"
                }.Concat(referenceSequence.GetSequence().SplitAtLength(60)));
        }

        [Test]
        [TestCase("HLA-A", ReferenceDnaStrandRole.Coding)]
        [TestCase("HLA-B", ReferenceDnaStrandRole.NonCoding)]
        [TestCase("HLA-C", ReferenceDnaStrandRole.NonCoding)]
        [TestCase("HLA-DMA", ReferenceDnaStrandRole.NonCoding)]
        [TestCase("HLA-DMB", ReferenceDnaStrandRole.NonCoding)]
        [TestCase("HLA-DOA", ReferenceDnaStrandRole.NonCoding)]
        [TestCase("HLA-DOB", ReferenceDnaStrandRole.NonCoding)]
        [TestCase("HLA-DPA1", ReferenceDnaStrandRole.NonCoding)]
        [TestCase("HLA-DPB1", ReferenceDnaStrandRole.Coding)]
        [TestCase("HLA-DQA1", ReferenceDnaStrandRole.Coding)]
        [TestCase("HLA-DQA2", ReferenceDnaStrandRole.Coding)]
        [TestCase("HLA-DQB1", ReferenceDnaStrandRole.NonCoding)]
        //[TestCase("HLA-DQB2", ReferenceDnaStrandRole.NonCoding)] // No allel file available
        [TestCase("HLA-DRA", ReferenceDnaStrandRole.Coding)]
        [TestCase("HLA-DRB1", ReferenceDnaStrandRole.NonCoding)]
        [TestCase("HLA-DRB5", ReferenceDnaStrandRole.NonCoding)]
        [TestCase("HLA-E", ReferenceDnaStrandRole.Coding)]
        [TestCase("HLA-F", ReferenceDnaStrandRole.Coding)]
        [TestCase("HLA-G", ReferenceDnaStrandRole.Coding)]
        public void CompareHlaVariantsToReference(string geneSymbol, ReferenceDnaStrandRole referenceDnaStrandRole)
        {
            var genePosition = GetGenePosition(geneSymbol);
            var referenceSequence = GetReferenceSequenceForGenePosition(genePosition).GetSequence();
            if (referenceDnaStrandRole == ReferenceDnaStrandRole.NonCoding)
                referenceSequence = SequenceInverter.ComplementaryDNA(referenceSequence);

            var allelFilePath = Path.Combine(HlaAllelDirectory, $"{geneSymbol.Substring(4)}_gen.fasta");
            var sequenceAccessor = new GenomeSequenceAccessor(allelFilePath, null);
            var sequenceNames = sequenceAccessor.GetSequenceNames();
            var sequenceAligner = new LongSequenceAligner();
            var outputFilePath = Path.Combine(OutputDirectory, $"sequenceAlignments_{geneSymbol}.txt");
            if(File.Exists(outputFilePath))
                File.Delete(outputFilePath);
            var geneVariantDb = new GeneVariantDb(GeneVariantDatabaseName);

            var poorMatchCount = 0;
            foreach (var sequenceName in sequenceNames)
            {
                var sequenceToBeAligned = sequenceAccessor.GetSequenceByName(sequenceName).GetSequence();
                var alignedRegions = sequenceAligner.Align(referenceSequence, sequenceToBeAligned);

                var matchLength = alignedRegions.OfType<MatchedAlignmentRegion>().Sum(x => x.AlignmentLength);
                if (matchLength < 0.8 * sequenceToBeAligned.Length)
                    poorMatchCount++;

                var variantPositions = alignedRegions
                    .OfType<MismatchAlignmentRegion>()
                    //.Where(x => x.Type != AlignmentRegionType.Match)
                    .Select(x => referenceDnaStrandRole == ReferenceDnaStrandRole.Coding 
                        ? genePosition.Position.From + x.ReferenceStartIndex
                        : genePosition.Position.To - x.ReferenceEndIndex)
                    .ToList();
                var geneVariant = new GeneVariant(sequenceName, genePosition, variantPositions);
                geneVariantDb.Store(geneVariant).Wait();

                OutputAlignedSequence(
                    referenceSequence, 
                    sequenceToBeAligned, 
                    alignedRegions,
                    sequenceName,
                    outputFilePath);
            }
            if(poorMatchCount > 0.6*sequenceNames.Count)
                throw new Exception("HLA-sequence is poorly matched to reference. Are you using the correct reference strand role (coding/non-coding)?");
        }

        private static void OutputAlignedSequence(
            string referenceSequence, string sequenceToBeAligned, List<IAlignmentRegion> alignedRegions,
            string sequenceName, string outputFilePath)
        {
            var alignedSequence = new char[referenceSequence.Length];
            Array.Fill(alignedSequence, ' ');
            var mismatchSequence = new char[referenceSequence.Length];
            Array.Fill(mismatchSequence, ' ');
            foreach (var alignmentRegion in alignedRegions)
            {
                if (alignmentRegion is MatchedAlignmentRegion matchedRegion)
                {
                    for (int i = 0; i < matchedRegion.AlignmentLength; i++)
                    {
                        alignedSequence[matchedRegion.ReferenceStartIndex + i] = sequenceToBeAligned[matchedRegion.AlignedSequenceStartIndex + i];
                    }
                }
                else if (alignmentRegion is MismatchAlignmentRegion mismatchedRegion)
                {
                    for (int referenceIndex = mismatchedRegion.ReferenceStartIndex; referenceIndex <= mismatchedRegion.ReferenceEndIndex; referenceIndex++)
                    {
                        var alignmentSequenceIndex = mismatchedRegion.AlignedSequenceStartIndex + referenceIndex - mismatchedRegion.ReferenceStartIndex;
                        if (alignmentSequenceIndex < 0)
                            continue;
                        if (alignmentSequenceIndex >= sequenceToBeAligned.Length)
                            break;
                        mismatchSequence[referenceIndex] = sequenceToBeAligned[alignmentSequenceIndex];
                    }
                }
            }

            var lines = new[] { $"Sequence {sequenceName}", referenceSequence, new string(alignedSequence), new string(mismatchSequence), string.Empty };
            //lines.ForEach(Console.WriteLine);
            File.AppendAllLines(outputFilePath, lines);
        }

        private static IGenomeSequence GetReferenceSequenceForGenePosition(GenePosition genePosition)
        {
            var referenceAccessor = new GenomeSequenceAccessor(ReferenceSequenceFilePath, null);
            var referenceSequence = referenceAccessor.GetSequenceByName($"chr{genePosition.Chromosome}", genePosition.Position.From, genePosition.Position.To);
            return referenceSequence;
        }

        private static GenePosition GetGenePosition(string geneSymbol)
        {
            var genePositions = GenePositionStudy.ReadGenePositions(GenePositionFilePath);
            var hlaGenePosition = genePositions.First(x => x.GeneSymbol == geneSymbol);
            return hlaGenePosition;
        }
    }
}
