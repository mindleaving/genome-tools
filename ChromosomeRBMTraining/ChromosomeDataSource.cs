using System;
using System.Collections.Generic;
using System.IO;
using Commons;
using Commons.Collections;
using Commons.DataProcessing;

namespace ChromosomeRBMTraining
{
    public class ChromosomeDataSource : IDataSource<bool>
    {
        private readonly string path;
        private StreamReader streamReader;
        private readonly CircularBuffer<bool> dataBuffer;

        public ChromosomeDataSource(string path, int dataLength)
        {
            this.path = path;
            dataBuffer = new CircularBuffer<bool>(2*dataLength);
            Reset();
        }

        public void Reset()
        {
            streamReader?.Dispose();
            streamReader = new StreamReader(path);
            dataBuffer.Clear();
        }

        public IEnumerable<bool> GetNext()
        {
            try
            {
                do
                {
                    var singleLetter = new char[1];
                    streamReader.Read(singleLetter, 0, 1);
                    UpdateDataBuffer(singleLetter[0]);
                } while (!dataBuffer.IsFilled);
            }
            catch (Exception)
            {
                return null;
            }
            return dataBuffer;
        }

        private void UpdateDataBuffer(char nucleotide)
        {
            switch (char.ToUpperInvariant(nucleotide))
            {
                case 'A':
                    dataBuffer.Put(false);
                    dataBuffer.Put(false);
                    break;
                case 'T':
                    dataBuffer.Put(false);
                    dataBuffer.Put(true);
                    break;
                case 'G':
                    dataBuffer.Put(true);
                    dataBuffer.Put(false);
                    break;
                case 'C':
                    dataBuffer.Put(true);
                    dataBuffer.Put(true);
                    break;
                default:
                    dataBuffer.Clear();
                    break;
            }
        }

        public void Dispose()
        {
            streamReader?.Dispose();
        }
    }
}