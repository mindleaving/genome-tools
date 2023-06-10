using System;
using System.Collections.Generic;
using System.IO;
using Accord.Math;
using Commons.Extensions;
using Commons.IO;
using GenomeTools.ChemistryLibrary.IO;
using NUnit.Framework;

namespace GenomeTools.Studies.Nanopore
{
    internal class NanoporeDatasetBuilding
    {
        private const string ReferenceName = "NR_003286_RNA18SN5";
        private const string ReferenceFilePath = @"F:\Projects\WFT_ONT_Win\ComputationalAnalysis\data\index.fasta";
        private const string ModificationGroundTruthFilePath = @"F:\Projects\WFT_ONT_Win\ComputationalAnalysis\data\all_rrna_mod_status.tsv";
        private const string IVTFeaturesFilePath = @"F:\Projects\WFT_ONT_Win\experiment-data\18S-IVT-noRT-features.csv";
        private const string OutputDirectory = @"F:\Projects\WFT_ONT_Win\experiment-data";
        private const char OutputDelimiter = ',';

        [Test]
        [TestCase("WT-psU", @"F:\Projects\WFT_ONT_Win\experiment-data\18S-WT-noRT-features.csv")]
        [TestCase("bloodA", @"F:\Projects\WFT_ONT_Win\experiment-data\18S-bloodA-noRT-features.csv")]
        [TestCase("bloodJ1", @"F:\Projects\WFT_ONT_Win\experiment-data\18S-bloodJ1-noRT-features.csv")]
        [TestCase("bloodV", @"F:\Projects\WFT_ONT_Win\experiment-data\18S-bloodV-noRT-features.csv")]
        public void BuildDataset(
            string datasetId,
            string featuresFilePath)
        {
            const bool RandomizeObservations = true;
            const bool UseBinaryModificationClass = true;
            const bool IncludeIndex = true;
            const int ModificationMultiplier = 10;
            bool IsModification(NucleotideModificationType modification) => modification == NucleotideModificationType.psU;
                

            var reference = new GenomeSequenceAccessor(ReferenceFilePath).GetSequenceByName(ReferenceName);
            var modifications = LoadModifications();
            var features = CsvReader.ReadTable<double>(
                featuresFilePath, 
                str => double.TryParse(str, out var value) ? value : double.NaN);
            var ivtFeatures = CsvReader.ReadTable<double>(
                IVTFeaturesFilePath,
                str => double.TryParse(str, out var value) ? value : double.NaN);

            var lines = new List<string>();
            var columnNames = new List<string>
            {
                "Modification",
                "Reference",
                "Wrong call %",
                "IVT Wrong call %",
                "Insert %",
                "Deletion %",
                "JACUSA Mis",
                "JACUSA Mis+Del+Ins",
                "JACUSA MisContext+Del+Ins",
                "JACUSA Mis+Del+Ins_Context",
                "ReadStart",
                "ReadEnd"
            };
            if(IncludeIndex)
                columnNames.Add("Index");
            var header = string.Join(OutputDelimiter, columnNames);
            foreach (var row in features.Rows)
            {
                var index = (int)row["Index"];
                var ivtRow = ivtFeatures.Rows[index];
                if ((int)ivtRow["Index"] != index)
                    throw new Exception("IVT data is offset");
                var referenceBase = reference.GetBaseAtPosition(index);
                if (!modifications.TryGetValue(index, out var modification))
                    modification = NucleotideModificationType.Unm;
                var coverage = (int)row["Coverage"];
                if(coverage < 10)
                    continue;
                var A = (int)row["A"];
                var C = (int)row["C"];
                var G = (int)row["C"];
                var T = (int)row["T"];
                var inserts = (int)row["Insert"];
                var deletions = (int)row["Deletion"];
                var readStart = (int)row["ReadStart"];
                var readEnd = (int)row["ReadEnd"];
                var jacusaMis = row["JACUSA Mis"];
                var jacusaMisDelIns = row["JACUSA Mis+Del+Ins"];
                var jacusaMisContextDelIns = row["JACUSA MisContext+Del+Ins"];
                var jacusaMisDelInsContext = row["JACUSA Mis+Del+Ins_Context"];

                var correctCalls = (int)row[referenceBase + ""];
                var wrongCall = coverage - correctCalls;
                var wrongCallPercentage = wrongCall / (double)coverage;

                var ivtCoverage = (int)ivtRow["Coverage"];
                if(ivtCoverage < 10)
                    continue;
                var ivtCorrectCalls = (int)ivtRow[referenceBase + ""];
                var ivtWrongCall = ivtCoverage - ivtCorrectCalls;
                var ivtWrongCallPercentage = ivtWrongCall / (double)ivtCoverage;

                var insertPercentage = inserts / (double)coverage;
                var deletionPercentage = deletions / (double)coverage;

                var values = new string[columnNames.Count];
                values[0] = UseBinaryModificationClass 
                    ? (IsModification(modification) ? "Mod" : "Unm")
                    : modification.ToString();
                values[1] = referenceBase + "";
                values[2] = wrongCallPercentage.ToString("F4");
                values[3] = ivtWrongCallPercentage.ToString("F4");
                values[4] = insertPercentage.ToString("F4");
                values[5] = deletionPercentage.ToString("F4");
                values[6] = jacusaMis.ToString("F2");
                values[7] = jacusaMisDelIns.ToString("F2");
                values[8] = jacusaMisContextDelIns.ToString("F2");
                values[9] = jacusaMisDelInsContext.ToString("F2");
                values[10] = readStart.ToString();
                values[11] = readEnd.ToString();
                if(IncludeIndex)
                    values[12] = index.ToString();
                var line = string.Join(OutputDelimiter, values);
                lines.Add(line);
                if (UseBinaryModificationClass && IsModification(modification))
                {
                    for (int i = 0; i < ModificationMultiplier-1; i++)
                    {
                        lines.Add(line);
                    }
                }
            }
            if(RandomizeObservations)
                lines.Shuffle();
            lines.Insert(0, header);
            var fileName = ConstructFileName(datasetId, UseBinaryModificationClass, ModificationMultiplier, RandomizeObservations);
            var outputFilePath = Path.Combine(OutputDirectory, fileName);
            File.WriteAllLines(outputFilePath, lines);
        }

        private static string ConstructFileName(
            string datasetId,
            bool useBinaryModificationClass,
            int modificationMultiplier,
            bool shuffled)
        {
            var fileNameParts = new List<string>
            {
                datasetId
            };
            if (useBinaryModificationClass)
                fileNameParts.Add("binary");
            if(modificationMultiplier > 1)
                fileNameParts.Add($"{modificationMultiplier}x");
            if(shuffled)
                fileNameParts.Add("shuffled");
            fileNameParts.Add(DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss"));
            var fileName = string.Join("_", fileNameParts) + ".csv";
            return fileName;
        }

        private Dictionary<int,NucleotideModificationType> LoadModifications()
        {
            var table = CsvReader.ReadTable(ModificationGroundTruthFilePath, x => x, delimiter: ',');
            var modifications = new Dictionary<int, NucleotideModificationType>();
            foreach (var row in table.Rows)
            {
                if(row["Chr"] != ReferenceName)
                    continue;
                var index = int.Parse(row["Position"]);
                var modification = Enum.Parse<NucleotideModificationType>(row["ModStatus"]);
                if(modification != NucleotideModificationType.Unm)
                    modifications.Add(index, modification);
            }
            return modifications;
        }
    }

    internal enum NucleotideModificationType
    {
        Unm,
        psU,
        Am,
        Cm,
        Gm,
        Um,
        ND,
        m1acp3psU,
        ac4C,
        m7G,
        m6A,
        m62A
    }
}
