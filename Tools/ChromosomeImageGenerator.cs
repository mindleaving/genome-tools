using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;

namespace Tools
{
    public class ChromosomeImageGenerator
    {
        [Test]
        [TestCase(@"F:\HumanGenome\xxxxxx.fa", 117449820, 117697209, @"C:\Temp\CFTR.png")]
        public void Generate(string file, int startBase, int endBase, string outputFilePath)
        {
            var chromosomeData = File.ReadAllText(file).Substring(startBase, endBase - startBase);
            var imageWidth = 1920;
            var imageHeight = chromosomeData.Length/imageWidth;
            if (chromosomeData.Length%imageWidth != 0)
                imageHeight++;
            var image = new Bitmap(imageWidth, imageHeight);
            var nucleotideIdx = startBase;
            var pixelIdx = 0;
            foreach (var nucleotide in chromosomeData)
            {
                Color color;
                switch (nucleotide)
                {
                    case 'A':
                        color = Color.Red;
                        break;
                    case 'T':
                        color = Color.Yellow;
                        break;
                    case 'G':
                        color = Color.Blue;
                        break;
                    case 'C':
                        color = Color.ForestGreen;
                        break;
                    case 'N':
                        color = Color.Gray;
                        break;
                    default:
                        throw new Exception();
                }
                //if(nucleotideIdx >= 117559592 && nucleotideIdx <= 117559594)
                //    color = Color.Black;
                var row = pixelIdx / imageWidth;
                var column = pixelIdx - row*imageWidth;
                image.SetPixel(column, row, color);
                pixelIdx++;
                nucleotideIdx++;
            }
            image.Save(outputFilePath, ImageFormat.Png);
        }
    }
}
