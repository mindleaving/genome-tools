using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Commons;
using Commons.Collections;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Builders;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    [TestFixture]
    public class AminoAcidAngleStudy
    {
        [Test]
        public void InterAminoAcidAngles()
        {
            // Angles between alpha-carbon of adjacent amino acids
            var aminoAcidPositionDirectory = @"F:\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions";
            var positionFiles = Directory.EnumerateFiles(aminoAcidPositionDirectory, "pdb*model000.csv");
            var angleOutputDirectory = @"F:\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions\InterAminoAcidAngles";
            var distanceOutputDirectory = @"F:\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions\InterAminoAcidDistances";
            var perAminoAcidAngles = new Dictionary<char, List<double>>();
            var aminoAcidDistances = new Dictionary<string, List<double>>();
            foreach (var positionFile in positionFiles)
            {
                var positionHistory = new CircularBuffer<Point3D>(3);
                var angles = new List<double>();
                var lastAminoAcidLetter = ' ';
                foreach (var line in File.ReadAllLines(positionFile))
                {
                    var splittedLine = line.Split(';');
                    if(splittedLine.Length != 4)
                        continue;
                    var aminoAcidLetter = splittedLine[0][0];
                    var point = new Point3D(
                        double.Parse(splittedLine[1]),
                        double.Parse(splittedLine[2]),
                        double.Parse(splittedLine[3]));
                    positionHistory.Put(point);
                    if(!positionHistory.IsFilled)
                    {
                        lastAminoAcidLetter = aminoAcidLetter;
                        continue;
                    }
                    var positions = positionHistory.ToList();
                    var v1 = positions[0].VectorTo(positions[1]);
                    var v2 = positions[1].VectorTo(positions[2]);
                    var angle = v1.AngleWith(v2).In(Unit.Degree);
                    angles.Add(angle);
                    if(!perAminoAcidAngles.ContainsKey(lastAminoAcidLetter))
                        perAminoAcidAngles.Add(lastAminoAcidLetter, new List<double>());
                    perAminoAcidAngles[lastAminoAcidLetter].Add(angle);
                    var distance = positions[1].DistanceTo(positions[2]);
                    var aminoAcidLetterPair = "" + lastAminoAcidLetter + aminoAcidLetter;
                    if(!aminoAcidDistances.ContainsKey(aminoAcidLetterPair))
                        aminoAcidDistances.Add(aminoAcidLetterPair, new List<double>());
                    aminoAcidDistances[aminoAcidLetterPair].Add(distance);

                    lastAminoAcidLetter = aminoAcidLetter;
                }
                File.WriteAllLines(
                    Path.Combine(angleOutputDirectory, Path.GetFileName(positionFile)),
                    angles.Select(x => x.ToString("F1", CultureInfo.InvariantCulture)));
            }

            foreach (var kvp in perAminoAcidAngles)
            {
                var aminoAcidLetter = kvp.Key;
                var angles = kvp.Value;
                File.WriteAllLines(
                    Path.Combine(angleOutputDirectory, $"aminoAcid_{aminoAcidLetter}.csv"),
                    angles.Select(x => x.ToString("F1", CultureInfo.InvariantCulture)));
            }
            foreach (var kvp in aminoAcidDistances)
            {
                var aminoAcidLetterPair = kvp.Key;
                File.WriteAllLines(
                    Path.Combine(distanceOutputDirectory, aminoAcidLetterPair + ".csv"),
                    kvp.Value.Select(x => x.ToString("F1", CultureInfo.InvariantCulture)));
            }
            var combinedDistances = aminoAcidDistances.Values.SelectMany(x => x);
            File.WriteAllLines(
                Path.Combine(distanceOutputDirectory, "combined.csv"),
                combinedDistances.Select(x => x.ToString("F1", CultureInfo.InvariantCulture)));
        }

        [Test]
        public void GenerateProteins()
        {
            var angleInverseDistributions = LoadInverseAngleDistributions();
            var pdbId = "1a2b";
            var aminoAcidSequence = ReadAminoAcidSequenceFromPositionFile($@"F:\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions\pdb{pdbId}_model000.csv");
            var distanceMapsDirectory = @"F:\HumanGenome\generatedProteins\DistanceMaps";
            var positionFilesDirectory = @"F:\HumanGenome\generatedProteins\PositionFiles";
            var distanceMean = 380;
            var distanceSigma = 4;

            var iterations = 1000;
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                var aminoAcidPositions = new List<Point3D> { new Point3D(0, 0, 0)};
                var lastAminoAcidLetter = aminoAcidSequence[0];
                for (var aminoAcidIdx = 1; aminoAcidIdx < aminoAcidSequence.Count; aminoAcidIdx++)
                {
                    var aminoAcidLetter = aminoAcidSequence[aminoAcidIdx];
                    var distance = StaticRandom.NextGaussian(distanceMean, distanceSigma);
                    if (aminoAcidPositions.Count < 2)
                    {
                        aminoAcidPositions.Add(new Point3D(distance, 0, 0));
                        lastAminoAcidLetter = aminoAcidLetter;
                        continue;
                    }

                    var angleInverseDistribution = angleInverseDistributions[lastAminoAcidLetter];
                    var angleInRadians = angleInverseDistribution.ValueAtX(StaticRandom.Rng.NextDouble());
                    var lastPosition = aminoAcidPositions.Last().To(SIPrefix.Pico, Unit.Meter);
                    var vector1 = aminoAcidPositions[aminoAcidIdx - 2].VectorTo(aminoAcidPositions[aminoAcidIdx - 1]);
                    var vector2 = aminoAcidIdx >= 3
                        ? aminoAcidPositions[aminoAcidIdx - 3].VectorTo(aminoAcidPositions[aminoAcidIdx - 2])
                        : new Vector3D(0, 1, 0);
                    var bestPositionedPoint = ApproximateAminoAcidPositioner.CalculateAtomPosition(
                        lastPosition,
                        vector1,
                        vector2,
                        distance.To(SIPrefix.Pico, Unit.Meter),
                        angleInRadians.To(Unit.Radians),
                        (2 * Math.PI * (StaticRandom.Rng.NextDouble()-0.5)).To(Unit.Radians));
                    var isColliding = CollidesWithExistingPositions(bestPositionedPoint, aminoAcidPositions, out var distanceToClosest);
                    var bestDistanceToClosest = distanceToClosest;
                    const int MaxPositionIterations = 3;
                    var positionIteration = 0;
                    while (isColliding && positionIteration < MaxPositionIterations)
                    {
                        positionIteration++;
                        var pointCandidate = ApproximateAminoAcidPositioner.CalculateAtomPosition(
                            lastPosition,
                            vector1,
                            vector2,
                            distance.To(SIPrefix.Pico, Unit.Meter),
                            angleInRadians.To(Unit.Radians),
                            (2 * Math.PI * (StaticRandom.Rng.NextDouble()-0.5)).To(Unit.Radians));
                        isColliding = CollidesWithExistingPositions(bestPositionedPoint, aminoAcidPositions, out distanceToClosest);
                        if(!isColliding)
                        {
                            bestPositionedPoint = pointCandidate;
                            break;
                        }
                        if (distanceToClosest > bestDistanceToClosest)
                        {
                            bestPositionedPoint = pointCandidate;
                            bestDistanceToClosest = distanceToClosest;
                        }
                    }
                    aminoAcidPositions.Add(bestPositionedPoint.In(SIPrefix.Pico, Unit.Meter));

                    lastAminoAcidLetter = aminoAcidLetter;
                }

                var filenameBase = $"pdb{pdbId}_gen{iteration}";
                var distanceMap = DistanceMapGenerator.Generate(aminoAcidPositions);
                distanceMap.Save(Path.Combine(distanceMapsDirectory, filenameBase + ".png"));
                File.WriteAllLines(
                    Path.Combine(positionFilesDirectory, filenameBase + ".csv"),
                    Enumerable.Range(0, aminoAcidSequence.Count)
                        .Select(idx => $"{aminoAcidSequence[idx]};{aminoAcidPositions[idx].ToString()}"));
            }
        }

        private static bool CollidesWithExistingPositions(UnitPoint3D position, List<Point3D> otherPositions, out UnitValue distanceToClosest)
        {
            var positionInPicoMeter = position.In(SIPrefix.Pico, Unit.Meter);
            var closestOther = otherPositions
                .Take(otherPositions.Count - 1)
                .MinimumItem(p => p.DistanceTo(positionInPicoMeter));
            var distance = closestOther.DistanceTo(positionInPicoMeter);
            distanceToClosest = distance.To(SIPrefix.Pico, Unit.Meter);
            return distance < 390;
        }

        private static List<char> ReadAminoAcidSequenceFromPositionFile(string positionFilePath)
        {
            return File.ReadAllLines(positionFilePath)
                .Select(line => line.Split(';'))
                .Where(splittedLine => splittedLine.Length == 4)
                .Select(splittedLine => splittedLine[0][0])
                .ToList();
        }

        private static Dictionary<char, ContinuousLine2D> LoadInverseAngleDistributions()
        {
            var angleDirectory = @"F:\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions\InterAminoAcidAngles";
            var angleDistributionFilePattern = "*_probabilityDistribution.csv";
            var probabilityDistributionFiles = Directory.EnumerateFiles(angleDirectory, angleDistributionFilePattern);
            var aminoAcidLetterPattern = "aminoAcid_([A-Z])_probabilityDistribution\\.csv";
            var angleInverseDistributions = new Dictionary<char, ContinuousLine2D>();
            foreach (var probabilityDistributionFile in probabilityDistributionFiles)
            {
                var match = Regex.Match(Path.GetFileName(probabilityDistributionFile), aminoAcidLetterPattern);
                var aminoAcidLetter = match.Groups[1].Value[0];
                var angleDistributionPoints = File.ReadAllLines(probabilityDistributionFile)
                    .Select(line => line.Split(';'))
                    .Select(splittedLine => new Point2D(double.Parse(splittedLine[0]) / 100, Math.PI * double.Parse(splittedLine[1]) / 180));
                var angleInverseDistribution = new ContinuousLine2D(angleDistributionPoints);
                angleInverseDistributions.Add(aminoAcidLetter, angleInverseDistribution);
            }

            return angleInverseDistributions;
        }
    }
}
