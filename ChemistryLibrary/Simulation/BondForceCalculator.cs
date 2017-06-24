using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Measurements;
using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.Simulation
{
    public class BondForceCalculator
    {
        public Dictionary<ApproximatedAminoAcid, ApproximateAminoAcidForces> Calculate(ApproximatePeptide peptide)
        {
            var bondForces = peptide.AminoAcids.ToDictionary(aa => aa, aa => new ApproximateAminoAcidForces());
            var nitrogenCarbonBondLength = BondLengthCalculator.CalculateApproximate(ElementName.Nitrogen, ElementName.Carbon);
            var carbonCarbonBondLength = BondLengthCalculator.CalculateApproximate(ElementName.Carbon, ElementName.Carbon);
            for (var aminoAcidIdx = 0; aminoAcidIdx < peptide.AminoAcids.Count; aminoAcidIdx++)
            {
                var aminoAcid = peptide.AminoAcids[aminoAcidIdx];
                var previousAminoAcid = aminoAcidIdx > 0 
                    ? peptide.AminoAcids[aminoAcidIdx - 1] 
                    : null;
                var aminoAcidForces = bondForces[aminoAcid];

                // Carbon - nitrogen bond
                if (previousAminoAcid != null)
                {
                    var carbonNitrogenForceVector = CalculateBondForce(previousAminoAcid.CarbonPosition, 
                        aminoAcid.NitrogenPosition, 
                        nitrogenCarbonBondLength);
                    var previousAminoAcidForces = bondForces[previousAminoAcid];
                    previousAminoAcidForces.CarbonForce += carbonNitrogenForceVector;
                    aminoAcidForces.NitrogenForce += -carbonNitrogenForceVector;
                }

                // Nitrogen - carbon alpha bond
                var nitrogenCarbonAlphaForceVector = CalculateBondForce(aminoAcid.NitrogenPosition,
                    aminoAcid.CarbonAlphaPosition,
                    nitrogenCarbonBondLength);
                aminoAcidForces.NitrogenForce += nitrogenCarbonAlphaForceVector;
                aminoAcidForces.CarbonAlphaForce += -nitrogenCarbonAlphaForceVector;

                // Carbon alpha - carbon bond
                var carbonAlphaCarbonForceVector = CalculateBondForce(aminoAcid.CarbonAlphaPosition,
                    aminoAcid.CarbonPosition,
                    carbonCarbonBondLength);
                aminoAcidForces.CarbonAlphaForce += carbonAlphaCarbonForceVector;
                aminoAcidForces.CarbonForce += -carbonAlphaCarbonForceVector;

            }
            return bondForces;
        }

        private UnitVector3D CalculateBondForce(UnitPoint3D atom1Position, UnitPoint3D atom2Position, UnitValue equilibriumBondLength)
        {
            // Settings
            const double bondSpringConstant = 1e6;

            var atomDistance = atom1Position.DistanceTo(atom2Position);
            var deviationFromEquilibrium = atomDistance - equilibriumBondLength;
            var forceMagnitude = (bondSpringConstant * deviationFromEquilibrium.In(Unit.Meter)).To(Unit.Newton);
            var forceDirection = atom1Position.VectorTo(atom2Position).Normalize();
            return forceMagnitude * forceDirection;
        }
    }
}