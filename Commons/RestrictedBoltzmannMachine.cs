using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Commons
{
    public class RestrictedBoltzmannMachine
    {
        private readonly RestrictedBoltzmannMachineSettings settings;
        private readonly Random rng = new Random();

        public Vector InputOffsets { get; }
        public Vector HiddenOffsets { get; }
        public Matrix Weights { get; }

        public RestrictedBoltzmannMachine(RestrictedBoltzmannMachineSettings settings)
        {
            this.settings = settings;
            InputOffsets = new Vector(settings.InputNodes);
            HiddenOffsets = new Vector(settings.HiddenNodes);
            Weights = new Matrix(settings.HiddenNodes, settings.InputNodes);
            InitializeWeights();
        }

        private void InitializeWeights()
        {
            var hiddenNodes = Weights.Rows;
            var inputNodes = Weights.Columns;
            var highValue = 4*Math.Sqrt(6.0/(hiddenNodes + inputNodes));
            var lowValue = -highValue;
            for (var row = 0; row < Weights.Rows; row++)
            {
                for (var column = 0; column < Weights.Columns; column++)
                {
                    Weights.Data[row, column] = lowValue + (highValue - lowValue)*rng.NextDouble();
                }
            }
        }

        public void Train(IDataSource<bool> dataSource)
        {
            for (var iteration = 0; iteration < settings.TrainingIterations; iteration++)
            {
                if(iteration % (100*1000) == 0)
                    Console.WriteLine("RBM training iteration " + iteration);
                var input = dataSource.GetNext();
                if(input == null)
                {
                    dataSource.Reset();
                    input = dataSource.GetNext();
                }
                var inputValues = ToDoubleArray(input);
                var gibbsSampledValues = GibbsSample(inputValues, 1);
                UpdateWeightsAndOffsets(inputValues, gibbsSampledValues);
            }
        }

        public void Train(IDataSource<double> dataSource)
        {
            for (var iteration = 0; iteration < settings.TrainingIterations; iteration++)
            {
                if (iteration % (100 * 1000) == 0)
                    Console.WriteLine("RBM training iteration " + iteration);
                var inputValues = dataSource.GetNext()?.ToArray();
                if (inputValues == null)
                {
                    dataSource.Reset();
                    inputValues = dataSource.GetNext().ToArray();
                }
                var gibbsSampledValues = GibbsSample(inputValues, 1);
                UpdateWeightsAndOffsets(inputValues, gibbsSampledValues);
            }
        }

        private void UpdateWeightsAndOffsets(double[] inputValues, double[] gibbsSampledValues)
        {
            var hiddenValuesFromInputValues = ToHiddenLayer(inputValues);
            var hiddenValuesFromGibbsSample = ToHiddenLayer(gibbsSampledValues);
            var weightsUpdate = hiddenValuesFromInputValues.OuterProduct(inputValues)
                .Subtract(hiddenValuesFromGibbsSample.OuterProduct(gibbsSampledValues));
            Weights.Set(Weights.Data.Sum(weightsUpdate.ScalarMultiply(settings.LearningRate)));

            var hiddenOffsetUpdate = hiddenValuesFromInputValues.Subtract(hiddenValuesFromGibbsSample);
            HiddenOffsets.Set(HiddenOffsets.Data.Sum(hiddenOffsetUpdate.ScalarMultiply(settings.LearningRate)));

            var inputOffsetUpdate = inputValues.Subtract(gibbsSampledValues);
            InputOffsets.Set(InputOffsets.Data.Sum(inputOffsetUpdate.ScalarMultiply(settings.LearningRate)));
        }

        private double[] GibbsSample(double[] inputValues, int cycles)
        {
            var gibbsSampledValues = inputValues;
            for (var cycleIdx = 0; cycleIdx < cycles; cycleIdx++)
            {
                gibbsSampledValues = ToInputLayer(ToHiddenLayer(gibbsSampledValues));
            }
            return gibbsSampledValues;
        }

        public double[] ToInputLayer(double[] hiddenData)
        {
            var inputValues = hiddenData
                .ConvertToMatrix().Transpose().Multiply(Weights.Data).Vectorize()
                .Sum(InputOffsets.Data);
            return inputValues.Select(MathFunctions.Sigmoid).ToArray();
        }

        public double[] ToHiddenLayer(double[] inputValues)
        {
            var hiddenValues = Weights.Data.Multiply(inputValues.ConvertToMatrix()).Vectorize()
                .Sum(HiddenOffsets.Data);
            return hiddenValues.Select(MathFunctions.Sigmoid).ToArray();
        }

        private static double[] ToDoubleArray(IEnumerable<bool> inputData)
        {
            return inputData.Select(x => x ? 1.0 : 0.0).ToArray();
        }

        private static bool[] Threshold(IEnumerable<double> values)
        {
            return values.Select(x => x > 0).ToArray();
        }

        private bool[] Sample(double[] probabilities)
        {
            return probabilities.Select(p => rng.NextDouble() <= p).ToArray();
        }

        private double CalcualteFreeEnergy(double[] inputValues)
        {
            var preSigmoidHiddenValues = Weights.Data.Multiply(inputValues.ConvertToMatrix()).Vectorize()
                .Sum(HiddenOffsets.Data);
            var valueOffsetProduct = inputValues.InnerProduct(InputOffsets.Data);
            var hiddenLog = preSigmoidHiddenValues.Sum(x => Math.Log(1 + Math.Exp(x)));
            return -hiddenLog - valueOffsetProduct;
        }

        public void OutputModel(string modelFile)
        {
            var output = "";
            var transposedWeights = Weights.Data.Transpose();
            for (int rowIdx = 0; rowIdx < transposedWeights.GetLength(0); rowIdx++)
            {
                for (int columnIdx = 0; columnIdx < transposedWeights.GetLength(1); columnIdx++)
                {
                    if (columnIdx > 0)
                        output += ";";
                    output += transposedWeights[rowIdx, columnIdx].ToString(CultureInfo.InvariantCulture);
                }
                output += Environment.NewLine;
            }
            output += Environment.NewLine;
            output += InputOffsets.Data
                          .Select(x => x.ToString(CultureInfo.InvariantCulture))
                          .Aggregate((a, b) => a + ";" + b)
                      + Environment.NewLine;
            output += HiddenOffsets.Data
                          .Select(x => x.ToString(CultureInfo.InvariantCulture))
                          .Aggregate((a, b) => a + ";" + b)
                      + Environment.NewLine;
            File.WriteAllText(modelFile, output);
        }
    }
}
