using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Builders;
using ChemistryLibrary.DataLookups;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Physics;

namespace ChemistryLibrary.Simulation
{
    public class MultiLevelGrowingFoldingSimulatorSettings
    {
        /// <summary>
        /// Move helices and sheets every N simulation round (after N new amino acids have been added)
        /// </summary>
        public int SecondaryStructureFoldingFrequency { get; set; } = 5;
        public UnitValue InfluenceHorizont { get; set; } = 10.To(SIPrefix.Nano, Unit.Meter);
        public int PositioningAttempts { get; set; } = 30;
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
            Stack<AminoAcidName> aminoAcidQueue,
            MultiLevelGrowingFoldingSimulatorSettings settings)
        {
            var aminoAcidName = aminoAcidQueue.Pop();

            var lastAminoAcid = peptide.AminoAcids.LastOrDefault();
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

            var remainingPositionAttempts = settings.PositioningAttempts;
            do
            {
                try
                {
                    var aminoAcid = new ApproximatedAminoAcid(aminoAcidName, sequenceNumber);
                    PositionAminoAcid(aminoAcid, lastAminoAcid, ramachandranPlot, influencingAminoAcids, settings);
                    peptide.Add(aminoAcid);

                    RecursiveAddAndPositionNewAminoAcid(peptide, aminoAcidQueue, settings);
                    return;
                }
                catch (AminoAcidCannotBePositionedException)
                {
                    remainingPositionAttempts--;
                }
            } while (remainingPositionAttempts > 0);

            aminoAcidQueue.Push(aminoAcidName);
            throw new AminoAcidCannotBePositionedException(aminoAcidName, sequenceNumber);
        }

        private void PositionAminoAcid(
            ApproximatedAminoAcid aminoAcid,
            ApproximatedAminoAcid lastAminoAcid,
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

                // Note: Clashing distance was determined using MultiLevelGrowingFoldingSimulatorStudy.MeasureAverageAminoAcidDistance
                var isClashingWithAnyInfluencingAminoAcid = influencingAminoAcids
                    .Any(otherAminoAcid => otherAminoAcid.CarbonAlphaPosition.DistanceTo(aminoAcid.CarbonAlphaPosition).In(SIPrefix.Nano, Unit.Meter) < 1.0);
                if(isClashingWithAnyInfluencingAminoAcid)
                {
                    positionAttempt++;
                    continue;
                }

                if (lastAminoAcid != null && influencingAminoAcids.Count > 0)
                {
                    var isTooFarAway = aminoAcid.CarbonAlphaPosition.DistanceTo(influencingCenter) > lastAminoAcid.CarbonAlphaPosition.DistanceTo(influencingCenter)
                                       && aminoAcid.CarbonAlphaPosition.DistanceTo(influencingCenter) > 0.8 * settings.InfluenceHorizont;
                    if (isTooFarAway)
                    {
                        positionAttempt++;
                        continue;
                    }
                }
                return;
            } while (positionAttempt < settings.PositioningAttempts);

            throw new AminoAcidCannotBePositionedException(aminoAcid.Name, aminoAcid.SequenceNumber);
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
