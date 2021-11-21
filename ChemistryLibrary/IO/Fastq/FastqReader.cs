using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.IO.Fastq
{
    public class FastqReader
    {
        public List<GenomeRead> Read(string filePath)
        {
            return Read(File.OpenRead(filePath));
        }

        public List<GenomeRead> Read(Stream stream)
        {
            var reads = new List<GenomeRead>();
            using var streamReader = new StreamReader(stream);
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

                var read = ParseRead(headerLine, nucleotideLine, qualityLine);
                reads.Add(read);
            }

            return reads;
        }

        private GenomeRead ParseRead(string headerLine, string nucleotideLine, string qualityLine)
        {
            if(nucleotideLine.Length != qualityLine.Length)
                throw new FormatException("Nucleotide and ");
            var nucleotides = new char[nucleotideLine.Length];
            var qualityScores = new char[nucleotides.Length];
            for (int idx = 0; idx < nucleotideLine.Length; idx++)
            {
                nucleotides[idx] = nucleotideLine[idx];
                qualityScores[idx] = qualityLine[idx];
                
            }
            var id = headerLine.Substring(1).Split().First();
            return GenomeRead.UnmappedRead(nucleotides, qualityScores, id);
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
            //return (byte)(qualityChar - '@');
            
            // Illumina 1.8+
            return (byte) (qualityChar - '!');
        }
    }
}
