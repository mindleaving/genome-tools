using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChemistryLibrary.IO.Pdb;
using ChemistryLibrary.Objects;
using NUnit.Framework;
using Tools;

namespace Studies
{
    public partial class AlphaHelixPairStudy
    {
        [Test]
        public void ExtractAndAnnotateHelixSequences()
        {
            var directory = @"F:\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\ByProtein";
            var outputFilePath = @"F:\HumanGenome\Protein-PDBs\HumanProteins\humanFullyPositionedSingleChainUniqueProteinHelixMarked.txt";
            var pdbLookupFiles = Directory.EnumerateFiles(directory, "*.csv");
            var pdbReaderOptions = new PdbReaderOptions {MaximumModelCount = 1, BuildMolecule = false};
            var outputLock = new object();
            File.Delete(outputFilePath);
            Parallel.ForEach(pdbLookupFiles, pdbLookupFile =>
            {
                var pdbFilePaths = File.ReadAllLines(pdbLookupFile);
                var proteinOutput = new List<string>();
                var maxProteinAminoAcidCount = 0;
                foreach (var pdbFilePath in pdbFilePaths)
                {
                    try
                    {
                        var pdbResult = PdbReader.ReadFile(pdbFilePath, pdbReaderOptions);
                        if (!pdbResult.Models.Any()) 
                            return;
                        var firstModel = pdbResult.Models.First();
                        if(!firstModel.Chains.Any()) 
                            return;
                        var hasHelixAnnotations = firstModel.Chains
                            .SelectMany(chain => chain.Annotations)
                            .Any(annotation => annotation.Type == PeptideSecondaryStructure.AlphaHelix);
                        if(!hasHelixAnnotations)
                            return;
                        var aminoAcidCount = firstModel.Chains.Sum(chain => chain.AminoAcids.Count);
                        if (aminoAcidCount <= maxProteinAminoAcidCount)
                            return;

                        proteinOutput.Clear();
                        proteinOutput.Add("#" + Path.GetFileNameWithoutExtension(pdbFilePath));
                        foreach (var chain in pdbResult.Models.First().Chains)
                        {
                            var helixAnnotations = chain.Annotations.Where(annot => annot.Type == PeptideSecondaryStructure.AlphaHelix).ToList();
                            var fullSequence = AlphaHelixAnnotationTool.GetFullSequence(chain, helixAnnotations);
                            //var helixSequence = AlphaHelixAnnotationTool.GetHelixSequences(helixAnnotations);
                            proteinOutput.Add(fullSequence);
                        }

                        maxProteinAminoAcidCount = aminoAcidCount;
                    }
                    catch (Exception e)
                    {
                        var errorMessage = $"Exception: {e.Message}(Path: {pdbFilePath})";
                        Console.WriteLine(errorMessage);
                        File.AppendAllLines(@"C:\Temp\errors.txt", new []{ errorMessage });
                    }
                }
                lock(outputLock)
                {
                    File.AppendAllLines(outputFilePath, proteinOutput);
                }
            });
        }
    }
}