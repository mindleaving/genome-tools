using System;
using System.Collections.Generic;
using ChemistryLibrary.DataLookups;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;

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
            var omega = aminoAcid.OmegaAngle ?? Math.PI.To(Unit.Radians);
            var phi = aminoAcid.PhiAngle ?? 0.To(Unit.Radians);
            var psi = lastAminoAcid?.PsiAngle ?? 0.To(Unit.Radians);
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
                aminoAcid.OmegaAngle = omega;
            if (aminoAcid.PhiAngle == null)
                aminoAcid.PhiAngle = phi;
            if (lastAminoAcid != null && lastAminoAcid.PsiAngle == null)
                lastAminoAcid.PsiAngle = psi;
        }

        private static UnitPoint3D CalculateAtomPosition(UnitPoint3D currentPosition,
            Vector3D vector1,
            Vector3D vector2,
            UnitValue bondLength,
            UnitValue bondAngle,
            UnitValue bondTorsion)
        {
            var basisVector1 = vector1.Normalize();
            var normalizedVector2 = vector2.Normalize();
            var basisVector2 = (normalizedVector2 - basisVector1.DotProduct(normalizedVector2) * basisVector1).Normalize();
            var basisVector3 = basisVector1.CrossProduct(basisVector2);

            var bondAngleRadians = bondAngle.In(Unit.Radians);
            var bondTorsionRadians = bondTorsion.In(Unit.Radians);
            var cosBondAngle = Math.Cos(bondAngleRadians);
            var cosBondTorsion = Math.Cos(bondTorsionRadians);
            var bondVector = bondLength.In(SIPrefix.Pico, Unit.Meter) * new Vector3D(
                             -cosBondAngle,
                             cosBondTorsion*Math.Sqrt(1-cosBondAngle*cosBondAngle),
                             Math.Sqrt(1 - cosBondTorsion*cosBondTorsion-cosBondAngle*cosBondAngle*(1-cosBondTorsion*cosBondTorsion)));

            var transformMatrix = new Matrix3X3();
            transformMatrix.SetColumn(0, basisVector1.Data);
            transformMatrix.SetColumn(1, basisVector2.Data);
            transformMatrix.SetColumn(2, basisVector3.Data);
            var bondDirection = transformMatrix.Data.Multiply(bondVector.Data.ConvertToMatrix()).Vectorize();

            // Debug
            var actualBondAngle = (-basisVector1).AngleWith(new Vector3D(bondDirection));
            var actualTorsionAngle = new Vector3D(bondDirection).CrossProduct(basisVector1).AngleWith(-basisVector3);
            if ((actualBondAngle - bondAngle).Abs().In(Unit.Degree) > 1)
                throw new Exception("Bug!");
            if ((actualTorsionAngle - bondTorsion).Abs().In(Unit.Degree) > 1)
                throw new Exception("Bug!");

            var atomPosition = currentPosition + new UnitVector3D(SIPrefix.Pico, Unit.Meter, bondDirection[0], bondDirection[1], bondDirection[2]);
            return atomPosition;
        }
    }
}
