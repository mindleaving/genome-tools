using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Commons;

namespace ChemistryLibrary.Pdb
{
    public class PdbReaderResult
    {
        public PdbReaderResult(params Peptide[] chains)
        {
            Chains.AddRange(chains);
        }
        public List<Peptide> Chains { get; } = new List<Peptide>();
    }
    public class PdbReader
    {
        public PdbReaderResult ReadFile(string filename)
        {
            var lines = File.ReadAllLines(filename);
            var chainIds = ExtractChainIds(lines);
            var chains = new List<Peptide>();
            foreach (var chainId in chainIds)
            {
                var chain = ExtractChain(lines, chainId);
                chains.Add(chain);
            }
            return new PdbReaderResult(chains.ToArray());
        }

        private Peptide ExtractChain(IList<string> lines, char chainId)
        {
            var aminoAcidSequence = ExtractSequence(lines, chainId);
            var peptide = PeptideBuilder.PeptideFromSequence(aminoAcidSequence);
            peptide.ChainId = chainId;
            var annotations = ExtractAnnotations(lines, chainId, peptide);
            peptide.Annotations.AddRange(annotations);
            ReadAtomPositions(lines, chainId, peptide);
            return peptide;
        }

        private void ReadAtomPositions(IList<string> lines, char chainId, Peptide peptide)
        {
            var aminoAcidAtomGroups = lines
                .Where(line => ReadLineCode(line) == "ATOM")
                .Select(ParseAtomLine)
                .Where(atom => atom.ChainId == chainId)
                .GroupBy(atom => atom.ResidueNumber);
            foreach (var aminoAcidAtomInfos in aminoAcidAtomGroups)
            {
                var aminoAcid = peptide.AminoAcids[aminoAcidAtomInfos.Key - 1];
                PdbAminoAcidAtomNamer.AssignNames(aminoAcid);
                foreach (var atomInfo in aminoAcidAtomInfos)
                {
                    var correspondingAtom = aminoAcid.GetAtomFromName(atomInfo.Name);
                    if(correspondingAtom == null)
                        continue;
                    correspondingAtom.Position = new UnitPoint3D(SIPrefix.Pico, Unit.Meter,
                        100*atomInfo.X,
                        100*atomInfo.Y,
                        100*atomInfo.Z);
                }
            }
        }

        private List<PeptideAnnotation> ExtractAnnotations(IList<string> lines, char chainId, Peptide peptide)
        {
            var helices = lines
                .Where(line => ReadLineCode(line) == "HELIX")
                .Select(ParseHelix)
                .Where(helix => helix.FirstResidueChainId == chainId);
            var annotations = new List<PeptideAnnotation>();
            foreach (var helix in helices)
            {
                var annotation = new PeptideAnnotation(
                    PeptideAnnotationType.AlphaHelix,
                    peptide.AminoAcids.GetRange(helix.FirstResidueNumber - 1, helix.ResidueCount));
                annotations.Add(annotation);
            }
            var sheetStrandGroups = lines
                .Where(line => ReadLineCode(line) == "SHEET")
                .Select(ParseSheetStrand)
                .Where(strand => strand.FirstResidueChainId == chainId)
                .GroupBy(strand => strand.SheetId);
            foreach (var sheetStrands in sheetStrandGroups)
            {
                var sheetAminoAcids = new List<AminoAcidReference>();
                foreach (var strand in sheetStrands)
                {
                    sheetAminoAcids.AddRange(peptide.AminoAcids.GetRange(strand.FirstResidueNumber - 1, strand.ResidueCount));
                }
                var annotation = new PeptideAnnotation(
                    PeptideAnnotationType.BetaSheet, 
                    sheetAminoAcids);
                annotations.Add(annotation);
            }
            return annotations;
        }

        private IList<AminoAcidName> ExtractSequence(IList<string> lines, char chainId)
        {
            var sequence = new List<AminoAcidName>();

            // Extract sequence from dedicated entries
            var seqresLines = lines
                .Where(line => ReadLineCode(line) == "SEQRES")
                .Where(line => line[11] == chainId);
            foreach (var seqresLine in seqresLines)
            {
                var aminoAcidNames = seqresLine
                    .Substring(19)
                    .Split()
                    .Select(aminoAcidCode => aminoAcidCode.ToAminoAcidName());
                sequence.AddRange(aminoAcidNames);
            }
            if (sequence.Any())
                return sequence;

            // Extract sequence from atom entries
            var aminoAcidMap = lines
                    .Where(line => ReadLineCode(line) == "ATOM")
                    .Select(ParseAtomLine)
                    .Where(atom => atom.ChainId == chainId)
                    .GroupBy(atom => atom.ResidueNumber)
                    .ToDictionary(atomGroup => atomGroup.Key, atomGroup => atomGroup.First().ResidueName.ToAminoAcidName());
            var residueCount = aminoAcidMap.Keys.Max();
            for (var residueIdx = 1; residueIdx <= residueCount; residueIdx++)
            {
                var aminoAcidName = aminoAcidMap.ContainsKey(residueIdx)
                    ? aminoAcidMap[residueIdx]
                    : (AminoAcidName)StaticRandom.Rng.Next(20); // !!!!!!! Random residue generation !!!!!!
                sequence.Add(aminoAcidName);
            }
            return sequence;
        }

        private static string ReadLineCode(string line)
        {
            return line.Substring(0, 6).ToUpperInvariant().Trim();
        }

        private IEnumerable<char> ExtractChainIds(IEnumerable<string> lines)
        {
            var chainIds = new List<char>();
            foreach (var line in lines)
            {
                var lineCode = ReadLineCode(line);
                switch (lineCode)
                {
                    case "SEQRES":
                        chainIds.Add(line[11]);
                        break;
                    case "ATOM":
                        chainIds.Add(line[21]);
                        break;
                }
            }
            return chainIds.Distinct();
        }

        private PdbAtomLine ParseAtomLine(string line)
        {
            if(ReadLineCode(line) != "ATOM")
                throw new ArgumentException("This isn't an atom line");
            return new PdbAtomLine
            {
                SerialNumber = int.Parse(line.Substring(6, 5).Trim()),
                Name = line.Substring(12, 4).Trim().ToUpperInvariant(),
                ResidueName = line.Substring(17, 3).Trim().ToUpperInvariant(),
                ChainId = line[21],
                ResidueNumber = int.Parse(line.Substring(22, 4).Trim()),
                X = double.Parse(line.Substring(30, 8).Trim(), CultureInfo.InvariantCulture),
                Y = double.Parse(line.Substring(38, 8).Trim(), CultureInfo.InvariantCulture),
                Z = double.Parse(line.Substring(46, 8).Trim(), CultureInfo.InvariantCulture),
                Occupancy = double.Parse(line.Substring(54, 6).Trim(), CultureInfo.InvariantCulture),
                TemperatureFactor = double.Parse(line.Substring(60, 6).Trim(), CultureInfo.InvariantCulture),
                Element = (ElementSymbol)Enum.Parse(typeof(ElementSymbol), line.Substring(76, 2).Trim()),
                Charge = int.Parse(new string(line.Substring(78, 2).Reverse().ToArray()).Trim())
            };
        }

        private PdbHelixLine ParseHelix(string line)
        {
            if (ReadLineCode(line) != "HELIX")
                throw new ArgumentException("This isn't a helix line");
            return new PdbHelixLine
            {
                SerialNumber = int.Parse(line.Substring(7, 3).Trim()),
                Id = line.Substring(11, 3).Trim(),
                FirstResidueChainId = line[19],
                FirstResidueName = line.Substring(15, 3).Trim().ToUpperInvariant(),
                FirstResidueNumber = int.Parse(line.Substring(21, 4).Trim()),
                LastResidueChainId = line[31],
                LastResidueName = line.Substring(27, 3).Trim().ToUpperInvariant(),
                LastResidueNumber = int.Parse(line.Substring(33, 4).Trim()),
                Type = (HelixType)int.Parse(line.Substring(38, 2).Trim()),
                Comment = line.Substring(40, 30).Trim()
            };
        }

        private PdbSheetLine ParseSheetStrand(string line)
        {
            if (ReadLineCode(line) != "SHEET")
                throw new ArgumentException("This isn't a sheet line");
            return new PdbSheetLine
            {
                StrandSerialNumber = int.Parse(line.Substring(7, 3).Trim()),
                SheetId = line.Substring(11, 3),
                StrandCount = int.Parse(line.Substring(14, 2).Trim()),
                FirstResidueName = line.Substring(17, 3).Trim().ToUpperInvariant(),
                FirstResidueNumber = int.Parse(line.Substring(22, 4).Trim()),
                FirstResidueChainId = line[26],
                LastResidueName = line.Substring(28, 3).Trim().ToUpperInvariant(),
                LastResidueNumber = int.Parse(line.Substring(33, 4).Trim()),
                LastResidueChainId = line[32],
                StrandSense = (SheetStrandSense)int.Parse(line.Substring(38, 2).Trim())
            };
        }
    }
}
