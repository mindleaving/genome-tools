using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ChromosomeImageGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var dataDirectory = @"G:\Projects\HumanGenome";
            if (args.Length > 1)
                dataDirectory = args[0];

            foreach (var file in Directory.GetFiles(dataDirectory, "*chromosome_7*.fa"))
            {
                var chromosomeData = File.ReadAllText(file).Substring(117449820, 117697209 - 117449821 + 1);
                var imageWidth = 1920;
                var imageHeight = chromosomeData.Length/imageWidth;
                if (chromosomeData.Length%imageWidth != 0)
                    imageHeight++;
                var image = new Bitmap(imageWidth, imageHeight);
                var nucleotideIdx = 117449821;
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
                image.Save(Path.Combine(dataDirectory, "CFTR.png"), ImageFormat.Png);
            }
        }
    }
}
