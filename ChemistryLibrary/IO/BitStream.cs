using System;
using System.Collections;
using System.IO;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class BitStream : IDisposable
    {
        private readonly Stream stream;
        private BitArray buffer;
        private int bufferIndex = -1;
        private bool hasReachedEnd;
        private bool hasFlushed;
        private bool hasBeenWrittenOnce;


        public BitStream(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            FillBuffer();
        }
        public BitStream(byte[] bytes) : this(new MemoryStream(bytes)) {}

        /// <summary>
        /// Returns next bit
        /// </summary>
        public bool ReadBit()
        {
            if (hasReachedEnd)
                throw new IOException("No more data");
            if (bufferIndex < 0)
            {
                FillBuffer();
                if (hasReachedEnd)
                    throw new IOException("No more data");
            }
            var bit = buffer[bufferIndex];
            bufferIndex--;
            return bit;
        }

        public bool[] ReadBits(int count)
        {
            var bits = new bool[count];
            for (int i = 0; i < count; i++)
            {
                var bit = ReadBit();
                bits[i] = bit;
            }
            return bits;
        }

        private void FillBuffer()
        {
            var b = stream.ReadByte();
            if (b < 0)
            {
                hasReachedEnd = true;
                return;
            }
            buffer = new BitArray(new []{ (byte)b });
            bufferIndex = 7;
            hasBeenWrittenOnce = false;
        }

        public void WriteBit(bool bit)
        {
            if(bufferIndex < 0)
            {
                FillBuffer();
                if (hasReachedEnd)
                {
                    buffer = new BitArray(8);
                    bufferIndex = 7;
                    hasBeenWrittenOnce = false;
                }
            }
            buffer[bufferIndex] = bit;
            bufferIndex--;
            hasFlushed = false;
            if(bufferIndex < 0)
                Flush();
        }

        public void Seek(long position, SeekOrigin origin)
        {
            Flush();
            stream.Seek(position, origin);
            hasReachedEnd = false;
            FillBuffer();
        }

        public void Dispose()
        {
            Flush();
            stream.Dispose();
        }

        public void Flush()
        {
            if(hasFlushed)
                return;
            var b = new byte[1];
            buffer.CopyTo(b, 0);
            if (!hasReachedEnd || hasBeenWrittenOnce)
                stream.Seek(-1, SeekOrigin.Current); // Move back one byte to overwrite the byte we have read with FillBuffer
            stream.WriteByte(b[0]);
            hasFlushed = true;
            hasBeenWrittenOnce = true;
        }
    }
}
