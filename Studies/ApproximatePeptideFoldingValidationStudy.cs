using System.Linq;
using System.Threading;
using ChemistryLibrary.Builders;
using ChemistryLibrary.IO.Pdb;
using ChemistryLibrary.Simulation;
using Commons;
using Commons.Extensions;
using Commons.Physics;
using NUnit.Framework;

namespace Studies
{
    public class ApproximatePeptideFoldingValidationStudy
    {
        private readonly ManualResetEvent simulationWaitHandle = new ManualResetEvent(true);

        [Test]
        [TestCase(@"F:\HumanGenome\Protein-PDBs\5m9j.pdb")]
        public void ApproximatePeptideIsFoldedToKnownStableState(string pdbFilePath)
        {
            var pdbReadResult = PdbReader.ReadFile(pdbFilePath);
            var peptide = pdbReadResult.Models.First().Chains.First();
            var approximatePeptide = ApproximatePeptideBuilder.FromPeptide(peptide);

            var simulationSettings = new ApproximatePeptideSimulationSettings
            {
                SimulationTime = 10.To(SIPrefix.Pico, Unit.Second),
                TimeStep = 2.To(SIPrefix.Femto, Unit.Second)
            };
            var ramachadranDataDirectory = @"F:\HumanGenome\ramachadranDistributions";
            var simulator = ApproximatePeptideFoldingSimulatorFactory.Create(
                approximatePeptide, simulationSettings, ramachadranDataDirectory);
            simulator.TimestepCompleted += Simulator_TimestepCompleted;
            simulator.SimulationCompleted += Simulator_SimulationCompleted;
            simulationWaitHandle.Reset();
            simulator.StartSimulation();

            simulationWaitHandle.WaitOne();
            Assert.Pass();
        }

        private void Simulator_SimulationCompleted(object sender, System.EventArgs e)
        {
            simulationWaitHandle.Set();
        }

        private void Simulator_TimestepCompleted(object sender, SimulationTimestepCompleteEventArgs e)
        {
        }
    }
}
