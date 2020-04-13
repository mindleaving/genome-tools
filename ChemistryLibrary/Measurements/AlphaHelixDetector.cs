using System;
using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Objects;
using Commons.DataProcessing;
using Commons.Extensions;
using Commons.Physics;

namespace ChemistryLibrary.Measurements
{
    public class AlphaHelixDetector
    {
        /// <summary>
        /// Detects alpha helixes in a peptide. Does NOT add these annotations to peptide.
        /// </summary>
        public List<PeptideAnnotation<AminoAcidReference>> Detect(Peptide peptide)
        {
            var slidingWindow = new SlidingWindow<AminoAcidReference>(
                peptide.AminoAcids,
                x => x.SequenceNumber, 
                4.5,
                WindowPositioningType.StartingAtPosition);
            const int MinimumHelixLength = 7;
            var helixAnnotations = new List<PeptideAnnotation<AminoAcidReference>>();
            PeptideAnnotation<AminoAcidReference> currentAnnotation = null;
            foreach (var aminoAcid in peptide.AminoAcids.OrderBy(x => x.SequenceNumber))
            {
                if (currentAnnotation != null && aminoAcid.SequenceNumber > currentAnnotation.AminoAcidReferences.Last().SequenceNumber)
                {
                    if(currentAnnotation.AminoAcidReferences.Count >= MinimumHelixLength)
                        helixAnnotations.Add(currentAnnotation);
                    currentAnnotation = null;
                }

                slidingWindow.SetPosition(aminoAcid.SequenceNumber - 0.1); // -0.1 to avoid problems with rounding
                var secondAminoAcid = slidingWindow.FirstOrDefault(x => x.SequenceNumber == aminoAcid.SequenceNumber + 1);
                if(secondAminoAcid == null)
                    continue;
                var thirdAminoAcid = slidingWindow.FirstOrDefault(x => x.SequenceNumber == aminoAcid.SequenceNumber + 2);
                if(thirdAminoAcid == null)
                    continue;
                var fourthAminoAcid = slidingWindow.FirstOrDefault(x => x.SequenceNumber == aminoAcid.SequenceNumber + 3);
                if(fourthAminoAcid == null)
                    continue;

                try
                {
                    var firstVector = FindPositionVector(aminoAcid, secondAminoAcid);
                    var secondVector = FindPositionVector(aminoAcid, thirdAminoAcid);
                    var thirdVector = FindPositionVector(aminoAcid, fourthAminoAcid);

                    var isAlphaHelix = IsAlphaHelix(firstVector, secondVector, thirdVector);
                    if (isAlphaHelix)
                    {
                        if (currentAnnotation == null)
                        {
                            currentAnnotation = new PeptideAnnotation<AminoAcidReference>(
                                PeptideSecondaryStructure.AlphaHelix,
                                new List<AminoAcidReference> { aminoAcid, secondAminoAcid, thirdAminoAcid, fourthAminoAcid });
                        }
                        else
                        {
                            currentAnnotation.AminoAcidReferences.Add(fourthAminoAcid);
                        }
                    }
                    else
                    {
                        if (currentAnnotation != null)
                        {
                            if(currentAnnotation.AminoAcidReferences.Count >= MinimumHelixLength)
                                helixAnnotations.Add(currentAnnotation);
                            currentAnnotation = null;
                        }
                    }
                }
                catch (KeyNotFoundException)
                {
                    continue;
                }
            }

            return helixAnnotations;
        }

        private bool IsAlphaHelix(
            UnitVector3D firstVector,
            UnitVector3D secondVector,
            UnitVector3D thirdVector)
        {
            var firstDistance = firstVector.Magnitude();
            var secondDistance = secondVector.Magnitude();
            var thirdDistance = thirdVector.Magnitude();

            if (secondDistance > firstDistance && secondDistance > thirdDistance)
                return true;
            return false;
        }

        private UnitVector3D FindPositionVector(
            AminoAcidReference aminoAcid,
            AminoAcidReference secondAminoAcid)
        {
            var firstCarbonAlpha = aminoAcid.GetAtomFromName("CA");
            if(firstCarbonAlpha == null)
                throw new KeyNotFoundException("Could not find carbon alpha in first amino acid");
            var secondCarbonAlpha = secondAminoAcid.GetAtomFromName("CA");
            if(secondCarbonAlpha == null)
                throw new KeyNotFoundException("Could not find carbon alpha in second amino acid");
            return firstCarbonAlpha.Position.VectorTo(secondCarbonAlpha.Position);
        }
    }
}
