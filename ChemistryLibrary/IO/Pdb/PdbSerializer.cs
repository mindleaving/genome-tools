using System;
using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.DataLookups;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.IO.Pdb
{
    public static class PdbSerializer
    {
        public static string Serialize(string pdbCode, params Peptide[] peptides)
        {
            var header = BuildHeader(pdbCode, peptides.First());
            var output = header + Environment.NewLine;
            for (var modelIdx = 0; modelIdx < peptides.Length; modelIdx++)
            {
                var peptide = peptides[modelIdx];
                var atoms = SerializeAtoms(peptide);
                output += $"MODEL     {modelIdx,4}".PadRight(80) + Environment.NewLine
                          + atoms + Environment.NewLine
                          + "ENDMDL".PadRight(80) + Environment.NewLine;
            }

            var lines = output.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            var master = GenerateMaster(lines);

            output += master + Environment.NewLine
                      + "END".PadRight(80) + Environment.NewLine;
            return output;
        }

        #region Header serialization
        private static string BuildHeader(string pdbCode, Peptide peptide)
        {
            var description = "";
            var title = "";
            var date = DateTime.Now;

            var aminoAcidSequence = peptide.AminoAcids
                .Select(aa => aa.Name.ToThreeLetterCode())
                .ToList();

            var output =  BuildHeader(description, date, pdbCode) + Environment.NewLine
                   + BuilTitle(title) + Environment.NewLine
                   + BuildCompound(new List<string>()) + Environment.NewLine
                   + BuildSource(new List<string>()) + Environment.NewLine
                   + BuildKeywords(new List<string>()) + Environment.NewLine
                   + BuildExperimentData(new List<string>()) + Environment.NewLine
                   + BuildAuthor(new List<string>()) + Environment.NewLine
                   + BuildRevision(1,new List<string>()) + Environment.NewLine
                   + BuildRemark(2, new List<string>()) + Environment.NewLine
                   + BuildRemark(3, new List<string>()) + Environment.NewLine
                   + BuildSequence('A', aminoAcidSequence) + Environment.NewLine
                   + BuildCrystalCell() + Environment.NewLine
                   + BuildOrigin() + Environment.NewLine
                   + BuildScale();
            return output;
        }

        private static string BuildHeader(string description, DateTime date, string pdbCode)
        {
            return $"HEADER    {description,40}{date.ToString("dd-MMM-yy").ToUpperInvariant(),9}   {pdbCode,4}".PadRight(80);
        }

        private static string BuilTitle(string title)
        {
            var idx = 1;
            return $"TITLE   {idx,2}{title,70}";
        }

        private static string BuildCompound(IList<string> descriptions)
        {
            if(!descriptions.Any())
                descriptions = new List<string> {""};
            var output = "";
            for (int idx = 0; idx < descriptions.Count; idx++)
            {
                if (idx > 0)
                    output += Environment.NewLine;
                var description = descriptions[idx];
                output += $"COMPND {idx+1,3}{description,70}";
            }
            return output;
        }

        private static string BuildSource(IList<string> sources)
        {
            if(!sources.Any())
                sources = new List<string> {""};
            var output = "";
            for (int idx = 0; idx < sources.Count; idx++)
            {
                if (idx > 0)
                    output += Environment.NewLine;
                var source = sources[idx];
                output += $"SOURCE {idx+1,3}{source,69} ";
            }
            return output;
        }

        private static string BuildKeywords(IList<string> experimentData)
        {
            if(!experimentData.Any())
                experimentData = new List<string> {""};
            var output = "";
            for (int idx = 0; idx < experimentData.Count; idx++)
            {
                if (idx > 0)
                    output += Environment.NewLine;
                var keyword = experimentData[idx];
                output += $"KEYWDS  {idx+1,2}{keyword,69} ";
            }
            return output;
        }

        private static string BuildExperimentData(IList<string> experimentData)
        {
            if (!experimentData.Any())
                experimentData = new List<string> { "" };
            var output = "";
            for (int idx = 0; idx < experimentData.Count; idx++)
            {
                if (idx > 0)
                    output += Environment.NewLine;
                var data = experimentData[idx];
                output += $"EXPDTA  {idx+1,2}{data,69} ";
            }
            return output;
        }
        private static string BuildAuthor(IList<string> authors)
        {
            if (!authors.Any())
                authors = new List<string> { "" };
            var output = "";
            for (int idx = 0; idx < authors.Count; idx++)
            {
                if (idx > 0)
                    output += Environment.NewLine;
                var author = authors[idx];
                output += $"AUTHOR  {idx+1,2}{author,69} ";
            }
            return output;
        }
        private static string BuildRevision(int revisionVersion, IList<string> revisionData)
        {
            if (!revisionData.Any())
                revisionData = new List<string> { "" };
            var output = "";
            for (int idx = 0; idx < revisionData.Count; idx++)
            {
                if (idx > 0)
                    output += Environment.NewLine;
                var revisionNote = revisionData[idx];
                var date = idx == 0
                    ? DateTime.MinValue.ToString("dd-MMM-yy").ToUpperInvariant()
                    : "";
                var revisionId = revisionVersion > 1 ? 1 : 0;
                output += $"REVDAT {revisionVersion,3}{idx+1,2} {date,9} {revisionId,4}    1       {revisionNote,41}";
            }
            return output;
        }

        private static string BuildRemark(int remarkNr, IList<string> remarks)
        {
            if (!remarks.Any())
                remarks = new List<string> { "" };
            var output = "";
            for (int idx = 0; idx < remarks.Count; idx++)
            {
                if (idx > 0)
                    output += Environment.NewLine;
                var remark = remarks[idx];
                output += $"REMARK {remarkNr,3} {remark,68} ";
            }
            return output;
        }

        private static string BuildSequence(char sequenceId, List<string> residues)
        {
            if (!residues.Any())
                residues = new List<string> { "" };
            var output = "";
            for (int lineIdx = 0; lineIdx < Math.Ceiling(residues.Count / 13.0); lineIdx++)
            {
                if (lineIdx > 0)
                    output += Environment.NewLine;
                var line = $"SEQRES {lineIdx+1,3} {sequenceId} {residues.Count,4}  "
                           + residues.Skip(lineIdx * 13).Take(13).Aggregate((a, b) => a + " " + b);
                output += line.PadRight(80);
            }
            return output;
        }

        private static string BuildCrystalCell()
        {
            var a = 1.0.ToString("F3");
            var b = 1.0.ToString("F3");
            var c = 1.0.ToString("F3");
            var alpha = 90.0.ToString("F2");
            var beta = 90.0.ToString("F2");
            var gamma = 90.0.ToString("F2");
            var spaceGroup = "P 1";
            var z = 1;
            return $"CRYST1{a,9}{b,9}{c,9}{alpha,7}{beta,7}{gamma,7}{spaceGroup,11}{z,4}".PadRight(80);
        }

        private static string BuildOrigin()
        {
            var output = "";
            for (int dimension = 1; dimension <= 3; dimension++)
            {
                if (dimension > 1)
                    output += Environment.NewLine;
                var x1 = (dimension == 1 ? 1.0 : 0.0).ToString("F6");
                var x2 = (dimension == 2 ? 1.0 : 0.0).ToString("F6");
                var x3 = (dimension == 3 ? 1.0 : 0.0).ToString("F6");
                var t = 0.0.ToString("F5");
                output += $"ORIGX{dimension}    {x1,-10}{x2,-10}{x3,-10}     {t,-10}".PadRight(80);
            }
            return output;
        }

        private static string BuildScale()
        {
            var output = "";
            for (int dimension = 1; dimension <= 3; dimension++)
            {
                if (dimension > 1)
                    output += Environment.NewLine;
                var x1 = (dimension == 1 ? 1.0 : 0.0).ToString("F6");
                var x2 = (dimension == 2 ? 1.0 : 0.0).ToString("F6");
                var x3 = (dimension == 3 ? 1.0 : 0.0).ToString("F6");
                var t = 0.0.ToString("F5");
                output += $"SCALE{dimension}    {x1,-10}{x2,-10}{x3,-10}     {t,-10}".PadRight(80);
            }
            return output;
        }
        #endregion

        #region Atom serialization
        private static string SerializeAtoms(Peptide peptide)
        {
            var output = "";
            var atomIdx = 1;
            var chainId = 'A';
            var aminoAcids = peptide.AminoAcids;
            for (int residueIdx = 0; residueIdx < aminoAcids.Count; residueIdx++)
            {
                var aminoAcid = aminoAcids[residueIdx];
                PdbAminoAcidAtomNamer.AssignNames(aminoAcid);
                var aminoAcidVertices = aminoAcid.VertexIds
                    .Select(vId => aminoAcid.Molecule.MoleculeStructure.GetVertexFromId(vId));
                var residueName = aminoAcid.Name.ToThreeLetterCode();
                var sequenceNumber = aminoAcid.SequenceNumber;
                foreach (var vertex in aminoAcidVertices)
                {
                    var atom = aminoAcid.Molecule.GetAtom(vertex.Id);
                    if (atom.Element == ElementName.Hydrogen)
                        continue;
                    if(atom.Position == null)
                        continue;
                    var atomName = atom.AminoAcidAtomName != null
                        ? atom.Element.ToElementSymbol().ToString().Length == 1 ? " " + atom.AminoAcidAtomName : atom.AminoAcidAtomName
                        : atom.Element.ToElementSymbol().ToString().PadLeft(2, ' ').ToUpperInvariant();
                    var x = (1e10*atom.Position.X).ToString("F3");
                    var y = (1e10*atom.Position.Y).ToString("F3");
                    var z = (1e10*atom.Position.Z).ToString("F3");
                    var occupancy = 1.0.ToString("F2");
                    var temperatureFactor = 0.0.ToString("F2");
                    var elementSymbol = atom.Element.ToElementSymbol();
                    var charge = "";

                    if (atomIdx > 1)
                        output += Environment.NewLine;
                    output += $"ATOM  {atomIdx,5} {atomName,-4} {residueName,3} {chainId}{sequenceNumber,4}    " +
                            $"{x,8}{y,8}{z,8}{occupancy,6}{temperatureFactor,6}          " +
                            $"{elementSymbol,2}{charge,2}";
                    atomIdx++;
                }
            }
            output += Environment.NewLine;
            output += $"TER   {atomIdx,5}      {aminoAcids.Last().Name.ToThreeLetterCode()} {chainId}{aminoAcids.Count-1,4}".PadRight(80);
            return output;
        }
        #endregion

        #region Master generation
        private static string GenerateMaster(string[] lines)
        {
            var remarkCount = lines.Count(line => line.StartsWith("REMARK"));
            var hetCount = lines.Count(line => line.StartsWith("HET"));
            var helixCount = lines.Count(line => line.StartsWith("HELIX"));
            var sheetCount = lines.Count(line => line.StartsWith("SHEET"));
            var siteCount = lines.Count(line => line.StartsWith("SITE"));
            var coordTransform = lines.Count(line => line.StartsWith("ORIGX") || line.StartsWith("SCALE") || line.StartsWith("MTRIX"));
            var atomCount = lines.Count(line => line.StartsWith("ATOM") || line.StartsWith("HETATM"));
            var terCount = lines.Count(line => line.StartsWith("TER"));
            var conectCount = lines.Count(line => line.StartsWith("CONECT"));
            var sequenceCount = lines.Count(line => line.StartsWith("SEQRES"));
            var master = $"MASTER    {remarkCount,5}{0,5}{hetCount,5}{helixCount,5}{sheetCount,5}     {siteCount,5}{coordTransform,5}" +
                   $"{atomCount,5}{terCount,5}{conectCount,5}{sequenceCount,5}";
            return master.PadRight(80);
        }
        #endregion
    }
}
