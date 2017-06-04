using System.Linq;
using ChemistryLibrary.Builders;
using ChemistryLibrary.Pdb;
using ChemistryLibrary.Simulation;
using Commons;
using NUnit.Framework;

namespace Studies
{
    public class ApproximatePeptideFoldingValidationStudy
    {
        [Test]
        [TestCase(@"G:\Projects\HumanGenome\Protein-PDBs\5m9j.pdb")]
        public void ApproximatePeptideIsFoldedToKnownStableState(string pdbFilePath)
        {
            var pdbReadResult = PdbReader.ReadFile(pdbFilePath);
            var peptide = pdbReadResult.Chains.First();
            var approximatePeptide = ApproximatePeptideBuilder.FromPeptide(peptide);

            var simulationSettings = new ApproximatePeptideSimulationSettings
            {
                SimulationTime = 10.To(SIPrefix.Pico, Unit.Second),
                TimeStep = 2.To(SIPrefix.Femto, Unit.Second)
            };
            var simulator = new ApproximatePeptideFoldingSimulator(approximatePeptide);
            simulator.StartSimulation(simulationSettings);

        }
    }
}
