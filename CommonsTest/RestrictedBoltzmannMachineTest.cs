using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Commons;
using NUnit.Framework;

namespace CommonsTest
{
    [TestFixture]
    public class RestrictedBoltzmannMachineTest
    {
        [Test]
        public void MNISTTraining()
        {
            var mnistFileName = @"G:\Projects\BinaryRBM\MNIST\train-images.idx3-ubyte";
            var mnistDataSource = new MNISTDataSource(mnistFileName);

            var rbmSettings = new RestrictedBoltzmannMachineSettings
            {
                InputNodes = MNISTDataSource.ImageHeight* MNISTDataSource.ImageWidth,
                HiddenNodes = 64,
                LearningRate = 0.1,
                TrainingIterations = 20*mnistDataSource.ImageCount
            };
            var rbm = new RestrictedBoltzmannMachine(rbmSettings);
            rbm.Train(mnistDataSource);

            OutputWeightImages(rbm.Weights, MNISTDataSource.ImageWidth, MNISTDataSource.ImageHeight, @"G:\Projects\BinaryRBM\weightImages");
        }

        private void OutputWeightImages(Matrix weights, int imageWidth, int imageHeight, string outputDirectory)
        {
            for (int imageIdx = 0; imageIdx < weights.Rows; imageIdx++)
            {
                var bitmap = new Bitmap(imageWidth, imageHeight);

                var minimumWeight = double.PositiveInfinity;
                var maximumWeight = double.NegativeInfinity;
                for (int i = 0; i < weights.Columns; i++)
                {
                    var weight = weights[imageIdx, i];
                    if (weight < minimumWeight)
                        minimumWeight = weight;
                    if (weight > maximumWeight)
                        maximumWeight = weight;
                }
                for (int row = 0; row < imageHeight; row++)
                {
                    for (int column = 0; column < imageWidth; column++)
                    {
                        var weight = weights.Data[imageIdx, row*imageWidth + column];
                        var normalizedWeight = (weight - minimumWeight)/(maximumWeight - minimumWeight);
                        var color = Color.FromArgb((int) (255*normalizedWeight), 
                            (int) (255*normalizedWeight),
                            (int) (255*normalizedWeight));
                        bitmap.SetPixel(column, row, color);
                    }
                }
                bitmap.Save(Path.Combine(outputDirectory, "image" + imageIdx.ToString("D2") + ".png"), ImageFormat.Png);
            }
        }
    }
}
