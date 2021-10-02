using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GeneLocationInfoReader
    {
        private readonly string peptideDataFile;

        public GeneLocationInfoReader(string peptideDataFile)
        {
            this.peptideDataFile = peptideDataFile;
        }

        public List<GeneLocationInfo> ReadAllGenes()
        {
            return ReadFilteredGenes();
        }

        public List<GeneLocationInfo> ReadGenesForSymbol(string geneSymbol)
        {
            return ReadFilteredGenes(x => x.GeneSymbol == geneSymbol);
        }

        private List<GeneLocationInfo> ReadFilteredGenes(Func<GeneLocationInfo, bool> filter = null)
        {
            var genes = new List<GeneLocationInfo>();
            using var streamReader = new StreamReader(peptideDataFile);
            GeneLocationInfo gene = null;
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                if (IsHeaderLine(line))
                {
                    gene = ParseHeaderLine(line);
                    if (gene == null) 
                        continue;
                    if (filter != null && !filter(gene))
                        gene = null;
                    else
                        genes.Add(gene);
                }
                else
                {
                    gene?.AminoAcidSequence.AddRange(line.Select(aminoAcidCode => aminoAcidCode.ToAminoAcidName()));
                }
            }

            return genes;
        }

        private static bool IsHeaderLine(string line)
        {
            return line.StartsWith(">");
        }

        private static GeneLocationInfo ParseHeaderLine(string line)
        {
            GeneLocationInfo geneLocationInfo;
            var splittedHeader = line.Split(':', ' ').ToList();
            // ReSharper disable once InconsistentNaming
            var GRCh38Index = splittedHeader.IndexOf("GRCh38");
            var chromosome = splittedHeader[GRCh38Index + 1];
            if (chromosome is "X" or "Y" || chromosome.All(char.IsNumber))
            {
                geneLocationInfo = new GeneLocationInfo();
                var startIndex = int.Parse(splittedHeader[GRCh38Index + 2]);
                var endIndex = int.Parse(splittedHeader[GRCh38Index + 3]);
                geneLocationInfo.Chromosome = chromosome;
                geneLocationInfo.StartBase = startIndex;
                geneLocationInfo.EndBase = endIndex;

                var geneSymbolIndex = splittedHeader.IndexOf("gene_symbol");
                if (geneSymbolIndex >= 0)
                    geneLocationInfo.GeneSymbol = splittedHeader[geneSymbolIndex + 1];
            }
            else
            {
                geneLocationInfo = null;
            }

            return geneLocationInfo;
        }
    }
}