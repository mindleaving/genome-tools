using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ChemistryLibrary.Builders;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.IO.Pdb
{
    public class PdbReaderResult
    {
        public PdbReaderResult(params Peptide[] chains)
        {
            Chains.AddRange(chains);
        }

        public List<Peptide> Chains { get; } = new List<Peptide>();
    }

    public static class PdbReader
    {
        public static PdbReaderResult ReadFile(string filename)
        {
            var lines = File.ReadAllLines(filename);
            var chainIds = ExtractChainIds(lines);
            var chains = new List<Peptide>();
            foreach (var chainId in chainIds)
            {
                try
                {
                    var chain = ExtractChain(lines, chainId);
                    chains.Add(chain);
                }
                catch (ChemistryException chemException)
                {
                    // TODO: Log
                }
            }
            return new PdbReaderResult(chains.ToArray());
        }

        private static Peptide ExtractChain(IList<string> lines, char chainId)
        {
            var aminoAcidSequence = ExtractSequence(lines, chainId);
            var peptide = PeptideBuilder.PeptideFromSequence(aminoAcidSequence.Sequence);
            peptide.ChainId = chainId;
            var annotations = ExtractAnnotations(lines, chainId, peptide, aminoAcidSequence.FirstResidueNumber);
            peptide.Annotations.AddRange(annotations);
            ReadAtomPositions(lines, chainId, peptide, aminoAcidSequence.FirstResidueNumber);
            return peptide;
        }

        private static void ReadAtomPositions(IList<string> lines, char chainId, Peptide peptide, int firstResidueNumber)
        {
            var aminoAcidAtomGroups = lines
                .Where(line => ReadLineCode(line) == "ATOM")
                .Select(ParseAtomLine)
                .Where(atom => atom.ChainId == chainId)
                .Where(atom => atom.AlternateConformationId == ' ' || atom.AlternateConformationId == 'A')
                .GroupBy(atom => atom.ResidueNumber);
            foreach (var aminoAcidAtomInfos in aminoAcidAtomGroups)
            {
                if(aminoAcidAtomInfos.Key < firstResidueNumber) // TODO: Log warning
                    continue;
                if(aminoAcidAtomInfos.Key >= peptide.AminoAcids.Count + firstResidueNumber) // TODO: Log warning
                    continue;
                var aminoAcid = peptide.AminoAcids[aminoAcidAtomInfos.Key - firstResidueNumber];
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
                    correspondingAtom.IsPositionFixed = true;
                }
            }
        }

        private static List<PeptideAnnotation> ExtractAnnotations(IList<string> lines, char chainId, Peptide peptide, int firstResidueNumber)
        {
            var helices = lines
                .Where(line => ReadLineCode(line) == "HELIX")
                .Select(ParseHelix)
                .Where(helix => helix.FirstResidueChainId == chainId);
            var annotations = new List<PeptideAnnotation>();
            foreach (var helix in helices)
            {
                var annotation = new PeptideAnnotation(
                    PeptideSecondaryStructure.AlphaHelix,
                    peptide.AminoAcids.Skip(helix.FirstResidueNumber - firstResidueNumber).Take(helix.ResidueCount).ToList(),
                    helix.FirstResidueNumber);
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
                    sheetAminoAcids.AddRange(peptide.AminoAcids.Skip(strand.FirstResidueNumber - firstResidueNumber).Take(strand.ResidueCount));
                }
                var annotation = new PeptideAnnotation(
                    PeptideSecondaryStructure.BetaSheet, 
                    sheetAminoAcids,
                    sheetStrands.First().FirstResidueNumber);
                annotations.Add(annotation);
            }
            return annotations;
        }

        private class AminoAcidSequence
        {
            public AminoAcidSequence(IList<AminoAcidName> aminoAcidNames, int firstResidueNumber)
            {
                Sequence = new List<AminoAcidName>(aminoAcidNames);
                FirstResidueNumber = firstResidueNumber;
            }

            public int FirstResidueNumber { get; }
            public IList<AminoAcidName> Sequence { get; }
        }
        private static AminoAcidSequence ExtractSequence(IList<string> lines, char chainId)
        {
            //var seqresSequence = ExtractSequenceFromSeqresLines(lines, chainId);
            var atomSequence = ExtractSequenceFromAtomLines(lines, chainId);
            //var matchedSequences = new OffsetSequenceMatcher<AminoAcidName>(
            //    seqresSequence.Select(x => x.AminoAcidName).ToList(), 
            //    atomSequence.Select(x => x.AminoAcidName).ToList());

            // Build sequence from atom sequence
            var startResidueNumber = atomSequence.First().ResidueNumber;
            var lastResidueNumber = atomSequence.Last().ResidueNumber;
            var sequence = new List<AminoAcidName>();
            var residueMap = atomSequence.ToDictionary(x => x.ResidueNumber, x => x.AminoAcidName);
            for (int residueIdx = startResidueNumber; residueIdx <= lastResidueNumber; residueIdx++)
            {
                AminoAcidName aminoAcidName;
                if (residueMap.ContainsKey(residueIdx))
                    aminoAcidName = residueMap[residueIdx];
                else
                {
                    aminoAcidName = (AminoAcidName)StaticRandom.Rng.Next(20); // !!!!!!! Random residue generation !!!!!!
                }
                sequence.Add(aminoAcidName);
            }
            return new AminoAcidSequence(sequence, startResidueNumber);
        }

        private static IList<ResidueSequenceItem> ExtractSequenceFromSeqresLines(IList<string> lines, char chainId)
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
                    .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(aminoAcidCode => aminoAcidCode.ToAminoAcidName());
                sequence.AddRange(aminoAcidNames);
            }
            return sequence.Select((item, idx) => new ResidueSequenceItem
                {
                    AminoAcidName = item,
                    ResidueNumber = idx
                })
                .ToList();
        }

        private class ResidueSequenceItem
        {
            public AminoAcidName AminoAcidName { get; set; }
            public int ResidueNumber { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                    return true;
                if (ReferenceEquals(obj, null))
                    return false;
                if (!(obj is ResidueSequenceItem))
                    return false;
                var otherItem = (ResidueSequenceItem) obj;
                return Equals(otherItem);
            }

            private bool Equals(ResidueSequenceItem other)
            {
                return AminoAcidName == other.AminoAcidName;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int) AminoAcidName*397) ^ ResidueNumber;
                }
            }
        }
        private static IList<ResidueSequenceItem> ExtractSequenceFromAtomLines(IList<string> lines, char chainId)
        {
            // Extract sequence from atom entries
            var aminoAcidMap = lines
                .Where(line => ReadLineCode(line) == "ATOM")
                .Select(ParseAtomLine)
                .Where(atom => atom.ChainId == chainId)
                .GroupBy(atom => atom.ResidueNumber)
                .Select(atomGroup => new ResidueSequenceItem
                    {
                        AminoAcidName = ParseAminoAcidName(atomGroup.First().ResidueName),
                        ResidueNumber = atomGroup.Key
                    })
                .ToList();
            return aminoAcidMap;
        }

        private static AminoAcidName ParseAminoAcidName(string redidueName)
        {
            return redidueName.ToAminoAcidName();
            //try
            //{
            //    return redidueName.ToAminoAcidName();
            //}
            //catch (ChemistryException)
            //{
            //    return AminoAcidName.Alanine;
            //}
        }

        private static string ReadLineCode(string line)
        {
            return line.Substring(0, 6).ToUpperInvariant().Trim();
        }

        private static IEnumerable<char> ExtractChainIds(IEnumerable<string> lines)
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

        private static PdbAtomLine ParseAtomLine(string line)
        {
            if(ReadLineCode(line) != "ATOM")
                throw new ArgumentException("This isn't an atom line");
            var chargeString = line.Substring(78, 2).Trim();
            var charge = chargeString.Length > 0
                ? int.Parse(new string(chargeString.Reverse().ToArray()))
                : 0;
            return new PdbAtomLine
            {
                SerialNumber = int.Parse(line.Substring(6, 5).Trim()),
                Name = line.Substring(12, 4).Trim().ToUpperInvariant(),
                ResidueName = line.Substring(17, 3).Trim().ToUpperInvariant(),
                ChainId = line[21],
                AlternateConformationId = line[16],
                ResidueNumber = int.Parse(line.Substring(22, 4).Trim()),
                X = double.Parse(line.Substring(30, 8).Trim(), CultureInfo.InvariantCulture),
                Y = double.Parse(line.Substring(38, 8).Trim(), CultureInfo.InvariantCulture),
                Z = double.Parse(line.Substring(46, 8).Trim(), CultureInfo.InvariantCulture),
                Occupancy = double.Parse(line.Substring(54, 6).Trim(), CultureInfo.InvariantCulture),
                TemperatureFactor = double.Parse(line.Substring(60, 6).Trim(), CultureInfo.InvariantCulture),
                Element = (ElementSymbol)Enum.Parse(typeof(ElementSymbol), line.Substring(76, 2).Trim()),
                Charge = charge
            };
        }

        private static PdbHelixLine ParseHelix(string line)
        {
            if (ReadLineCode(line) != "HELIX")
                throw new ArgumentException("This isn't a helix line");
            var helix =  new PdbHelixLine
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
            if (helix.FirstResidueNumber == 0)
                helix.FirstResidueNumber = 1;
            return helix;
        }

        private static PdbSheetLine ParseSheetStrand(string line)
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
                FirstResidueChainId = line[21],
                LastResidueName = line.Substring(28, 3).Trim().ToUpperInvariant(),
                LastResidueNumber = int.Parse(line.Substring(33, 4).Trim()),
                LastResidueChainId = line[32],
                StrandSense = (SheetStrandSense)int.Parse(line.Substring(38, 2).Trim())
            };
        }
    }
}
