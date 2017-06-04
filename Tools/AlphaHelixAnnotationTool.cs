using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Objects;
using ChemistryLibrary.Pdb;
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
        public void AlphaHelixOuput()
        {
            var pdbFiles = Directory.GetFiles(@"G:\Projects\HumanGenome\Protein-PDBs", "*.pdb");
            var output = new List<string>();
            foreach (var pdbFile in pdbFiles)
            {
                try
                {
                    var pdbResult = PdbReader.ReadFile(pdbFile);
                    if (!pdbResult.Chains.Any())
                        continue;
                    output.Add("#" + Path.GetFileNameWithoutExtension(pdbFile));
                    foreach (var chain in pdbResult.Chains)
                    {
                        var helixAnnotations = chain.Annotations
                            .Where(annot => annot.Type == PeptideSecondaryStructure.AlphaHelix)
                            .ToList();
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
            File.WriteAllLines(@"G:\Projects\HumanGenome\fullPdbSequencesHelixMarked.txt", output);
        }

        private static string GetFullSequence(Peptide chain, List<PeptideAnnotation> helixAnnotations)
        {
            var fullSequence = "";
            foreach (var aminoAcid in chain.AminoAcids)
            {
                if (helixAnnotations.Any(annotation => annotation.AminoAcidReferences.First() == aminoAcid))
                    fullSequence += "[";
                fullSequence += aminoAcid.Name.ToOneLetterCode();
                if (helixAnnotations.Any(annotation => annotation.AminoAcidReferences.Last() == aminoAcid))
                    fullSequence += "]";
            }
            return fullSequence;
        }

        private IEnumerable<string> GetHelixSequences(List<PeptideAnnotation> helixAnnotations)
        {
            foreach (var annotation in helixAnnotations)
            {
                var sequence = annotation.AminoAcidReferences.Select(aa => aa.Name.ToOneLetterCode()).ToArray();
                yield return new string(sequence);
            }
        }
    }
}
