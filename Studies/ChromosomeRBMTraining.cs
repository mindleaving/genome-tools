using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons.DataProcessing;
using Commons.Mathematics;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    /// <summary>
    /// Train Restricted Boltzmann machine from chromosome data
    /// </summary>
    public class ChromosomeRBMTraining
    {
        [Test]
        public void Run()
        {
            var sequenceLength = 250;
            var chromosomeNr = 7;
            var chromosomeDataSource = new ChromosomeDataSource($@"F:\HumanGenome\chromosomes\Homo_sapiens.GRCh38.dna.primary_assembly.chromosome_{chromosomeNr}.fa", sequenceLength);
            var rbmSettings = new RestrictedBoltzmannMachineSettings
            {
                HiddenNodes = (int)(0.2 * sequenceLength),
                InputNodes = 2 * sequenceLength,
                LearningRate = 0.1,
                TrainingIterations = 10 * 1000 * 1000
            };
            var rbm = new RestrictedBoltzmannMachine(rbmSettings);
            rbm.Train((IDataSource<bool>) chromosomeDataSource);

            //rbm.OutputModel($@"F:\HumanGenome\chromosomes\chromosome{chromosomeNr}_rbm_model.txt");
            OutputNucleotideSequences(rbm, rbmSettings, $@"F:\HumanGenome\chromosomes\chromosome{chromosomeNr}_nucelotides.txt");
        }

        private static void OutputNucleotideSequences(RestrictedBoltzmannMachine rbm, RestrictedBoltzmannMachineSettings rbmSettings, string outputFile)
        {
            var output = new List<string>();
            for (var hiddenIdx = 0; hiddenIdx < rbmSettings.HiddenNodes; hiddenIdx++)
            {
                var unitVector = Enumerable.Range(0, rbmSettings.HiddenNodes).Select(x => x == hiddenIdx ? 1.0 : 0.0).ToArray();
                var response = rbm.ToInputLayer(unitVector);
                var thresholdedResponse = response.Select(x => x > 0.5).ToArray();
                var nucleotides = NucleotidesFromBoolArray(thresholdedResponse);
                output.Add(new string(nucleotides.ToArray()));
            }
            File.WriteAllLines(outputFile, output);
        }

        private static IEnumerable<char> NucleotidesFromBoolArray(IReadOnlyList<bool> bits)
        {
            var nucleotides = new List<char>();
            for (var idx = 0; idx < bits.Count-1; idx+=2)
            {
                var firstBit = bits[idx];
                var secondBit = bits[idx + 1];
                if (firstBit)
                {
                    nucleotides.Add(secondBit ? 'C' : 'G');
                }
                else
                {
                    nucleotides.Add(secondBit ? 'T' : 'A');
                }
            }
            return nucleotides;
        }
    }
}
