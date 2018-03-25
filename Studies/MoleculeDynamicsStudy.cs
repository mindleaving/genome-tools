using System;
using System.Collections.Generic;
using System.Threading;
using ChemistryLibrary.Builders;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Simulation;
using Commons;
using Commons.Extensions;
using Commons.Physics;
using NUnit.Framework;

namespace Studies
{
    [TestFixture]
    public class MoleculeDynamicsStudy
    {
        private readonly ManualResetEvent simulationCompletedEvent = new ManualResetEvent(true);

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
            var customForces = new List<CustomAtomForce>();
            var settings = new MoleculeDynamicsSimulationSettings
            {
                SimulationTime = 10.To(SIPrefix.Nano, Unit.Second),
                TimeStep = 4.To(SIPrefix.Femto, Unit.Second)
            };
            var simulator = new MoleculeDynamicsSimulator(molecule, customForces, settings);
            simulator.SimulationCompleted += Simulator_SimulationCompleted;
            simulationCompletedEvent.Reset();
            simulator.StartSimulation();
            simulationCompletedEvent.WaitOne();
        }

        private void Simulator_SimulationCompleted(object sender, EventArgs e)
        {
            simulationCompletedEvent.Set();
        }
    }
}
