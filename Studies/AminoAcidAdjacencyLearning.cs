using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChemistryLibrary.IO.Pdb;
using Commons.Extensions;
using NUnit.Framework;

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
            Parallel.ForEach(Directory.EnumerateFiles(inputDirectory, "*.ent"), pdbFile =>
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
