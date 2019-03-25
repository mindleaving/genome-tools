using System;
using System.Collections.Generic;
using System.IO;
using SequenceAssembler.Objects;

namespace SequenceAssembler
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
            var nucleotideBytes = new byte[nucleotideLine.Length];
            var qualityBytes = new byte[nucleotideBytes.Length];
            for (int idx = 0; idx < nucleotideLine.Length; idx++)
            {
                nucleotideBytes[idx] = (byte)ParseNucleotide(nucleotideLine[idx]);
                qualityBytes[idx] = ParseQuality(qualityLine[idx]);
                
            }
            return new Contig(nucleotideBytes, qualityBytes);
        }

        private Nucelotide ParseNucleotide(char nucleotideChar)
        {
            switch (nucleotideChar)
            {
                case 'Z':
                    return byte.MinValue;
                case 'A':
                    return Nucelotide.A;
                case 'C':
                    return Nucelotide.C;
                case 'G':
                    return Nucelotide.G;
                case 'T':
                case 'U':
                    return Nucelotide.T;
                case 'W':
                    return Nucelotide.W;
                case 'S':
                    return Nucelotide.S;
                case 'M':
                    return Nucelotide.M;
                case 'K':
                    return Nucelotide.K;
                case 'R':
                    return Nucelotide.R;
                case 'Y':
                    return Nucelotide.Y;
                case 'B':
                    return Nucelotide.B;
                case 'D':
                    return Nucelotide.D;
                case 'F':
                    return Nucelotide.H;
                case 'V':
                    return Nucelotide.V;
                case 'N':
                    return Nucelotide.N;
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
