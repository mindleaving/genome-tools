using System.Collections.Generic;
using ChemistryLibrary.IO.Pdb;
using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.Measurements
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
                    var psi = CalculateDihedralAngle(
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
                    omega = CalculateDihedralAngle(
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
                    phi = CalculateDihedralAngle(
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
                    var psi = CalculateDihedralAngle(
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
                    omega = CalculateDihedralAngle(
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
                    phi = CalculateDihedralAngle(
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

        public static UnitValue CalculateDihedralAngle(UnitPoint3D point1, 
            UnitPoint3D point2, 
            UnitPoint3D point3, 
            UnitPoint3D point4)
        {
            var vector1 = point1.VectorTo(point2);
            var vector2 = point2.VectorTo(point3);
            var vector3 = point3.VectorTo(point4);

            var plane12Normal = vector1.CrossProduct(vector2);
            var plane23Normal = vector2.CrossProduct(vector3);

            var angle = plane12Normal.AngleWith(plane23Normal);
            var sign = vector3.DotProduct(plane12Normal) > 0 ? 1 : -1;
            return sign*angle;
        }
    }
}
