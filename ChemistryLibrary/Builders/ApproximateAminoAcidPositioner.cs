using System;
using System.Collections.Generic;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.DataLookups;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Builders
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

        public static void PositionAminoAcid(ApproximatedAminoAcid aminoAcid, 
            ApproximatedAminoAcid lastAminoAcid = null,
            UnitPoint3D startPosition = null)
        {
            if(lastAminoAcid == null && startPosition == null)
                throw new ArgumentNullException("Either last amino acid or start position must be non-null");
            var nitrogenCarbonDistance = PeriodicTable.GetCovalentRadius(ElementName.Nitrogen) +
                                         PeriodicTable.GetCovalentRadius(ElementName.Carbon);
            var CaCDistance = 2 * PeriodicTable.GetCovalentRadius(ElementName.Carbon);
            var CODistance = PeriodicTable.GetCovalentRadius(ElementName.Carbon) + PeriodicTable.GetCovalentRadius(ElementName.Oxygen);

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
            var nitrogenPosition = lastAminoAcid?.NextNitrogenPosition
                ?? CalculateAtomPosition(carbonPosition,
                carbonAlphaCarbonBondDirection,
                nitrogenCarbonAlphaBondDirection,
                nitrogenCarbonDistance,
                AminoAcidBondAngles.CaCNAngle,
                psi);
            carbonAlphaPosition = CalculateAtomPosition(
                nitrogenPosition,
                carbonPosition.VectorTo(nitrogenPosition),
                carbonAlphaCarbonBondDirection,
                nitrogenCarbonDistance,
                AminoAcidBondAngles.CNCaAngle,
                omega);
            carbonPosition = CalculateAtomPosition(
                carbonAlphaPosition,
                nitrogenPosition.VectorTo(carbonAlphaPosition),
                carbonPosition.VectorTo(nitrogenPosition),
                CaCDistance,
                AminoAcidBondAngles.NCaCAngle,
                phi);
            var nextNitrogenPosition = CalculateAtomPosition(
                carbonPosition,
                carbonAlphaPosition.VectorTo(carbonPosition),
                nitrogenPosition.VectorTo(carbonAlphaPosition),
                nitrogenCarbonDistance,
                AminoAcidBondAngles.CaCNAngle,
                aminoAcid.PsiAngle);
            var oxygenPosition = CalculateOxygenPosition(carbonPosition, carbonAlphaPosition, nextNitrogenPosition, CODistance);

            aminoAcid.NitrogenPosition = nitrogenPosition;
            aminoAcid.CarbonAlphaPosition = carbonAlphaPosition;
            aminoAcid.CarbonPosition = carbonPosition;
            aminoAcid.NextNitrogenPosition = nextNitrogenPosition;
            aminoAcid.OxygenPosition = oxygenPosition;

            if (aminoAcid.OmegaAngle == null)
                aminoAcid.OmegaAngle = omega;
            if (aminoAcid.PhiAngle == null)
                aminoAcid.PhiAngle = phi;
            if (lastAminoAcid != null && lastAminoAcid.PsiAngle == null)
                lastAminoAcid.PsiAngle = psi;
        }

        private static UnitPoint3D CalculateOxygenPosition(UnitPoint3D carbonPosition,
            UnitPoint3D carbonAlphaPosition,
            UnitPoint3D nextNitrogenPosition,
            UnitValue CODistance)
        {
            // Add normalized vectors to next atoms, which will
            // point to the middle of the parallelogram spanned by these two vectors.
            // Then normalize and point in the opposite direction.
            // Then multiply by the distance and add to carbon position

            return carbonPosition + CODistance
                * -(carbonPosition.VectorTo(carbonAlphaPosition)
                        .Normalize()
                    + carbonPosition.VectorTo(nextNitrogenPosition)
                        .Normalize()).Normalize().ToVector3D();
        }

        public static UnitPoint3D CalculateAtomPosition(UnitPoint3D currentPosition,
            Vector vector1,
            Vector vector2,
            UnitValue bondLength,
            UnitValue bondAngle,
            UnitValue bondTorsion)
        {
            // Spherical coordinates
            var polarAngle = 180.To(Unit.Degree) - bondAngle;
            var azimuthAngle = bondTorsion;

            var atomCentricX = Math.Sin(polarAngle.In(Unit.Radians)) * Math.Cos(azimuthAngle.In(Unit.Radians));
            var atomCentricY = Math.Sin(polarAngle.In(Unit.Radians)) * Math.Sin(azimuthAngle.In(Unit.Radians));
            var atomCentricZ = Math.Cos(polarAngle.In(Unit.Radians));
            var atomicBondVector = new Vector3D(atomCentricX, atomCentricY, atomCentricZ);

            var zAxis = vector1.Normalize().ToVector3D();
            var xAxis = -(vector2 - vector2.ProjectOnto(zAxis)).Normalize().ToVector3D();
            var yAxis = zAxis.CrossProduct(xAxis);

            var transformMatrix = new Matrix3X3();
            transformMatrix.SetColumn(0, xAxis.Data);
            transformMatrix.SetColumn(1, yAxis.Data);
            transformMatrix.SetColumn(2, zAxis.Data);
            var bondVector = new Vector3D(transformMatrix.Data.Multiply(atomicBondVector.Data.ConvertToMatrix()).Vectorize());

            var atomPosition = currentPosition + bondLength * bondVector;
            return atomPosition;
        }
    }
}
