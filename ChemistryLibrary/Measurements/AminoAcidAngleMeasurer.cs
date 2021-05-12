using System.Collections.Generic;
using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.IO.Pdb;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Measurements
{
    public class AminoAcidAngles
    {
        /// <summary>
        /// Dihedral angle of nitrogen and alpha-carbon of current amino acid
        /// with carbons from PREVIOUS amino acid
        /// </summary>
        public UnitValue Omega { get; set; }
        /// <summary>
        /// Dihedral angle of carbon of previous amino acid and nitrogen of current amino acid
        /// with alpha-carbon and carbon of current amino acid
        /// </summary>
        public UnitValue Phi { get; set; }
        /// <summary>
        /// Dihedral angle of nitrogen and alpha-carbon of current amino acid
        /// with carbon of current amino acid and nitrogen of next amino acid
        /// </summary>
        public UnitValue Psi { get; set; }

        public override string ToString()
        {
            return $"Omega/Phi/Psi: {Omega.In(Unit.Degree):F1}/{Phi.In(Unit.Degree):F1}/{Psi.In(Unit.Degree):F1}";
        }
    }
    public static class AminoAcidAngleMeasurer
    {
        public static Dictionary<AminoAcidReference, AminoAcidAngles> MeasureAngles(Peptide peptide)
        {
            var measurements = new Dictionary<AminoAcidReference, AminoAcidAngles>();
            AminoAcidAngles lastAngles = null;

            UnitPoint3D lastNitrogenPosition = null;
            UnitPoint3D lastCarbonAlphaPosition = null;
            UnitPoint3D lastCarbonPosition = null;
            foreach (var aminoAcid in peptide.AminoAcids)
            {
                PdbAminoAcidAtomNamer.AssignNames(aminoAcid);
                var nitrogen = aminoAcid.GetAtomFromName("N");
                var carbonAlpha = aminoAcid.GetAtomFromName("CA");
                var carbon = aminoAcid.GetAtomFromName("C");
                var nitrogenPosition = nitrogen != null && nitrogen.IsPositionFixed
                    ? nitrogen.Position
                    : null;
                var carbonAlphaPosition = carbonAlpha != null && carbonAlpha.IsPositionFixed
                    ? carbonAlpha.Position
                    : null;
                var carbonPosition = carbon != null && carbon.IsPositionFixed
                    ? carbon.Position
                    : null;

                if (lastAngles != null
                    && lastNitrogenPosition != null
                    && lastCarbonAlphaPosition != null
                    && lastCarbonPosition != null
                    && nitrogenPosition != null)
                {
                    var psi = DihedralAngleCalculator.Calculate(
                        lastNitrogenPosition,
                        lastCarbonAlphaPosition,
                        lastCarbonPosition,
                        nitrogenPosition);
                    lastAngles.Psi = psi;
                }

                UnitValue omega = null;
                if (lastCarbonAlphaPosition != null
                    && lastCarbonPosition != null
                    && nitrogenPosition != null
                    && carbonAlphaPosition != null)
                {
                    omega = DihedralAngleCalculator.Calculate(
                        lastCarbonAlphaPosition,
                        lastCarbonPosition,
                        nitrogenPosition,
                        carbonAlphaPosition);
                }
                UnitValue phi = null;
                if (lastCarbonPosition != null
                    && nitrogenPosition != null
                    && carbonAlphaPosition != null
                    && carbonPosition != null)
                {
                    phi = DihedralAngleCalculator.Calculate(
                        lastCarbonPosition,
                        nitrogenPosition,
                        carbonAlphaPosition,
                        carbonPosition);
                }

                var angles = new AminoAcidAngles
                {
                    Omega = omega,
                    Phi = phi
                };
                measurements.Add(aminoAcid, angles);

                lastNitrogenPosition = nitrogenPosition;
                lastCarbonAlphaPosition = carbonAlphaPosition;
                lastCarbonPosition = carbonPosition;
                lastAngles = angles;
            }
            return measurements;
        }

        public static Dictionary<ApproximatedAminoAcid, AminoAcidAngles> MeasureAngles(ApproximatePeptide peptide)
        {
            var measurements = new Dictionary<ApproximatedAminoAcid, AminoAcidAngles>();
            AminoAcidAngles lastAngles = null;

            UnitPoint3D lastNitrogenPosition = null;
            UnitPoint3D lastCarbonAlphaPosition = null;
            UnitPoint3D lastCarbonPosition = null;
            foreach (var aminoAcid in peptide.AminoAcids)
            {
                var nitrogenPosition = aminoAcid.NitrogenPosition;
                var carbonAlphaPosition = aminoAcid.CarbonAlphaPosition;
                var carbonPosition = aminoAcid.CarbonPosition;

                if (lastAngles != null
                    && lastNitrogenPosition != null
                    && lastCarbonAlphaPosition != null
                    && lastCarbonPosition != null
                    && nitrogenPosition != null)
                {
                    var psi = DihedralAngleCalculator.Calculate(
                        lastNitrogenPosition,
                        lastCarbonAlphaPosition,
                        lastCarbonPosition,
                        nitrogenPosition);
                    lastAngles.Psi = psi;
                }

                UnitValue omega = null;
                if (lastCarbonAlphaPosition != null
                    && lastCarbonPosition != null
                    && nitrogenPosition != null
                    && carbonAlphaPosition != null)
                {
                    omega = DihedralAngleCalculator.Calculate(
                        lastCarbonAlphaPosition,
                        lastCarbonPosition,
                        nitrogenPosition,
                        carbonAlphaPosition);
                }
                UnitValue phi = null;
                if (lastCarbonPosition != null
                    && nitrogenPosition != null
                    && carbonAlphaPosition != null
                    && carbonPosition != null)
                {
                    phi = DihedralAngleCalculator.Calculate(
                        lastCarbonPosition,
                        nitrogenPosition,
                        carbonAlphaPosition,
                        carbonPosition);
                }

                var angles = new AminoAcidAngles
                {
                    Omega = omega,
                    Phi = phi
                };
                measurements.Add(aminoAcid, angles);

                lastNitrogenPosition = nitrogenPosition;
                lastCarbonAlphaPosition = carbonAlphaPosition;
                lastCarbonPosition = carbonPosition;
                lastAngles = angles;
            }
            return measurements;
        }
    }
}
