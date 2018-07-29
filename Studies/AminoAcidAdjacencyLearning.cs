using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.IO.Pdb;
using Commons.Extensions;
using Commons.Physics;
using Domain;
using NUnit.Framework;
using Peptide = Domain.Peptide;

namespace Studies
{
    [TestFixture]
    public class AminoAcidAdjacencyLearning
    {
        [Test]
        [Ignore("Already done")]
        public void FilterPdbFiles()
        {
            var inputDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\wwPDB\pdb";
            var outputDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins";

            foreach (var directory in Directory.GetDirectories(inputDirectory))
            {
                var allPdbFiles = Directory.EnumerateFiles(directory, "*.ent");
                foreach (var pdbFile in allPdbFiles)
                {
                    var allText = File.ReadAllText(pdbFile);
                    if (!allText.Contains("HUMAN") && !allText.Contains("SAPIENS")) 
                        continue;
                    if(!allText.Contains("\nATOM"))
                        continue;
                    File.Copy(pdbFile, Path.Combine(outputDirectory, Path.GetFileName(pdbFile)));
                }
            }
        }

        [Test]
        public void NarrowDownToExplicitlyHumanProteins()
        {
            var inputDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins";
            var outputDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins2";

            foreach (var pdbFile in Directory.EnumerateFiles(inputDirectory))
            {
                var outputFilename = Path.Combine(outputDirectory, Path.GetFileName(pdbFile));
                if(File.Exists(outputFilename))
                {
                    File.Delete(pdbFile);
                    continue;
                }
                var allText = File.ReadAllText(pdbFile);
                if (!allText.Contains("ORGANISM_SCIENTIFIC: HOMO SAPIENS")) 
                    continue;
                File.Copy(pdbFile, outputFilename);
                File.Delete(pdbFile);
            }
        }

        [Test]
        public void PdbReadTest()
        {
            var inputDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins";
            Directory.CreateDirectory(Path.Combine(inputDirectory, "NoChain"));
            Directory.CreateDirectory(Path.Combine(inputDirectory, "SingleChain"));
            Directory.CreateDirectory(Path.Combine(inputDirectory, "MultiChain"));

            var cancellationTokenSource = new CancellationTokenSource();
            var files = Directory.EnumerateFiles(inputDirectory, "*.ent");
            Parallel.ForEach(files, pdbFile =>
            {
                //cancellationTokenSource.Token.ThrowIfCancellationRequested();
                try
                {
                    using (var pdbResult = PdbReader.ReadFile(pdbFile))
                    {
                        var maxChainCount = pdbResult.Models.Max(model => model.Chains.Count);
                        if (maxChainCount == 0)
                        {
                            File.Move(pdbFile, Path.Combine(inputDirectory, "NoChain", Path.GetFileName(pdbFile)));
                        }
                        else if (maxChainCount == 1)
                        {
                            File.Move(pdbFile, Path.Combine(inputDirectory, "SingleChain", Path.GetFileName(pdbFile)));
                        }
                        else
                        {
                            File.Move(pdbFile, Path.Combine(inputDirectory, "MultiChain", Path.GetFileName(pdbFile)));
                        }
                    }
                }
                catch
                {
                    File.Move(pdbFile, Path.Combine(inputDirectory, "Failed", Path.GetFileName(pdbFile)));
                    //cancellationTokenSource.Cancel();
                }
            });
            if(cancellationTokenSource.IsCancellationRequested)
                Assert.Fail();
            Assert.Pass();
        }

        [Test]
        public void FilterByPositionedMolecules()
        {
            var inputDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain";
            Directory.CreateDirectory(Path.Combine(inputDirectory, "FullyPositioned"));
            Directory.CreateDirectory(Path.Combine(inputDirectory, "PartiallyPositioned"));
            Directory.CreateDirectory(Path.Combine(inputDirectory, "NotPositioned"));

            var files = Directory.EnumerateFiles(inputDirectory, "*.ent");
            Parallel.ForEach(files, pdbFile =>
            {
                using (var pdbResult = PdbReader.ReadFile(pdbFile))
                {
                    foreach (var model in pdbResult.Models)
                    {
                        if(model.Chains.Count != 1)
                            continue;
                        var chain = model.Chains.Single();
                        var carbonAlphaAtoms = chain.Molecule.Atoms.Where(atom => atom.AminoAcidAtomName == "CA").ToList();
                        if (carbonAlphaAtoms.All(atom => atom.IsPositioned))
                        {
                            File.Move(pdbFile, Path.Combine(inputDirectory, "FullyPositioned", Path.GetFileName(pdbFile)));
                        }
                        else if (carbonAlphaAtoms.Any(atom => atom.IsPositioned))
                        {
                            File.Move(pdbFile, Path.Combine(inputDirectory, "PartiallyPositioned", Path.GetFileName(pdbFile)));
                        }
                        else
                        {
                            File.Move(pdbFile, Path.Combine(inputDirectory, "NotPositioned", Path.GetFileName(pdbFile)));
                        }
                        break;
                    }
                }
            });
        }

        [Test]
        public void FilterByMethod()
        {
            var inputDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned";
            Directory.CreateDirectory(Path.Combine(inputDirectory, "ByMethod"));
            var methodMap = new Dictionary<string, List<string>>
            {
                {"Other", new List<string>()}
            };
            var files = Directory.EnumerateFiles(inputDirectory, "*.ent");
            Parallel.ForEach(files, pdbFile =>
            {
                var lines = File.ReadAllLines(pdbFile);
                Match experimentTypeMatch = null;
                foreach (var line in lines)
                {
                    experimentTypeMatch = Regex.Match(line, "EXPDTA\\s+([A-Z].+)");
                    if (experimentTypeMatch.Success)
                        break;
                }

                if (experimentTypeMatch == null || !experimentTypeMatch.Success)
                {
                    methodMap["Other"].Add(pdbFile);
                    return;
                }
                var experimentMethod = experimentTypeMatch.Groups[1].Value.Trim().ToUpperInvariant();
                if (!methodMap.ContainsKey(experimentMethod))
                    methodMap.Add(experimentMethod, new List<string>());
                methodMap[experimentMethod].Add(pdbFile);
            });
            foreach (var kvp in methodMap)
            {
                var method = kvp.Key;
                File.WriteAllLines(
                    Path.Combine(inputDirectory, "ByMethod", $"{method}.csv"),
                    kvp.Value);
            }
        }

        [Test]
        public void FilterByProtein()
        {
            var inputDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned";
            var outputDirectory = Path.Combine(inputDirectory, "ByProtein");
            Directory.CreateDirectory(outputDirectory);
            Directory.EnumerateFiles(outputDirectory).ForEach(File.Delete);
            var moleculeMap = new ConcurrentDictionary<string, ConcurrentBag<string>>();
            moleculeMap.TryAdd("Other", new ConcurrentBag<string>());
            var files = Directory.EnumerateFiles(inputDirectory, "*.ent");
            Parallel.ForEach(files, pdbFile =>
            {
                var lines = File.ReadAllLines(pdbFile);
                Match moleculeNameMatch = null;
                foreach (var line in lines)
                {
                    moleculeNameMatch = Regex.Match(line, "COMPND.+MOLECULE.*:([^;]+).*");
                    if (moleculeNameMatch.Success)
                        break;
                }

                if (moleculeNameMatch == null || !moleculeNameMatch.Success)
                {
                    moleculeMap["Other"].Add(pdbFile);
                    return;
                }
                var moleculeName = moleculeNameMatch.Groups[1].Value.Trim().ToUpperInvariant();
                if (!moleculeMap.ContainsKey(moleculeName))
                    moleculeMap.TryAdd(moleculeName, new ConcurrentBag<string>());
                moleculeMap[moleculeName].Add(pdbFile);
            });
            foreach (var kvp in moleculeMap)
            {
                var moleculeName = kvp.Key;
                moleculeName = Regex.Replace(moleculeName,"[^A-Z0-9-,\\s]","_");
                moleculeName = Regex.Replace(moleculeName, "\\s+", " ");
                File.WriteAllLines(
                    Path.Combine(outputDirectory, $"{moleculeName}.csv"),
                    kvp.Value);
            }
        }

        [Test]
        public void ExtractAminoAcidPositions()
        {
            var inputDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned";
            var failedDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\Failed";
            var outputDirection = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions";
            var files = Directory.EnumerateFiles(inputDirectory, "pdb5uak.ent");
            Parallel.ForEach(files, pdbFile =>
            {
                try
                {
                    using (var pdbResult = PdbReader.ReadFile(pdbFile))
                    {
                        for (var modelIdx = 0; modelIdx < pdbResult.Models.Count; modelIdx++)
                        {
                            var model = pdbResult.Models[modelIdx];
                            if (model.Chains.Count != 1)
                                continue;
                            var chain = model.Chains.Single();
                            var carbonAlphaAtoms = chain.Molecule.Atoms.Where(atom => atom.AminoAcidAtomName == "CA").ToList();
                            if (!carbonAlphaAtoms.All(atom => atom.IsPositioned))
                                continue;
                            var lines = new List<string>();
                            var allPositioned = true;
                            foreach (var aminoAcid in chain.AminoAcids)
                            {
                                var carbonAlpha = aminoAcid.GetAtomFromName("CA");
                                if (carbonAlpha == null || !carbonAlpha.IsPositioned)
                                {
                                    allPositioned = false;
                                    break;
                                }
                                lines.Add($"{aminoAcid.Name.ToOneLetterCode()};{carbonAlpha.Position.In(SIPrefix.Pico, Unit.Meter)}");
                            }
                            if(!allPositioned)
                                continue;
                            File.WriteAllLines(
                                Path.Combine(outputDirection, $"{Path.GetFileNameWithoutExtension(pdbFile)}_model{modelIdx:D3}.csv"),
                                lines);
                        }
                    }
                }
                catch
                {
                    File.Move(pdbFile, Path.Combine(failedDirectory, Path.GetFileName(pdbFile)));
                }
            });
        }

        [Test]
        public void SequenceLengthHistogramFromPositionFiles()
        {
            var directory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions";
            var files = Directory.EnumerateFiles(directory, "*model000.csv");
            var histogram = new Dictionary<int, int>();
            foreach (var file in files)
            {
                var lineCount = File.ReadLines(file).Count();
                if(!histogram.ContainsKey(lineCount))
                    histogram.Add(lineCount, 0);
                histogram[lineCount]++;
            }
            Console.WriteLine(
                histogram
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => $"{kvp.Key.ToString().PadRight(5,' ')}\t{kvp.Value}")
                .Aggregate((a,b) => a + Environment.NewLine + b)
            );
        }

        [Test]
        public void PdbIdsWithSequenceLength()
        {
            var sequenceLength = 115;
            var directory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions";
            var files = Directory.EnumerateFiles(directory, "*.csv");
            var outputDirectory = Path.Combine(directory, $"SequenceLength {sequenceLength:0000}");
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            foreach (var file in files)
            {
                var lineCount = File.ReadLines(file).Count();
                if(lineCount != sequenceLength)
                    continue;
                File.Copy(file, Path.Combine(outputDirectory, Path.GetFileName(file)), true);
            }
        }

        [Test]
        public void FullyPositionedSingleChainPeptideSequences()
        {
            // Similar to PeptideFrequencyStudy.FindCommonPeptideSequences,
            // but restricted to peptides where we know the positions of the amino acids in the protein
            var positionFilesDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions";
            var outputDirectory = @"G:\Projects\HumanGenome\sequenceFrequencies\FullyPositionedSingleChainPeptides";
            var positionFiles = Directory.EnumerateFiles(positionFilesDirectory, "*_model000.csv");
            var peptides = new List<Peptide>();
            foreach (var positionFile in positionFiles)
            {
                var peptideSequence = File.ReadAllLines(positionFile)
                    .Where(line => !string.IsNullOrEmpty(line))
                    .Select(line => line[0])
                    .ToList();
                var peptide = new Peptide();
                peptide.Sequence.AddRange(peptideSequence);                
                peptides.Add(peptide);
            }
            PeptideFrequencyAnalysis.Analyze(peptides, outputDirectory);
        }

        [Test]
        public void FilterPositionFilesBySubsequence()
        {
            var subsequence = "CAQYWPQKEEKEMIFEDTNLKLTLISEDIK";
            var positionFilesDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions";
            var outputDirectory = $@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions\sequence_{subsequence}";
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            var positionFiles = Directory.EnumerateFiles(positionFilesDirectory, "*_model000.csv");
            foreach (var positionFile in positionFiles)
            {
                var peptideSequence = File.ReadAllLines(positionFile)
                    .Where(line => !string.IsNullOrEmpty(line))
                    .Select(line => line[0])
                    .ToArray();
                var sequenceString = new string(peptideSequence).ToUpperInvariant();
                if(sequenceString.Contains(subsequence))
                    File.Copy(positionFile, Path.Combine(outputDirectory, Path.GetFileName(positionFile)));
            }
        }

        [Test]
        public void PdbReaderDebug()
        {
            var inputDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\Failed";

            foreach (var pdbFile in Directory.EnumerateFiles(inputDirectory, "*.ent"))
            {
                PdbReader.ReadFile(pdbFile);
                File.Delete(pdbFile);
            }
        }
    }
}
