using System;
using System.Collections.Generic;
using System.Threading;
using ChemistryLibrary.Builders;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Objects;
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
            var molecule = PeptideBuilder.PeptideFromSequence(new[]
            {
                AminoAcidName.Proline,
                AminoAcidName.Tyrosine,
                AminoAcidName.Alanine,
                AminoAcidName.Glutamine,
                AminoAcidName.Glutamine,
                AminoAcidName.Lysine
            }).Molecule;
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
