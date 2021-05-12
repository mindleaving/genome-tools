using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.Studies
{
    public class ProteinAlignerResult
    {
        public ProteinAlignerResult(
            LinearTransformation3D transformation, 
            UnitValue averagePositionError,
            bool isTransformationValid)
        {
            Transformation = transformation;
            AveragePositionError = averagePositionError;
            IsTransformationValid = isTransformationValid;
        }

        public LinearTransformation3D Transformation { get; }
        public UnitValue AveragePositionError { get; }
        public bool IsTransformationValid { get; }
    }
    public class ProteinAligner
    {
        /// <summary>
        /// Aligns <paramref name="peptide2"/> with <paramref name="peptide1"/>
        /// </summary>
        /// <returns>
        /// Linear transformation that maps <paramref name="peptide2"/> positions
        /// to best alignment with <paramref name="peptide1"/>
        /// </returns>
        public ProteinAlignerResult Align(Peptide peptide1, Peptide peptide2)
        {
            // TODO: Select sequences where missing amino acids are filled in (using sequence numbers)
            var sequence1AminoAcids = peptide1.AminoAcids.Select(aa => aa.Name).ToList();
            var sequence2AminoAcids = peptide2.AminoAcids.Select(aa => aa.Name).ToList();
            var logicalAlignment = SequenceAligner.Align(sequence1AminoAcids, sequence2AminoAcids);
            var sequence1CarbonAlphaPositions = logicalAlignment.AlignedPairs
                .Select(alignedPair => alignedPair.Item1Index)
                .Select(aminoAcidIdx => peptide1.AminoAcids[aminoAcidIdx].GetAtomFromName("CA"))
                .Select(carbonAlpha => carbonAlpha?.Position?.In(SIPrefix.Pico, Unit.Meter))
                .ToList();
            var sequence2CarbonAlphaPositions = logicalAlignment.AlignedPairs
                .Select(alignedPair => alignedPair.Item2Index)
                .Select(aminoAcidIdx => peptide2.AminoAcids[aminoAcidIdx].GetAtomFromName("CA"))
                .Select(carbonAlpha => carbonAlpha?.Position?.In(SIPrefix.Pico, Unit.Meter))
                .ToList();

            return AlignUsingPositionPairs(sequence1CarbonAlphaPositions, sequence2CarbonAlphaPositions);
        }

        public ProteinAlignerResult AlignSubsequence(
            Peptide peptide1,
            int startIndex1,
            Peptide peptide2,
            int startIndex2,
            int length)
        {
            var subsequence1 = peptide1.AminoAcids
                .Skip(startIndex1)
                .Take(length)
                .Select(aa => aa.GetAtomFromName("CA"))
                .Select(carbonAlpha => carbonAlpha?.Position?.In(SIPrefix.Pico, Unit.Meter))
                .ToList();
            var subsequence2 = peptide2.AminoAcids
                .Skip(startIndex2)
                .Take(length)
                .Select(aa => aa.GetAtomFromName("CA"))
                .Select(carbonAlpha => carbonAlpha?.Position?.In(SIPrefix.Pico, Unit.Meter))
                .ToList();

            return AlignUsingPositionPairs(subsequence1, subsequence2);
        }

        private ProteinAlignerResult AlignUsingPositionPairs(List<Point3D> sequence1, List<Point3D> sequence2)
        {
            var validIndices = sequence1
                .PairwiseOperation(sequence2, (p1, p2) => p1 != null && p2 != null)
                .Select((isValid, idx) => new {IsValid = isValid, Index = idx})
                .Where(x => x.IsValid)
                .Select(x => x.Index)
                .ToList();
            if(validIndices.Count < 2)
                throw new ArgumentException("Too little position information (too many null-points) for alignment of sequences");
            var sequence2PositionArray = new double[validIndices.Count, 4];
            for (int validIdx = 0; validIdx < validIndices.Count; validIdx++)
            {
                var idx = validIndices[validIdx];
                sequence2PositionArray[validIdx, 0] = 1;
                sequence2PositionArray[validIdx, 1] = sequence2[idx].X;
                sequence2PositionArray[validIdx, 2] = sequence2[idx].Y;
                sequence2PositionArray[validIdx, 3] = sequence2[idx].Z;
            }

            var sequence1X = validIndices.Select(idx => sequence1[idx].X).ToArray();
            var sequence1Y = validIndices.Select(idx => sequence1[idx].Y).ToArray();
            var sequence1Z = validIndices.Select(idx => sequence1[idx].Z).ToArray();

            var betaX = LinearRegression(sequence2PositionArray, sequence1X);
            var betaY = LinearRegression(sequence2PositionArray, sequence1Y);
            var betaZ = LinearRegression(sequence2PositionArray, sequence1Z);

            var rotationMatrix = new Matrix3X3();
            rotationMatrix.Set(new[,]
            {
                {betaX[1], betaX[2], betaX[3]}, 
                {betaY[1], betaY[2], betaY[3]}, 
                {betaZ[1], betaZ[2], betaZ[3]}
            });
            var transformation = new LinearTransformation3D(
                new Vector3D(betaX[0], betaY[0], betaZ[0]),
                rotationMatrix);
            var averagePositionError = validIndices
                .Select(idx => sequence1[idx].DistanceTo(transformation.Apply(sequence2[idx])))
                .Average()
                .To(SIPrefix.Pico, Unit.Meter);
            var isRotationMatrixValid = (rotationMatrix[0, 0].Square() + rotationMatrix[1, 0].Square() + rotationMatrix[2, 0].Square()).IsBetween(0.9,1.1)
                && (rotationMatrix[0, 1].Square() + rotationMatrix[1, 1].Square() + rotationMatrix[2, 1].Square()).IsBetween(0.9,1.1)
                && (rotationMatrix[0, 2].Square() + rotationMatrix[1, 2].Square() + rotationMatrix[2, 2].Square()).IsBetween(0.9,1.1);
            return new ProteinAlignerResult(transformation, averagePositionError, isRotationMatrixValid);
        }

        private double[] LinearRegression(double[,] X, double[] b)
        {
            return X.Transpose().Multiply(X).Inverse().Multiply(X.Transpose().Multiply(b.ConvertToMatrix())).Vectorize();
        }

        //public LinearTransformation3D AlignAtSequence(Peptide peptide1, Range<int> range1, Peptide peptide2, Range<int> range2)
        //{
        //    if(range1.From-range1.To != range2.From-range2.To)
        //        throw new ArgumentException("Ranges do not have the same length");
        //    var aminoAcidSequence1 = peptide1.AminoAcids.SubArray(range1);
        //    var aminoAcidSequence2 = peptide2.AminoAcids.SubArray(range2);
        //}
    }
}
