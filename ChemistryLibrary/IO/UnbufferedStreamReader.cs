using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class UnbufferedStreamReader : IDisposable
    {
        public Encoding Encoding { get; }
        public long Position => bufferedByte.HasValue ? stream.Position - 1 : stream.Position;

        private byte? bufferedByte = null;
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
            var buffer = new List<byte>();
            if (bufferedByte.HasValue)
            {
                buffer.Add(bufferedByte.Value);
                bufferedByte = null;
            }
            int result;
            var carriageReturnFound = false;
            while ((result = stream.ReadByte()) >= 0)
            {
                var b = (byte)result;
                if(b == '\n')
                    break;
                if (carriageReturnFound)
                {
                    bufferedByte = b;
                    break;
                }
                if (b == '\r')
                {
                    carriageReturnFound = true;
                }
                buffer.Add(b);
            }

            if (stream.Position >= stream.Length && buffer.Count == 0)
                return null;

            return Encoding.GetString(buffer.ToArray());
        }

        public void Dispose()
        {
            if(!leaveStreamOpen)
                stream.Dispose();
        }
    }
}
