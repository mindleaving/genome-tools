using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.DataLookups;
using GenomeTools.ChemistryLibrary.Measurements;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation
{
    public class BondForceCalculator
    {
        public Dictionary<ApproximatedAminoAcid, ApproximateAminoAcidForces> Calculate(ApproximatePeptide peptide)
        {
            // VALIDATED: Forces point in the right direction and restore expected bond length

            var bondForces = peptide.AminoAcids.ToDictionary(aa => aa, aa => new ApproximateAminoAcidForces());
            for (var aminoAcidIdx = 0; aminoAcidIdx < peptide.AminoAcids.Count; aminoAcidIdx++)
            {
                var aminoAcid = peptide.AminoAcids[aminoAcidIdx];
                var previousAminoAcid = aminoAcidIdx > 0 
                    ? peptide.AminoAcids[aminoAcidIdx - 1] 
                    : null;

                CalculateBondLengthForces(previousAminoAcid, aminoAcid, bondForces);
                CalculateBondAngleForces(previousAminoAcid, aminoAcid, bondForces);
            }
            return bondForces;
        }

        private void CalculateBondLengthForces(ApproximatedAminoAcid previousAminoAcid, ApproximatedAminoAcid aminoAcid,
            Dictionary<ApproximatedAminoAcid, ApproximateAminoAcidForces> bondForces)
        {
            var nitrogenCarbonBondLength = BondLengthCalculator.CalculateApproximate(ElementName.Nitrogen, ElementName.Carbon);
            var carbonCarbonBondLength = BondLengthCalculator.CalculateApproximate(ElementName.Carbon, ElementName.Carbon);

            var aminoAcidForces = bondForces[aminoAcid];
            // Carbon - nitrogen bond
            if (previousAminoAcid != null)
            {
                var carbonNitrogenForceVector = CalculateBondLengthForce(previousAminoAcid.CarbonPosition,
                    aminoAcid.NitrogenPosition,
                    nitrogenCarbonBondLength);
                var previousAminoAcidForces = bondForces[previousAminoAcid];
                previousAminoAcidForces.CarbonForce += carbonNitrogenForceVector;
                aminoAcidForces.NitrogenForce += -carbonNitrogenForceVector;
            }

            // Nitrogen - carbon alpha bond
            var nitrogenCarbonAlphaForceVector = CalculateBondLengthForce(aminoAcid.NitrogenPosition,
                aminoAcid.CarbonAlphaPosition,
                nitrogenCarbonBondLength);
            aminoAcidForces.NitrogenForce += nitrogenCarbonAlphaForceVector;
            aminoAcidForces.CarbonAlphaForce += -nitrogenCarbonAlphaForceVector;

            // Carbon alpha - carbon bond
            var carbonAlphaCarbonForceVector = CalculateBondLengthForce(aminoAcid.CarbonAlphaPosition,
                aminoAcid.CarbonPosition,
                carbonCarbonBondLength);
            aminoAcidForces.CarbonAlphaForce += carbonAlphaCarbonForceVector;
            aminoAcidForces.CarbonForce += -carbonAlphaCarbonForceVector;
        }

        private UnitVector3D CalculateBondLengthForce(UnitPoint3D atom1Position, UnitPoint3D atom2Position, UnitValue equilibriumBondLength)
        {
            // Settings
            const double BondSpringConstant = 1e4;

            var atomDistance = atom1Position.DistanceTo(atom2Position);
            var deviationFromEquilibrium = atomDistance - equilibriumBondLength;
            var forceMagnitude = (BondSpringConstant * deviationFromEquilibrium.In(Unit.Meter)).To(Unit.Newton);
            var forceDirection = atom1Position.VectorTo(atom2Position).Normalize().ToVector3D();
            return forceMagnitude * forceDirection;
        }

        private void CalculateBondAngleForces(ApproximatedAminoAcid previousAminoAcid, ApproximatedAminoAcid aminoAcid,
            Dictionary<ApproximatedAminoAcid, ApproximateAminoAcidForces> bondForces)
        {
            const double RestoreForceScaling = 1e-10;

            var aminoAcidForces = bondForces[aminoAcid];
            var nitrogenCarbonAlphaVector = aminoAcid.NitrogenPosition.VectorTo(aminoAcid.CarbonAlphaPosition);

            if (previousAminoAcid != null)
            {
                var previousAminoAcidForces = bondForces[previousAminoAcid];

                // Carbon alpha - carbon - nitrogen bond pair
                var carbonAlphaCarbonVector = previousAminoAcid.CarbonAlphaPosition
                    .VectorTo(previousAminoAcid.CarbonPosition);
                var carbonNitrogenVector = previousAminoAcid.CarbonPosition
                    .VectorTo(aminoAcid.NitrogenPosition);
                var cacnAngle = 180.To(Unit.Degree) - carbonAlphaCarbonVector.AngleWith(carbonNitrogenVector);
                var cacnPlaneNormal = carbonAlphaCarbonVector.CrossProduct(carbonNitrogenVector);
                var cacnForceMagnitude = RestoreForceScaling *
                                         (cacnAngle.In(Unit.Degree) - AminoAcidBondAngles.CaCNAngle.In(Unit.Degree))
                                         .To(Unit.Newton);
                var carbonAlpha1ForceDirection = -carbonAlphaCarbonVector.CrossProduct(cacnPlaneNormal).Normalize().ToVector3D();
                var nitrogenForceDirection = -carbonNitrogenVector.CrossProduct(cacnPlaneNormal).Normalize().ToVector3D();
                previousAminoAcidForces.CarbonAlphaForce += cacnForceMagnitude * carbonAlpha1ForceDirection;
                previousAminoAcidForces.CarbonForce += -(cacnForceMagnitude * carbonAlpha1ForceDirection
                                                       + cacnForceMagnitude * nitrogenForceDirection);
                aminoAcidForces.NitrogenForce += cacnForceMagnitude * nitrogenForceDirection;

                // Carbon - nitrogen - carbon alpha bond pair
                var cncaAngle = 180.To(Unit.Degree) - carbonNitrogenVector.AngleWith(nitrogenCarbonAlphaVector);
                var cncaPlaneNormal = carbonNitrogenVector.CrossProduct(nitrogenCarbonAlphaVector);
                var cncaForceMagnitude = RestoreForceScaling *
                                         (cncaAngle.In(Unit.Degree) - AminoAcidBondAngles.CNCaAngle.In(Unit.Degree))
                                         .To(Unit.Newton);
                var carbonForceDirection = -carbonNitrogenVector.CrossProduct(cncaPlaneNormal).Normalize().ToVector3D();
                var carbonAlpha2ForceDirection = -nitrogenCarbonAlphaVector.CrossProduct(cncaPlaneNormal).Normalize().ToVector3D();
                previousAminoAcidForces.CarbonForce += cncaForceMagnitude * carbonForceDirection;
                aminoAcidForces.NitrogenForce += -(cncaForceMagnitude * carbonForceDirection +
                                                   cncaForceMagnitude * carbonAlpha2ForceDirection);
                aminoAcidForces.CarbonAlphaForce += cncaForceMagnitude * carbonAlpha2ForceDirection;
            }

            // Nitrogen - carbon alpha - carbon bond pair
            var carbonAlphaCarbon2Vector = aminoAcid.CarbonAlphaPosition.VectorTo(aminoAcid.CarbonPosition);
            var ncacAngle = 180.To(Unit.Degree) - nitrogenCarbonAlphaVector.AngleWith(carbonAlphaCarbon2Vector);
            var ncacPlaneNormal = nitrogenCarbonAlphaVector.CrossProduct(carbonAlphaCarbon2Vector);
            var ncacForceMagnitude = RestoreForceScaling *
                                     (ncacAngle.In(Unit.Degree) - AminoAcidBondAngles.NCaCAngle.In(Unit.Degree))
                                     .To(Unit.Newton);
            var nitrogenForceDirection2 = -nitrogenCarbonAlphaVector.CrossProduct(ncacPlaneNormal).Normalize().ToVector3D();
            var carbonForceDirection2 = -carbonAlphaCarbon2Vector.CrossProduct(ncacPlaneNormal).Normalize().ToVector3D();
            aminoAcidForces.NitrogenForce += ncacForceMagnitude * nitrogenForceDirection2;
            aminoAcidForces.CarbonAlphaForce += -(ncacForceMagnitude * nitrogenForceDirection2 +
                                                  ncacForceMagnitude * carbonForceDirection2);
            aminoAcidForces.CarbonForce += ncacForceMagnitude * carbonForceDirection2;
        }
    }
}