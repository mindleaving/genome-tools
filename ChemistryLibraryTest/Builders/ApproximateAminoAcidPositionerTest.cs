using System;
using System.Linq;
using ChemistryLibrary.Builders;
using ChemistryLibrary.IO.Pdb;
using ChemistryLibrary.Measurements;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;
using NUnit.Framework;

namespace ChemistryLibraryTest.Builders
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

            Assert.That(
                aminoAcid1.CarbonAlphaPosition.DistanceTo(aminoAcid2.CarbonAlphaPosition).In(SIPrefix.Pico, Unit.Meter), 
                Is.EqualTo(380.3).Within(0.1));
        }

        [Test]
        [TestCase(@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb2da0.ent", 90, 91)]
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
            var bondAngle = 135.To(Unit.Degree);
            var bondTorsion = 30.To(Unit.Degree);
            var carbonAlphaPosition = new Point3D(-150, -140, 0);
            var carbonPosition = new Point3D(-100, 0, 0);
            var nitrogenPosition = new Point3D(0, 0, 0);

            var v1 = carbonAlphaPosition.VectorTo(carbonPosition).Normalize().ToVector3D();
            var v2 = carbonPosition.VectorTo(nitrogenPosition).Normalize().ToVector3D();
            var n1 = v2.CrossProduct(v1).Normalize().ToVector3D();

            var matrix = new Matrix(3, 4);
            matrix[0, 0] = n1.Y * v2.Z + n1.Z * v2.Y;
            matrix[0, 1] = n1.X * v2.Z - n1.Z * v2.X;
            matrix[0, 2] = -n1.X * v2.Y - n1.Y * v2.X;
            matrix[0, 3] = Math.Cos(bondTorsion.In(Unit.Radians))
                           - (n1.X * (nitrogenPosition.Y * carbonPosition.Z - nitrogenPosition.Z * carbonPosition.Y)
                              + n1.Y * (nitrogenPosition.X * carbonPosition.Z - nitrogenPosition.Z * carbonPosition.X)
                              + n1.Z * (nitrogenPosition.X * carbonPosition.Y - nitrogenPosition.Y * carbonPosition.X));
            matrix[1, 0] = v1.X;
            matrix[1, 1] = v1.Y;
            matrix[1, 2] = v1.Z;
            matrix[1, 3] = Math.Cos(Math.PI - bondAngle.In(Unit.Radians))
                           + nitrogenPosition.X * v1.X
                           + nitrogenPosition.Y * v1.Y
                           + nitrogenPosition.Z * v1.Z;

            var rref = matrix.Data.ReducedRowEchelonForm();
            var v3X = rref[0, 3];
            var v3Y = rref[1, 3];
            var v3Z = Math.Sqrt(1 - v3X.Square() - v3Y.Square());
            var v3 = new Vector3D(v3X, v3Y, v3Z);

            var actualBondAngle = Math.Acos(v1.Normalize().DotProduct(v3.Normalize())).To(Unit.Radians);
            var n2 = v3.CrossProduct(v1);
            var actualBondTorsion = Math.Acos(n1.Normalize().DotProduct(n2.Normalize())).To(Unit.Radians);

            Console.WriteLine("Finished");
        }
    }
}
