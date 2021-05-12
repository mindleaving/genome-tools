using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Builders;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.IO.Pdb
{
    public class PdbReaderOptions
    {
        public int? MaximumModelCount { get; set; } = null;
        public bool BuildMolecule { get; set; } = true;
    }
    public static class PdbReader
    {
        public static PdbReaderResult ReadFile(string filename, PdbReaderOptions options = null)
        {
            if(options == null)
                options = new PdbReaderOptions();
            var lines = File.ReadAllLines(filename);
            var models = ExtractModels(lines, options);
            return new PdbReaderResult(models);
        }

        private static PdbModel[] ExtractModels(
            string[] lines,
            PdbReaderOptions options)
        {
            var annotationLines = GetAnnotationLines(lines);
            var models = new List<PdbModel>();
            var modelLines = new List<string>();
            var modelStarted = false;
            try
            {
                foreach (var line in lines)
                {
                    if (!modelStarted && ReadLineCode(line).InSet("MODEL", "ATOM"))
                        modelStarted = true;
                    if(!modelStarted)
                        continue;
                    modelLines.Add(line);
                    if (ReadLineCode(line) == "ENDMDL")
                    {
                        models.Add(ConstructModel(modelLines, annotationLines, options));
                        modelStarted = false;
                        modelLines.Clear();
                        if(options.MaximumModelCount.HasValue && models.Count >= options.MaximumModelCount.Value)
                            break;
                    }
                }
                if(modelStarted)
                    models.Add(ConstructModel(modelLines, annotationLines, options));
            }
            catch
            {
                models.ForEach(model => model.Dispose());
                throw;
            }

            return models.ToArray();
        }

        private static IList<string> GetAnnotationLines(IList<string> lines)
        {
            return lines.Where(
                line => ReadLineCode(line).InSet("HELIX", "SHEET"))
                .ToList();
        }

        private static PdbModel ConstructModel(
            IList<string> modelLines,
            IList<string> annotationLines,
            PdbReaderOptions options)
        {
            var chainIds = ExtractChainIds(modelLines);
            var chains = new List<Peptide>();
            foreach (var chainId in chainIds)
            {
                try
                {
                    var chain = ExtractChain(modelLines, annotationLines, chainId, options);
                    if(!chain.AminoAcids.Any())
                        continue;
                    chains.Add(chain);
                }
                catch (ChemistryException chemException)
                {
                    // TODO: Log
                }
                catch
                {
                    chains.ForEach(chain => chain.Dispose());
                    throw;
                }
            }
            return new PdbModel(chains.ToArray());
        }

        private static Peptide ExtractChain(
            IList<string> modelLines,
            IList<string> annotationLines,
            char chainId,
            PdbReaderOptions options)
        {
            var aminoAcidSequence = ExtractSequence(modelLines, chainId);
            if(!aminoAcidSequence.Any())
                return new Peptide(new MoleculeReference(new Molecule()), new List<AminoAcidReference>());
            var peptide = PeptideBuilder.PeptideFromSequence(aminoAcidSequence, new PeptideBuilderOptions { BuildMolecule = options.BuildMolecule });
            peptide.ChainId = chainId;
            var annotations = ExtractAnnotations(annotationLines, chainId, peptide);
            peptide.Annotations.AddRange(annotations);
            if(options.BuildMolecule)
                ReadAtomPositions(modelLines, chainId, peptide);
            return peptide;
        }

        private static void ReadAtomPositions(IList<string> lines, char chainId, Peptide peptide)
        {
            var aminoAcidAtomGroups = lines
                .Where(line => ReadLineCode(line) == "ATOM")
                .Select(ParseAtomLine)
                .Where(atom => atom.ChainId == chainId)
                .Where(atom => atom.AlternateConformationId == ' ' || atom.AlternateConformationId == 'A')
                .Where(atom => !atom.IsAlternative)
                .GroupBy(atom => atom.ResidueNumber);
            foreach (var aminoAcidAtomInfos in aminoAcidAtomGroups)
            {
                if(aminoAcidAtomInfos.Key < peptide.AminoAcids.First().SequenceNumber) // TODO: Log warning
                    continue;
                if(aminoAcidAtomInfos.Key > peptide.AminoAcids.Last().SequenceNumber) // TODO: Log warning
                    continue;
                var residueNumber = aminoAcidAtomInfos.Key;
                var aminoAcid = peptide.AminoAcids.SingleOrDefault(aa => aa.SequenceNumber == residueNumber);
                if(aminoAcid == null) // TODO: Log warning
                    continue;
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
                    correspondingAtom.IsPositioned = true;
                    correspondingAtom.IsPositionFixed = true;
                }
            }
        }

        private static List<PeptideAnnotation<AminoAcidReference>> ExtractAnnotations(IList<string> lines, char chainId, Peptide peptide)
        {
            var helices = lines
                .Where(line => ReadLineCode(line) == "HELIX")
                .Select(ParseHelix)
                .Where(helix => helix.FirstResidueChainId == chainId);
            var annotations = new List<PeptideAnnotation<AminoAcidReference>>();
            foreach (var helix in helices)
            {
                var annotation = new PeptideAnnotation<AminoAcidReference>(
                    PeptideSecondaryStructure.AlphaHelix,
                    peptide.AminoAcids
                        .Where(aa => aa.SequenceNumber >= helix.FirstResidueNumber && aa.SequenceNumber <= helix.LastResidueNumber)
                        .ToList());
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
                    var aminoAcids = peptide.AminoAcids.Where(aa =>
                        aa.SequenceNumber >= strand.FirstResidueNumber
                        && aa.SequenceNumber <= strand.LastResidueNumber);
                    sheetAminoAcids.AddRange(aminoAcids);
                }
                var annotation = new PeptideAnnotation<AminoAcidReference>(
                    PeptideSecondaryStructure.BetaSheet, 
                    sheetAminoAcids);
                annotations.Add(annotation);
            }
            return annotations;
        }

        private static AminoAcidSequence ExtractSequence(IList<string> lines, char chainId)
        {
            var seqresSequence = ExtractSequenceFromSeqresLines(lines, chainId);
            var atomSequence = ExtractSequenceFromAtomLines(lines, chainId);
            ValidateSequences(seqresSequence, atomSequence);
            var longerSequence = seqresSequence.Count > atomSequence.Count ? seqresSequence : atomSequence;
            return longerSequence;
        }

        private static void ValidateSequences(AminoAcidSequence seqresSequence, AminoAcidSequence atomSequence)
        {
            var seqresQueue = new Queue<AminoAcidSequenceItem>(seqresSequence);
            var atomSequenceQueue = new Queue<AminoAcidSequenceItem>(atomSequence);

            while (seqresQueue.Any() && atomSequenceQueue.Any())
            {
                
            }
        }

        private static AminoAcidSequence ExtractSequenceFromSeqresLines(IList<string> lines, char chainId)
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
            return new AminoAcidSequence(sequence.Select((item, idx) => new AminoAcidSequenceItem
            {
                AminoAcidName = item,
                ResidueNumber = idx
            }));
        }

        private static AminoAcidSequence ExtractSequenceFromAtomLines(IList<string> lines, char chainId)
        {
            // Extract sequence from atom entries
            var aminoAcidMap = lines
                .Where(line => ReadLineCode(line) == "ATOM")
                .Select(ParseAtomLine)
                .Where(atom => atom.ChainId == chainId)
                .GroupBy(atom => atom.ResidueNumber)
                .Where(IsAminoAcidAtomGroup)
                .Select(atomGroup => new AminoAcidSequenceItem
                    {
                        AminoAcidName = ParseAminoAcidName(atomGroup.First().ResidueName),
                        ResidueNumber = atomGroup.Key
                    })
                .OrderBy(x => x.ResidueNumber)
                .ToList();
            return new AminoAcidSequence(aminoAcidMap);
        }

        private static bool IsAminoAcidAtomGroup(IGrouping<int, PdbAtomLine> atomGroup)
        {
            var residueName = atomGroup.First().ResidueName;
            return residueName.Length == 3 && residueName != "UNK";
        }

        private static AminoAcidName ParseAminoAcidName(string redidueName)
        {
            return redidueName.ToAminoAcidName();
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
                IsAlternative = line[26] != ' ',
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
