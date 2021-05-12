using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    [TestFixture]
    public class PeptideSequenceSimilarityStudy
    {
        [Test]
        public void SortPeptideSequence()
        {
            var peptideFile = @"F:\HumanGenome\Homo_sapiens.GRCh38.pep.all.fa";
            var peptides = new Dictionary<string, string>();
            using (var streamReader = new StreamReader(peptideFile))
            {
                string header = null;
                string sequence = null;
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.StartsWith(">"))
                    {
                        if (header != null && !string.IsNullOrEmpty(sequence))
                        {
                            peptides.Add(header, sequence);
                        }
                        header = line;
                        sequence = "";
                    }
                    else
                    {
                        sequence += line.Replace(" ", "");
                    }
                }
                if (header != null && !string.IsNullOrEmpty(sequence))
                {
                    peptides.Add(header, sequence);
                }
            }

            var orderedPeptides = peptides
                .OrderBy(kvp => kvp.Value)
                .ToList();
            var distinctPeptides = new List<KeyValuePair<string, string>>();
            string lastSequence = null;
            foreach (var peptide in orderedPeptides)
            {
                if(peptide.Value != lastSequence)
                    distinctPeptides.Add(peptide);
                lastSequence = peptide.Value;
            }
            File.WriteAllLines(@"F:\HumanGenome\orderedPeptides.csv", distinctPeptides.Select(kvp => $"{kvp.Key};{kvp.Value}"));
            File.WriteAllLines(@"F:\HumanGenome\orderedPeptides_sequenceOnly.txt", distinctPeptides.Select(kvp => kvp.Value));
        }
    }
}
