using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons;
using Commons.DataProcessing;
using Commons.Extensions;

namespace CommonsTest
{
    public class MNISTDataSource : IDataSource<double>
    {
        private readonly string imageFileName;
        private BinaryReader binaryReader;
        private Stream fileStream;
        private const int BorderSize = 4;
        public const int ImageWidth = 20;
        public const int ImageHeight = 20;
        public int ImageCount { get; private set; }
        private int imagesRead;

        public MNISTDataSource(string imageFileName)
        {
            this.imageFileName = imageFileName;
            Reset();
        }

        public void Reset()
        {
            binaryReader?.Dispose();
            fileStream?.Dispose();
            fileStream = File.OpenRead(imageFileName);
            binaryReader = new BinaryReader(fileStream);
            var checkSum = binaryReader.ReadInt32().InvertEndian();
            if (checkSum != 2051)
                throw new InvalidOperationException("Invalid MNIST image file header");
            ImageCount = binaryReader.ReadInt32().InvertEndian();
            var paddedImageHeight = binaryReader.ReadInt32().InvertEndian();
            if(paddedImageHeight != ImageHeight + 2*BorderSize)
                throw new Exception("Unexpected image height");
            var paddedImageWidth = binaryReader.ReadInt32().InvertEndian();
            if (paddedImageWidth != ImageWidth + 2 * BorderSize)
                throw new Exception("Unexpected image width");
            imagesRead = 0;
        }

        public IEnumerable<double> GetNext()
        {
            if (imagesRead >= ImageCount)
                return null;
            var paddedImage = binaryReader.ReadBytes((ImageHeight+2*BorderSize)*(ImageWidth+2*BorderSize));
            var unpaddedImage = new double[ImageHeight * ImageWidth];
            var unpaddedIndex = 0;
            for (var row = BorderSize; row < ImageHeight+BorderSize; row++)
            {
                for (var column = BorderSize; column < ImageWidth + BorderSize; column++)
                {
                    unpaddedImage[unpaddedIndex] = paddedImage[row*(ImageWidth + 2*BorderSize) + column] / 255.0;
                    unpaddedIndex++;
                }
            }
            imagesRead++;
            return unpaddedImage;
        }

        public void Dispose()
        {
            binaryReader?.Dispose();
            fileStream?.Dispose();
        }
    }
}