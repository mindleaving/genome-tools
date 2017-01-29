using Commons;
using NUnit.Framework;

namespace CommonsTest
{
    [TestFixture]
    public class UnitValueTest
    {
        [Test]
        [TestCase(Unit.Meter)]
        [TestCase(Unit.Feet)]
        [TestCase(Unit.FeetPerMinute)]
        [TestCase(Unit.Kilogram)]
        [TestCase(Unit.InchesOfMercury)]
        [TestCase(Unit.Fahrenheit)]
        [TestCase(Unit.Mach)]
        public void UnitConversionRoundtripReturnsInput(Unit unit)
        {
            var number = StaticRandom.Rng.NextDouble();

            var unitValue = number.To(unit);
            var roundtripNumber = unitValue.In(unit);

            Assert.That(roundtripNumber, Is.EqualTo(number).Within(1e-5));
        }

        [Test]
        public void CompoundUnitConversionRoundtripReturnsInput()
        {
            var unit = new CompoundUnit(
                new []{SIBaseUnit.Kilogram, SIBaseUnit.Second, SIBaseUnit.Meter },
                new []{SIBaseUnit.Ampere, SIBaseUnit.Ampere });
            var number = StaticRandom.Rng.NextDouble();

            var unitValue = number.To(unit);
            var roundtripNumber = unitValue.In(unit);

            Assert.That(roundtripNumber, Is.EqualTo(number).Within(1e-5));
        }

        [Test]
        public void PrefixedUnitConversionRoundtripReturnsInput()
        {
            var prefix = SIPrefix.Micro;
            var unit = Unit.Meter;
            var number = StaticRandom.Rng.NextDouble();

            var unitValue = number.To(prefix, unit);
            var roundtripNumber = unitValue.In(prefix, unit);

            Assert.That(roundtripNumber, Is.EqualTo(number).Within(1e-5));
        }

        [Test]
        public void ProductReturnsCorrectValueAndUnit()
        {
            var mass = 1.5.To(Unit.Kilogram);
            var acceleration = 3.To(Unit.MetersPerSecondSquared);

            var product = mass*acceleration;

            Assert.That(product.Unit == Unit.Newton);
            var expectedValue = mass.In(Unit.Kilogram)*acceleration.In(Unit.MetersPerSecondSquared);
            Assert.That(product.Value, Is.EqualTo(expectedValue).Within(1e-5));
        }
    }
}
