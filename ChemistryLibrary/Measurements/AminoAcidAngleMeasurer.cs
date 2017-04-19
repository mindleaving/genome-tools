using System.Collections.Generic;
using ChemistryLibrary.Pdb;
using Commons;

namespace ChemistryLibrary.Measurements
{
    public class AminoAcidAngles
    {
        public UnitValue Omega { get; set; }
        public UnitValue Phi { get; set; }
        public UnitValue Psi { get; set; }
    }
    public class AminoAcidAngleMeasurer
    {
        public Dictionary<AminoAcidReference, AminoAcidAngles> MeasureAngles(Peptide peptide)
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
                    var psi = MeasureAnglesFromPositions(
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
                    omega = MeasureAnglesFromPositions(
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
                    phi = MeasureAnglesFromPositions(
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

        private UnitValue MeasureAnglesFromPositions(UnitPoint3D point1, 
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
