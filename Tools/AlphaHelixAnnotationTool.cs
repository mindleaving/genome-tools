using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.IO.Pdb;
using ChemistryLibrary.Objects;
using NUnit.Framework;

namespace Tools
{
    [TestFixture]
    public class AlphaHelixAnnotationTool
    {
        /// <summary>
        /// Generate annotation data for AlphaHelixStrengthStudy
        /// </summary>
        [Test]
        [TestCase(
            @"G:\Projects\HumanGenome\Protein-PDBs", 
            "*.pdb", 
            @"G:\Projects\HumanGenome\fullPdbSequencesHelixMarked.txt"
        )]
        [TestCase(
            @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain", 
            "*.ent",
            @"G:\Projects\HumanGenome\humanSingleChainHelixMarked.txt"
        )]
        public void AlphaHelixOuput(string directory, string filter, string outputFilePath)
        {
            var pdbFiles = Directory.GetFiles(directory, filter, SearchOption.AllDirectories);
            var output = new List<string>();
            foreach (var pdbFile in pdbFiles)
            {
                ExtractFullSequenceFromFile(pdbFile, output);
            }
            File.WriteAllLines(outputFilePath, output);
        }

        public static void ExtractFullSequenceFromFile(
            string pdbFile,
            List<string> output)
        {
            try
            {
                var pdbResult = PdbReader.ReadFile(pdbFile);
                if (!pdbResult.Models.Any() || !pdbResult.Models.First().Chains.Any())
                    return;
                output.Add("#" + Path.GetFileNameWithoutExtension(pdbFile));
                foreach (var chain in pdbResult.Models.First().Chains)
                {
                    var helixAnnotations = chain.Annotations.Where(annot => annot.Type == PeptideSecondaryStructure.AlphaHelix).ToList();
                    var fullSequence = GetFullSequence(chain, helixAnnotations);
                    //var helixSequence = GetHelixSequences(helixAnnotations);
                    output.Add(fullSequence);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        public static string GetFullSequence(Peptide chain, List<PeptideAnnotation<AminoAcidReference>> helixAnnotations)
        {
            var fullSequence = "";
            var aminoAcidCount = chain.AminoAcids.Last().SequenceNumber;
            var aminoAcidQueue = new Queue<AminoAcidReference>(chain.AminoAcids);
            var nonEmptyHelixAnnotations = helixAnnotations.Where(annotation => annotation.AminoAcidReferences.Any()).ToList();
            for (int aminoAcidIndex = 1; aminoAcidIndex <= aminoAcidCount; aminoAcidIndex++)
            {
                while (aminoAcidQueue.Peek().SequenceNumber < aminoAcidIndex)
                {
                    aminoAcidQueue.Dequeue();
                }
                var nextAminoAcid = aminoAcidQueue.Peek();
                if (nextAminoAcid.SequenceNumber == aminoAcidIndex)
                {
                    var currentAminoAcid = aminoAcidQueue.Dequeue();
                    if (nonEmptyHelixAnnotations.Any(annotation => annotation.AminoAcidReferences.First() == currentAminoAcid))
                        fullSequence += "[";
                    fullSequence += currentAminoAcid.Name.ToOneLetterCode();
                    if (nonEmptyHelixAnnotations.Any(annotation => annotation.AminoAcidReferences.Last() == currentAminoAcid))
                        fullSequence += "]";
                }
                else
                {
                    fullSequence += "?";
                }
            }
            return fullSequence;
        }

        public IEnumerable<string> GetHelixSequences(List<PeptideAnnotation<AminoAcidReference>> helixAnnotations)
        {
            foreach (var annotation in helixAnnotations)
            {
                var sequence = annotation.AminoAcidReferences.Select(aa => aa.Name.ToOneLetterCode()).ToArray();
                yield return new string(sequence);
            }
        }
    }
}
