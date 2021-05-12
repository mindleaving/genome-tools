using System;
using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Builders;
using GenomeTools.ChemistryLibrary.IO.Pdb;
using GenomeTools.ChemistryLibrary.Measurements;
using GenomeTools.ChemistryLibrary.Objects;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.Builders
{
    [TestFixture]
    public class ApproximateAminoAcidPositionerTest
    {
        [Test]
        public void AtomPositionsAsExpected()
        {
            var aminoAcid1 = new ApproximatedAminoAcid(AminoAcidName.Valine, 90)
            {
                OmegaAngle = 45.To(Unit.Degree),
                PhiAngle = -60.To(Unit.Degree),
                PsiAngle = 90.To(Unit.Degree)
            };
            var aminoAcid2 = new ApproximatedAminoAcid(AminoAcidName.Alanine, 91)
            {
                OmegaAngle = 0.To(Unit.Degree),
                PhiAngle = -90.To(Unit.Degree),
                PsiAngle = 90.To(Unit.Degree)
            };
            var peptide = new ApproximatePeptide(new[] { aminoAcid1, aminoAcid2 });
            ApproximateAminoAcidPositioner.Position(peptide.AminoAcids, new UnitPoint3D(Unit.Meter, 0, 0, 0));
            var angles = AminoAcidAngleMeasurer.MeasureAngles(peptide);

            Assert.That(angles[aminoAcid1].Psi.In(Unit.Degree), Is.EqualTo(aminoAcid1.PsiAngle.In(Unit.Degree)).Within(1));
            Assert.That(angles[aminoAcid2].Omega.In(Unit.Degree), Is.EqualTo(aminoAcid2.OmegaAngle.In(Unit.Degree)).Within(1));
            Assert.That(angles[aminoAcid2].Phi.In(Unit.Degree), Is.EqualTo(aminoAcid2.PhiAngle.In(Unit.Degree)).Within(1));

            //Assert.That(
            //    aminoAcid1.NitrogenPosition.DistanceTo(new UnitPoint3D(Unit.Meter, 0, 0, 0)).In(SIPrefix.Pico, Unit.Meter),
            //    Is.EqualTo(0).Within(1e-3));
            //Assert.That(
            //    aminoAcid1.CarbonAlphaPosition.DistanceTo(new UnitPoint3D(SIPrefix.Pico, Unit.Meter, 152, 0, 0)).In(SIPrefix.Pico, Unit.Meter),
            //    Is.EqualTo(0).Within(1e-3));
            //Assert.That(
            //    aminoAcid1.CarbonPosition.DistanceTo(new UnitPoint3D(SIPrefix.Pico, Unit.Meter, 152, 154, 0)).In(SIPrefix.Pico, Unit.Meter),
            //    Is.EqualTo(0).Within(1e-3));
            //Assert.That(
            //    aminoAcid2.NitrogenPosition.DistanceTo(new UnitPoint3D(SIPrefix.Pico, Unit.Meter, 152, 154, 152)).In(SIPrefix.Pico, Unit.Meter),
            //    Is.EqualTo(0).Within(1e-3));
        }

        [Test]
        public void CarbonAlphaDistancesMatchRealMeasurements()
        {
            var aminoAcid1 = new ApproximatedAminoAcid(AminoAcidName.Valine, 90)
            {
                OmegaAngle = 179.9.To(Unit.Degree),
                PhiAngle = -35.9.To(Unit.Degree),
                PsiAngle = -52.0.To(Unit.Degree)
            };
            var aminoAcid2 = new ApproximatedAminoAcid(AminoAcidName.Alanine, 91)
            {
                OmegaAngle = 179.9.To(Unit.Degree),
                PhiAngle = -63.5.To(Unit.Degree),
                PsiAngle = -59.7.To(Unit.Degree)
            };
            var peptide = new[] { aminoAcid1, aminoAcid2 };
            ApproximateAminoAcidPositioner.Position(peptide, new UnitPoint3D(Unit.Meter, 0, 0, 0));

            var carbonAlphaCarbonDistance = aminoAcid1.CarbonAlphaPosition
                .DistanceTo(aminoAcid1.CarbonPosition);
            Assert.That(carbonAlphaCarbonDistance.In(SIPrefix.Pico, Unit.Meter), Is.EqualTo(154).Within(0.1));
            var carbonNitrogenDistance = aminoAcid1.CarbonPosition
                .DistanceTo(aminoAcid2.NitrogenPosition);
            Assert.That(carbonNitrogenDistance.In(SIPrefix.Pico, Unit.Meter), Is.EqualTo(152).Within(0.1));

            var aminoAcidAngles = AminoAcidAngleMeasurer.MeasureAngles(new ApproximatePeptide(peptide));
            Assert.That(aminoAcidAngles[aminoAcid1].Psi.Value - aminoAcid1.PsiAngle.Value, Is.EqualTo(0).Within(0.1));
            Assert.That(aminoAcidAngles[aminoAcid2].Phi.Value - aminoAcid2.PhiAngle.Value, Is.EqualTo(0).Within(0.1));

            // The bond length vary because we use theoretical bond length, which do not exactly match real bond lengths
            //Assert.That(
            //    aminoAcid1.CarbonAlphaPosition.DistanceTo(aminoAcid2.CarbonAlphaPosition).In(SIPrefix.Pico, Unit.Meter), 
            //    Is.EqualTo(380.3).Within(0.1));
        }

        [Test]
        [TestCase(@"F:\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb2da0.ent", 90, 91)]
        public void MeasureAminoAcidDistances(string pdbFilePath, int sequenceNumber1, int sequenceNumber2)
        {
            var pdb = PdbReader.ReadFile(pdbFilePath);
            var firstChain = pdb.Models.First().Chains.First();
            var aminoAcidAngles = AminoAcidAngleMeasurer.MeasureAngles(firstChain);
            var aminoAcid1 = firstChain.AminoAcids.Find(x => x.SequenceNumber == sequenceNumber1);
            var aminoAcid2 = firstChain.AminoAcids.Find(x => x.SequenceNumber == sequenceNumber2);
            var aminoAcid1Angles = aminoAcidAngles[aminoAcid1];
            var aminoAcid2Angles = aminoAcidAngles[aminoAcid2];
            var carbonAlphaDistance = aminoAcid1.GetAtomFromName("CA").Position
                .DistanceTo(aminoAcid2.GetAtomFromName("CA").Position);
            Console.WriteLine("Carbon alpha distance: " + carbonAlphaDistance);
            Console.WriteLine($"Amino acid {sequenceNumber1} ({aminoAcid1.Name}) angles: {aminoAcid1Angles}");
            Console.WriteLine($"Amino acid {sequenceNumber2} ({aminoAcid2.Name}) angles: {aminoAcid2Angles}");

            var carbonAlphaCarbonDistance = aminoAcid1.GetAtomFromName("CA").Position
                .DistanceTo(aminoAcid1.GetAtomFromName("C").Position);
            Console.WriteLine($"Carbon alpha-Carbon distance: {carbonAlphaCarbonDistance}");

            var carbonNitrogenDistance = aminoAcid1.GetAtomFromName("C").Position
                .DistanceTo(aminoAcid2.GetAtomFromName("N").Position);
            Console.WriteLine($"Carbon-Nitrogen distance: {carbonNitrogenDistance}");
        }

        [Test]
        public void DerivePositionEquationSystem()
        {
            var bondAngle = 160.To(Unit.Degree);
            var bondTorsion = 30.To(Unit.Degree);
            var bondLength = 154.To(SIPrefix.Pico, Unit.Meter);
            var carbonAlphaPosition = new Point3D(-150, -140, 0);
            var carbonPosition = new Point3D(-100, 0, 0);
            var nitrogenPosition = new Point3D(0, 0, 0);

            var v1 = carbonAlphaPosition.VectorTo(carbonPosition).Normalize().ToVector3D();
            var v2 = carbonPosition.VectorTo(nitrogenPosition).Normalize().ToVector3D();
            var n1 = v2.CrossProduct(v1).Normalize().ToVector3D();

            // Spherical coordinates
            var polarAngle = 180.To(Unit.Degree) - bondAngle;
            var azimuthAngle = bondTorsion;
            var radius = bondLength;
            var radiusInPicoMeter = radius.In(SIPrefix.Pico, Unit.Meter);

            var atomCentricX = radiusInPicoMeter * Math.Sin(polarAngle.In(Unit.Radians)) * Math.Cos(azimuthAngle.In(Unit.Radians));
            var atomCentricY = radiusInPicoMeter * Math.Sin(polarAngle.In(Unit.Radians)) * Math.Sin(azimuthAngle.In(Unit.Radians));
            var atomCentricZ = radiusInPicoMeter * Math.Cos(polarAngle.In(Unit.Radians));
            var bondVector = new Vector3D(atomCentricX, atomCentricY, atomCentricZ);

            var zAxis = v2;
            var xAxis = -(v1 - v1.ProjectOnto(zAxis)).Normalize().ToVector3D();
            var yAxis = zAxis.CrossProduct(xAxis);

            var transformMatrix = new Matrix3X3();
            transformMatrix.SetColumn(0, xAxis.Data);
            transformMatrix.SetColumn(1, yAxis.Data);
            transformMatrix.SetColumn(2, zAxis.Data);
            var bondDirection = transformMatrix.Data.Multiply(bondVector.Data.ConvertToMatrix()).Vectorize();
            var v3 = new Vector3D(bondDirection);

            var actualBondAngle = (Math.PI - Math.Acos(v2.Normalize().DotProduct(v3.Normalize()))).To(Unit.Radians);
            var n2 = v3.CrossProduct(v2);
            var actualBondTorsion = Math.Acos(n1.Normalize().DotProduct(n2.Normalize())).To(Unit.Radians);

            Assert.That(actualBondAngle.Value, Is.EqualTo(bondAngle.Value).Within(1e-6));
            Assert.That(actualBondTorsion.Value, Is.EqualTo(bondTorsion.Value).Within(1e-6));
        }
    }
}
