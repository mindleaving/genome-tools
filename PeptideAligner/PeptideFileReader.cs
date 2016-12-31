using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PeptideAligner
{
    public static class PeptideFileReader
    {
        public static List<Peptide> ReadPeptides(string peptideDataFile)
        {
            var peptides = new List<Peptide>();
            using (var streamReader = new StreamReader(peptideDataFile))
            {
                Peptide peptide = null;
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.StartsWith(">"))
                    {
                        var splittedHeader = line.Split(':', ' ').ToList();
                        // ReSharper disable once InconsistentNaming
                        var GRCh38Index = splittedHeader.IndexOf("GRCh38");
                        var chromosome = splittedHeader[GRCh38Index + 1];
                        if (chromosome == "X" || chromosome == "Y" || chromosome.All(char.IsNumber))
                        {
                            peptide = new Peptide();
                            peptides.Add(peptide);
                            var startIndex = int.Parse(splittedHeader[GRCh38Index + 2]);
                            var endIndex = int.Parse(splittedHeader[GRCh38Index + 3]);
                            peptide.Chromosome = chromosome;
                            peptide.StartBase = startIndex;
                            peptide.EndBase = endIndex;

                            var geneSymbolIndex = splittedHeader.IndexOf("gene_symbol");
                            if (geneSymbolIndex >= 0)
                                peptide.GeneSymbol = splittedHeader[geneSymbolIndex + 1];
                        }
                        else
                        {
                            peptide = null;
                        }
                    }
                    else
                    {
                        peptide?.Sequence.AddRange(line);
                    }
                }
            }
            return peptides;
        }
    }
}