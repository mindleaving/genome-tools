using System;
using System.Collections.Generic;
using ChemistryLibrary.DataLookups;
using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.Builders
{
    public static class ApproximateAminoAcidPositioner
    {
        public static void Position(IList<ApproximatedAminoAcid> aminoAcids, UnitPoint3D startPosition)
        {
            ApproximatedAminoAcid lastAminoAcid = null;
            foreach (var aminoAcid in aminoAcids)
            {
                PositionAminoAcid(aminoAcid, lastAminoAcid, startPosition);
                lastAminoAcid = aminoAcid;
            }
        }

        private static void PositionAminoAcid(ApproximatedAminoAcid aminoAcid, 
            ApproximatedAminoAcid lastAminoAcid,
            UnitPoint3D startPosition)
        {
            var nitrogenCarbonDistance = PeriodicTable.GetRadius(ElementName.Nitrogen) +
                                         PeriodicTable.GetRadius(ElementName.Carbon);
            var CaCDistance = 2 * PeriodicTable.GetRadius(ElementName.Carbon);

            UnitPoint3D carbonPosition;
            UnitPoint3D carbonAlphaPosition;
            Vector3D carbonAlphaCarbonBondDirection;
            Vector3D nitrogenCarbonAlphaBondDirection;
            if (lastAminoAcid == null)
            {
                carbonPosition = startPosition;
                carbonAlphaPosition = startPosition + new UnitPoint3D(SIPrefix.Pico, Unit.Meter, -CaCDistance.In(SIPrefix.Pico, Unit.Meter), 0, 0);
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
            var omega = aminoAcid.OmegaAngle?.In(Unit.Radians) ?? 180 * Math.PI / 180;
            var phi = aminoAcid.PhiAngle?.In(Unit.Radians) ?? 0;
            var psi = lastAminoAcid?.PsiAngle?.In(Unit.Radians) ?? 0;
            var nitrogenPosition = CalculateAtomPosition(carbonPosition,
                carbonAlphaCarbonBondDirection,
                nitrogenCarbonAlphaBondDirection,
                nitrogenCarbonDistance,
                AminoAcidBondAngles.CaCNAngle,
                psi);
            carbonAlphaPosition = CalculateAtomPosition(nitrogenPosition,
                carbonPosition.VectorTo(nitrogenPosition),
                carbonAlphaCarbonBondDirection,
                nitrogenCarbonDistance,
                AminoAcidBondAngles.CNCaAngle,
                omega);
            carbonPosition = CalculateAtomPosition(carbonAlphaPosition,
                nitrogenPosition.VectorTo(carbonAlphaPosition),
                carbonPosition.VectorTo(nitrogenPosition),
                CaCDistance,
                AminoAcidBondAngles.NCaCAngle,
                phi);
            aminoAcid.NitrogenPosition = nitrogenPosition;
            aminoAcid.CarbonAlphaPosition = carbonAlphaPosition;
            aminoAcid.CarbonPosition = carbonPosition;
            if (aminoAcid.OmegaAngle == null)
                aminoAcid.OmegaAngle = omega.To(Unit.Radians);
            if (aminoAcid.PhiAngle == null)
                aminoAcid.PhiAngle = phi.To(Unit.Radians);
            if (lastAminoAcid != null && lastAminoAcid.PsiAngle == null)
                lastAminoAcid.PsiAngle = psi.To(Unit.Radians);
        }

        private static UnitPoint3D CalculateAtomPosition(UnitPoint3D currentPosition,
            Vector3D vector1,
            Vector3D vector2,
            UnitValue bondLength,
            UnitValue bondAngle,
            double bondTorsion)
        {
            var basisVector1 = vector1.Normalize();
            var normalizedVector2 = vector2.Normalize();
            var basisVector2 = (normalizedVector2 - basisVector1.DotProduct(normalizedVector2) * basisVector1).Normalize();
            var basisVector3 = basisVector1.CrossProduct(basisVector2);

            var bondAngleRadians = bondAngle.In(Unit.Radians);
            var bondVector = bondLength.In(SIPrefix.Pico, Unit.Meter) * new Vector3D(
                                 -Math.Cos(bondAngleRadians),
                                 -Math.Sin(bondAngleRadians) * Math.Cos(bondTorsion),
                                 -Math.Sin(bondAngleRadians) * Math.Sin(bondTorsion));

            var transformMatrix = new Matrix3X3();
            transformMatrix.SetColumn(0, basisVector1.Data);
            transformMatrix.SetColumn(1, basisVector2.Data);
            transformMatrix.SetColumn(2, basisVector3.Data);
            var bondDirection = transformMatrix.Data.Multiply(bondVector.Data.ConvertToMatrix()).Vectorize();
            //var v4 = new Vector3D(bondDirection);
            //var v4Angle = v4.CrossProduct(vector1).AngleWith(basisVector3);
            //if(Math.Abs(v4Angle.In(Unit.Radians) - bondTorsion) > 1e-3)
            //    Debugger.Break();
            var atomPosition = currentPosition + new UnitVector3D(SIPrefix.Pico, Unit.Meter, bondDirection[0], bondDirection[1], bondDirection[2]);
            return atomPosition;
        }
    }
}
