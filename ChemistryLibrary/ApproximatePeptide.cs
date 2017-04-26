using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Commons;

namespace ChemistryLibrary
{
    public class ApproximatePeptide
    {
        public ApproximatePeptide(IList<AminoAcidName> aminoAcidNames)
        {
            BuildPeptide(aminoAcidNames);
        }

        public ApproximatePeptide(string sequence)
        {
            var cleanedPeptideString = Regex.Replace(sequence.ToUpperInvariant(), "[^A-Z]", "");
            var aminoAcidNames = cleanedPeptideString.Select(str => str.ToAminoAcidName()).ToList();
            BuildPeptide(aminoAcidNames);
        }

        private void BuildPeptide(IList<AminoAcidName> aminoAcidNames)
        {
            foreach (var aminoAcidName in aminoAcidNames)
            {
                var aminoAcid = new ApproximatedAminoAcid(aminoAcidName);
                PositionAminoAcid(aminoAcid, AminoAcids.LastOrDefault());
                AminoAcids.Add(aminoAcid);
            }
        }

        private void PositionAminoAcid(ApproximatedAminoAcid aminoAcid, ApproximatedAminoAcid lastAminoAcid)
        {
            var nitrogenCarbonDistance = PeriodicTable.GetRadius(ElementName.Nitrogen) +
                              PeriodicTable.GetRadius(ElementName.Carbon);
            var CaCDistance = 2 * PeriodicTable.GetRadius(ElementName.Carbon);
            var NCaCAngle = 109.5*Math.PI/180;
            var CaCNAngle = 116.0*Math.PI/180;
            var CNCaAngle = 122.0*Math.PI/180;

            Point3D carbonPosition;
            Point3D carbonAlphaPosition;
            Vector3D carbonAlphaCarbonBondDirection;
            Vector3D nitrogenCarbonAlphaBondDirection;
            if (lastAminoAcid == null)
            {
                carbonPosition = new Point3D(0, 0, 0);
                carbonAlphaPosition = new Point3D(-CaCDistance.In(SIPrefix.Pico, Unit.Meter), 0, 0);
                carbonAlphaCarbonBondDirection = carbonAlphaPosition.VectorTo(carbonPosition);
                nitrogenCarbonAlphaBondDirection = new Vector3D(0, 1, 0);
            }
            else
            {
                carbonPosition = lastAminoAcid.CarbonPosition;
                carbonAlphaPosition = lastAminoAcid.CarbonAlphaPosition;
                carbonAlphaCarbonBondDirection = carbonAlphaPosition
                    .VectorTo(carbonPosition);
                nitrogenCarbonAlphaBondDirection = lastAminoAcid.NitrogenPosition
                    .VectorTo(carbonAlphaPosition);
            }
            var omega = 0*Math.PI/180;
            var phi = 0*Math.PI/180;
            var psi = -10*Math.PI/180;
            var nitrogenPosition = CalculateAtomPosition(carbonPosition, 
                carbonAlphaCarbonBondDirection, 
                nitrogenCarbonAlphaBondDirection, 
                nitrogenCarbonDistance, 
                CaCNAngle, 
                psi);
            carbonAlphaPosition = CalculateAtomPosition(nitrogenPosition,
                carbonPosition.VectorTo(nitrogenPosition),
                carbonAlphaCarbonBondDirection,
                nitrogenCarbonDistance,
                CNCaAngle,
                omega);
            carbonPosition = CalculateAtomPosition(carbonAlphaPosition,
                nitrogenPosition.VectorTo(carbonAlphaPosition),
                carbonPosition.VectorTo(nitrogenPosition),
                CaCDistance,
                NCaCAngle,
                phi);
            aminoAcid.NitrogenPosition = nitrogenPosition;
            aminoAcid.CarbonAlphaPosition = carbonAlphaPosition;
            aminoAcid.CarbonPosition = carbonPosition;
            aminoAcid.PhiAngle = phi.To(Unit.Radians);
            aminoAcid.PsiAngle = psi.To(Unit.Radians);
        }

        private Point3D CalculateAtomPosition(Point3D currentPosition, 
            Vector3D vector1, 
            Vector3D vector2, 
            UnitValue bondLength, 
            double bondAngle, 
            double bondTorsion)
        {
            var basisVector1 = vector1.Normalize();
            var normalizedVector2 = vector2.Normalize();
            var basisVector2 = (normalizedVector2 - basisVector1.DotProduct(normalizedVector2)*basisVector1).Normalize();
            var basisVector3 = basisVector1.CrossProduct(basisVector2);

            var bondVector = bondLength.In(SIPrefix.Pico, Unit.Meter)*new Vector3D(
                                 -Math.Cos(bondAngle),
                                 Math.Sin(bondAngle)*Math.Cos(bondTorsion),
                                 Math.Sin(bondAngle)*Math.Sin(bondTorsion));
            var transformMatrix = new Matrix3X3();
            transformMatrix.SetColumn(0, basisVector1.Data);
            transformMatrix.SetColumn(1, basisVector2.Data);
            transformMatrix.SetColumn(2, basisVector3.Data);
            return currentPosition + new Vector3D(transformMatrix.Data.Multiply(bondVector.Data.ConvertToMatrix()).Vectorize());
        }

        public List<ApproximatedAminoAcid> AminoAcids { get; } = new List<ApproximatedAminoAcid>();
    }
}
