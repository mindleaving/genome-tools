using System.Threading;
using ChemistryLibrary;
using Commons;
using NUnit.Framework;

namespace ChemistryLibraryTest
{
    [TestFixture]
    public class MoleculeDynamicsRunner
    {
        [Test]
        public void RunSimulation()
        {
            var molecule = AminoAcidLibrary.Proline
                .Add(AminoAcidLibrary.Tyrosine)
                .Add(AminoAcidLibrary.Alanine)
                .Add(AminoAcidLibrary.Glutamine)
                .Add(AminoAcidLibrary.Glutamine)
                .Add(AminoAcidLibrary.Lysine)
                .Molecule;
            var moleculeDynamicsSimulator = new MoleculeDynamicsSimulator();
            var settings = new MoleculeDynamicsSimulationSettings
            {
                SimulationTime = 10.To(SIPrefix.Nano, Unit.Second),
                TimeStep = 4.To(SIPrefix.Femto, Unit.Second)
            };
            var cancellationTokenSource = new CancellationTokenSource();
            moleculeDynamicsSimulator.MinimizeEnergy(molecule, settings, cancellationTokenSource.Token);
        }
    }
}
