using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Commons.Extensions;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class UnbufferedStreamReader : IDisposable
    {
        private const int BufferSize = 128*1024;
        public Encoding Encoding { get; }
        public long Position { get; private set; }

        private readonly byte[] buffer = new byte[BufferSize];
        private int bufferIndex = -1;
        private int bufferLength = 0;
        private readonly Stream stream;
        private readonly bool leaveStreamOpen;

        public UnbufferedStreamReader(string filePath, Encoding encoding = null)
        {
            stream = File.OpenRead(filePath);
            Encoding = encoding ?? Encoding.UTF8;
        }
        public UnbufferedStreamReader(Stream stream, Encoding encoding = null, bool leaveStreamOpen = false)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.leaveStreamOpen = leaveStreamOpen;
            Encoding = encoding ?? Encoding.UTF8;
        }

        public string ReadLine()
        {
            if(bufferIndex < 0)
                FillBuffer();
            var line = new List<byte>();
            var carriageReturnFound = false;
            while (bufferIndex < bufferLength)
            {
                var b = buffer[bufferIndex];
                if (carriageReturnFound && b != '\n')
                    break;
                AdvanceBuffer();
                if(b == '\n')
                    break;
                if (b == '\r')
                {
                    carriageReturnFound = true;
                    continue;
                }
                line.Add(b);
            }

            if (stream.Position >= stream.Length && line.Count == 0)
                return null;

            return Encoding.GetString(line.ToArray());
        }

        private void AdvanceBuffer()
        {
            bufferIndex++;
            Position++;
            if (bufferIndex >= bufferLength) 
                FillBuffer();
        }

        private void FillBuffer()
        {
            bufferLength = stream.Read(buffer, 0, BufferSize);
            bufferIndex = 0;
        }

        public void Dispose()
        {
            if(!leaveStreamOpen)
                stream.Dispose();
        }
    }
}
