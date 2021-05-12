using System;
using System.Collections.Generic;
using System.IO;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.IO.Fastq
{
    public class FastqReader
    {
        public FastqData Read(string filePath)
        {
            var contigs = new List<Contig>();
            using (var streamReader = new StreamReader(filePath))
            {
                while (true)
                {
                    var headerLine = streamReader.ReadLine();
                    while (headerLine != null && !headerLine.StartsWith("@"))
                    {
                        headerLine = streamReader.ReadLine();
                    }
                    if(headerLine == null)
                        break;

                    var nucleotideLine = streamReader.ReadLine();
                    var controlLine = streamReader.ReadLine();
                    if(controlLine == null || !controlLine.StartsWith("+"))
                        continue;
                    var qualityLine = streamReader.ReadLine();

                    var contig = ParseContig(headerLine, nucleotideLine, qualityLine);
                    contigs.Add(contig);
                }
            }
            return new FastqData(contigs);
        }

        private Contig ParseContig(string headerLine, string nucleotideLine, string qualityLine)
        {
            if(nucleotideLine.Length != qualityLine.Length)
                throw new FormatException("Nucleotide and ");
            var nucleotideBytes = new Nucleotide[nucleotideLine.Length];
            var qualityBytes = new byte[nucleotideBytes.Length];
            for (int idx = 0; idx < nucleotideLine.Length; idx++)
            {
                nucleotideBytes[idx] = ParseNucleotide(nucleotideLine[idx]);
                qualityBytes[idx] = ParseQuality(qualityLine[idx]);
                
            }
            return new Contig(nucleotideBytes, qualityBytes);
        }

        private Nucleotide ParseNucleotide(char nucleotideChar)
        {
            switch (nucleotideChar)
            {
                case 'Z':
                    return byte.MinValue;
                case 'A':
                    return Nucleotide.A;
                case 'C':
                    return Nucleotide.C;
                case 'G':
                    return Nucleotide.G;
                case 'T':
                case 'U':
                    return Nucleotide.T;
                case 'W':
                    return Nucleotide.W;
                case 'S':
                    return Nucleotide.S;
                case 'M':
                    return Nucleotide.M;
                case 'K':
                    return Nucleotide.K;
                case 'R':
                    return Nucleotide.R;
                case 'Y':
                    return Nucleotide.Y;
                case 'B':
                    return Nucleotide.B;
                case 'D':
                    return Nucleotide.D;
                case 'F':
                    return Nucleotide.H;
                case 'V':
                    return Nucleotide.V;
                case 'N':
                    return Nucleotide.N;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nucleotideChar));
            }
        }

        private byte ParseQuality(char qualityChar)
        {
            // Illumina 1.5+
            return (byte)(qualityChar - '@');
            
            // Illumina 1.8+
            //return (byte) (qualityChar - '!');
        }
    }
}
