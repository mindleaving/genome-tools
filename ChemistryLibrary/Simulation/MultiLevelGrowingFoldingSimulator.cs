using System;
using System.Collections.Generic;
using System.Linq;
using Commons;
using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Builders;
using GenomeTools.ChemistryLibrary.DataLookups;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation
{
    public class MultiLevelGrowingFoldingSimulatorSettings
    {
        /// <summary>
        /// Move helices and sheets every N simulation round (after N new amino acids have been added)
        /// </summary>
        public int SecondaryStructureFoldingFrequency { get; set; } = 5;
        public UnitValue InfluenceHorizont { get; set; } = 10.To(SIPrefix.Nano, Unit.Meter);
        public int TipPositioningAttempts { get; set; } = 10;
        public int RecursivePositioningAttempts { get; set; } = 30;
        public double HydrogenBondBreakProbability { get; set; } = 0.4;
    }
    public class MultiLevelGrowingFoldingSimulator
    {
        private readonly RamachandranPlotFileSource ramachandranPlotSource;

        public MultiLevelGrowingFoldingSimulator(string distributionDirectory)
        {
            ramachandranPlotSource = new RamachandranPlotFileSource(distributionDirectory);
        }

        public ApproximatePeptide Simulate(
            IList<AminoAcidName> aminoAcidSequence,
            MultiLevelGrowingFoldingSimulatorSettings settings)
        {
            var peptide = new ApproximatePeptide();
            var aminoAcidStack = new Stack<AminoAcidName>(aminoAcidSequence.Reverse()); // See MultiLevelGrowingFoldingSimulatorStudy.StackInit
            RecursiveAddAndPositionNewAminoAcid(peptide, aminoAcidStack, settings);
            FoldSecondaryStructure(peptide);

            return peptide;
        }

        private void RecursiveAddAndPositionNewAminoAcid(
            ApproximatePeptide peptide,
            Stack<AminoAcidName> aminoAcidStack,
            MultiLevelGrowingFoldingSimulatorSettings settings)
        {
            var aminoAcidName = aminoAcidStack.Pop();

            var lastAminoAcid = peptide.AminoAcids.LastOrDefault();
            var nextAminoAcid = aminoAcidStack.Count > 0 ? (AminoAcidName?)aminoAcidStack.Peek() : null;
            var lastPosition = lastAminoAcid?.CarbonAlphaPosition ?? new UnitPoint3D(Unit.Meter, 0, 0, 0);
            var influencingAminoAcids = peptide.AminoAcids
                .Where(x => x.CarbonAlphaPosition.DistanceTo(lastPosition) < settings.InfluenceHorizont)
                .ToList();
            var ramachandranPlot = ramachandranPlotSource.Get(aminoAcidName);
            var sequenceNumber = lastAminoAcid?.SequenceNumber + 1 ?? 0;

            var isTimeForSecondaryStructureFolding = 
                sequenceNumber % settings.SecondaryStructureFoldingFrequency == 0;
            if (isTimeForSecondaryStructureFolding)
            {
                FoldSecondaryStructure(peptide);
            }

            var remainingPositionAttempts = settings.RecursivePositioningAttempts;
            do
            {
                var aminoAcid = new ApproximatedAminoAcid(aminoAcidName, sequenceNumber);
                Console.WriteLine($"Positioning amino acid {peptide.AminoAcids.Count+1}");
                PositionAminoAcid(aminoAcid, lastAminoAcid, nextAminoAcid, ramachandranPlot, influencingAminoAcids, settings);
                peptide.Add(aminoAcid);

                try
                {
                    if(aminoAcidStack.Count > 0)
                        RecursiveAddAndPositionNewAminoAcid(peptide, aminoAcidStack, settings);
                    return;
                }
                catch (AminoAcidCannotBePositionedException)
                {
                    peptide.RemoveLast();
                    remainingPositionAttempts--;
                }
            } while (remainingPositionAttempts > 0);

            aminoAcidStack.Push(aminoAcidName);
            throw new AminoAcidCannotBePositionedException(aminoAcidName, sequenceNumber);
        }

        private void PositionAminoAcid(
            ApproximatedAminoAcid aminoAcid,
            ApproximatedAminoAcid lastAminoAcid,
            AminoAcidName? nextAminoAcidName,
            RamachandranPlot ramachandranPlot,
            List<ApproximatedAminoAcid> influencingAminoAcids,
            MultiLevelGrowingFoldingSimulatorSettings settings)
        {
            var influencingCenter = CalculateCenter(influencingAminoAcids);

            var positionAttempt = 1;
            do
            {
                var aminoAcidAngles = ramachandranPlot.GetRandomPhiPsi();
                aminoAcid.OmegaAngle = aminoAcidAngles.Omega;
                aminoAcid.PhiAngle = aminoAcidAngles.Phi;
                aminoAcid.PsiAngle = aminoAcidAngles.Psi;
                ApproximateAminoAcidPositioner.PositionAminoAcid(aminoAcid, lastAminoAcid, new UnitPoint3D(Unit.Meter, 0, 0, 0));

                var canFormHydrogenBond = CanFormHydrogenBond(aminoAcid, nextAminoAcidName, influencingAminoAcids);
                if (canFormHydrogenBond)
                {
                    if(StaticRandom.Rng.NextDouble() < 1 - settings.HydrogenBondBreakProbability)
                    {
                        Console.WriteLine("Position attempt succeeded: Hydrogen bond formed");
                        return;
                    }
                }

                // Note: Clashing distance was determined using MultiLevelGrowingFoldingSimulatorStudy.MeasureAverageAminoAcidDistance
                var isClashingWithAnyInfluencingAminoAcid = influencingAminoAcids
                    .Any(otherAminoAcid => otherAminoAcid.Equals(lastAminoAcid)
                        ? otherAminoAcid.CarbonAlphaPosition.DistanceTo(aminoAcid.CarbonAlphaPosition).In(SIPrefix.Pico, Unit.Meter) < 320.0
                        : otherAminoAcid.CarbonAlphaPosition.DistanceTo(aminoAcid.CarbonAlphaPosition).In(SIPrefix.Pico, Unit.Meter) < 450.0);
                if(isClashingWithAnyInfluencingAminoAcid)
                {
                    positionAttempt++;
                    Console.WriteLine("Position attempt failed: Clash detected");
                    continue;
                }

                if (lastAminoAcid != null && influencingAminoAcids.Count > 0)
                {
                    var isTooFarAway = aminoAcid.CarbonAlphaPosition.DistanceTo(influencingCenter) > lastAminoAcid.CarbonAlphaPosition.DistanceTo(influencingCenter)
                                       && aminoAcid.CarbonAlphaPosition.DistanceTo(influencingCenter) > 0.8 * settings.InfluenceHorizont;
                    if (isTooFarAway)
                    {
                        positionAttempt++;
                        Console.WriteLine("Position attempt failed: Too far away");
                        continue;
                    }
                }
                return;
            } while (positionAttempt < settings.TipPositioningAttempts);

            throw new AminoAcidCannotBePositionedException(aminoAcid.Name, aminoAcid.SequenceNumber);
        }

        private static bool CanFormHydrogenBond(ApproximatedAminoAcid aminoAcid,
            AminoAcidName? nextAminoAcidName,
            List<ApproximatedAminoAcid> influencingAminoAcids)
        {
            return (nextAminoAcidName == null || nextAminoAcidName != AminoAcidName.Proline) && influencingAminoAcids.FirstOrDefault(
                otherAminoAcid => aminoAcid.NextNitrogenPosition.DistanceTo(otherAminoAcid.OxygenPosition)
                                      .In(SIPrefix.Pico, Unit.Meter)
                                  < 320.0) != null;
        }

        private UnitPoint3D CalculateCenter(List<ApproximatedAminoAcid> aminoAcids)
        {
            var center = new UnitPoint3D(Unit.Meter, 0, 0, 0);
            foreach (var aminoAcid in aminoAcids)
            {
                center += aminoAcid.CarbonAlphaPosition;
            }

            return (1.0 / aminoAcids.Count) * center;
        }

        private void FoldSecondaryStructure(ApproximatePeptide peptide)
        {
            // Do nothing for now
        }
    }
}
