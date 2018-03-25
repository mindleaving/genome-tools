using System.Linq;
using ChemistryLibrary.Builders;
using ChemistryLibrary.IO.Pdb;
using ChemistryLibrary.Measurements;
using Commons.Extensions;
using Commons.Physics;
using NUnit.Framework;

namespace ChemistryLibraryTest.Measurements
{
    [TestFixture]
    public class AminoAcidAngleMeasurerTest
    {
        [Test]
        public void Psi90PlusMeasuredCorrectly()
        {
            var peptide = PeptideBuilder.PeptideFromString("GG");
            var firstAminoAcid = peptide.AminoAcids.First();
            var lastAminoAcid = peptide.AminoAcids.Last();
            PdbAminoAcidAtomNamer.AssignNames(firstAminoAcid);
            PdbAminoAcidAtomNamer.AssignNames(lastAminoAcid);
            var N1 = firstAminoAcid.GetAtomFromName("N");
            var Ca1 = firstAminoAcid.GetAtomFromName("CA");
            var C1 = firstAminoAcid.GetAtomFromName("C");
            var N2 = lastAminoAcid.GetAtomFromName("N");
            var Ca2 = lastAminoAcid.GetAtomFromName("CA");
            var C2 = lastAminoAcid.GetAtomFromName("C");

            N1.Position = new UnitPoint3D(SIPrefix.Pico, Unit.Meter, 0, 0, 0);
            N1.IsPositionFixed = true;

            Ca1.Position = new UnitPoint3D(SIPrefix.Pico, Unit.Meter, 100, 0, 0);
            Ca1.IsPositionFixed = true;

            C1.Position = new UnitPoint3D(SIPrefix.Pico, Unit.Meter, 100, 100, 0);
            C1.IsPositionFixed = true;

            N2.Position = new UnitPoint3D(SIPrefix.Pico, Unit.Meter, 100, 100, 100);
            N2.IsPositionFixed = true;

            Ca2.Position = new UnitPoint3D(SIPrefix.Pico, Unit.Meter, 200, 100, 100);
            Ca2.IsPositionFixed = true;

            C2.Position = new UnitPoint3D(SIPrefix.Pico, Unit.Meter, 200, 200, 100);
            C2.IsPositionFixed = true;

            var measurements = AminoAcidAngleMeasurer.MeasureAngles(peptide);

            Assert.That(measurements.ContainsKey(firstAminoAcid));
            Assert.That(measurements.ContainsKey(lastAminoAcid));

            var firstAngles = measurements[firstAminoAcid];
            var lastAngles = measurements[lastAminoAcid];

            Assert.That(firstAngles.Omega, Is.Null);
            Assert.That(firstAngles.Phi, Is.Null);
            Assert.That(firstAngles.Psi, Is.Not.Null);
            Assert.That(firstAngles.Psi.In(Unit.Degree), Is.EqualTo(90));

            Assert.That(lastAngles.Omega, Is.Not.Null);
            Assert.That(lastAngles.Omega.In(Unit.Degree), Is.EqualTo(90));
        }
    }
}
