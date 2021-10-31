using System.IO;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public static class RansDecoder
    {
        public static void Encode(Stream input, Stream output)
        {
            throw new System.NotImplementedException();
        }

        public static void Decode(Stream input, Stream output)
        {
            using var reader = new CramBinaryReader(input, keepStreamOpen: true);
            var order = reader.ReadByte();
            var compressedSize = reader.ReadUInt32();
            var uncompressedSize = reader.ReadUInt32();

            if (order == 0)
            {
                DecodeOrder0(reader, output, uncompressedSize);
            }
            else
            {
                DecodeOrder1(reader, output, uncompressedSize);
            }
        }

        private static void DecodeOrder0(CramBinaryReader reader, Stream output, uint uncompressedSize)
        {
            var frequencyTable = ReadOrder0FrequencyTable(reader);
            const int StateCount = 4;
            var states = new uint[StateCount];
            for (int stateIndex = 0; stateIndex < StateCount; stateIndex++)
            {
                states[stateIndex] = reader.ReadUInt32();
            }

            var outputByteIndex = 0u;
            while (outputByteIndex < uncompressedSize)
            {
                for (int stateIndex = 0; stateIndex < StateCount; stateIndex++)
                {
                    if(outputByteIndex + stateIndex >= uncompressedSize)
                        return;
                    var state = states[stateIndex];
                    var frequency = GetCummulativeFrequency(state);
                    var symbol = ConvertFrequencyToSymbol(frequencyTable, frequency);
                    output.WriteByte(symbol);
                    var cummulativeFrequency = frequencyTable.CummulativeFrequency[symbol];
                    var symbolFrequency = frequencyTable.SymbolFrequency[symbol];
                    states[stateIndex] = AdvanceState(state, cummulativeFrequency, symbolFrequency);
                    states[stateIndex] = RenormState(states[stateIndex], reader);
                }

                outputByteIndex += StateCount;
            }
        }

        private static void DecodeOrder1(CramBinaryReader reader, Stream output, uint uncompressedSize)
        {
            var frequencyTable = ReadOrder1FrequencyTable(reader);
            const uint StateCount = 4;
            var states = new uint[StateCount];
            var lowerBounds = new byte[StateCount];
            for (int stateIndex = 0; stateIndex < StateCount; stateIndex++)
            {
                states[stateIndex] = reader.ReadUInt32();
            }

            var outputByteIndex = 0u;
            var batchSize = uncompressedSize / StateCount; // Intentionally uses the truncation of the integer division
            var useLocalMemoryStream = !(output is MemoryStream);
            // Use a memory stream for performance and to ensure that we can seek
            var localOutputStream = useLocalMemoryStream
                ? new MemoryStream(new byte[uncompressedSize])
                : output;
            while (outputByteIndex < batchSize)
            {
                for (int stateIndex = 0; stateIndex < StateCount; stateIndex++)
                {
                    var state = states[stateIndex];
                    var frequency = GetCummulativeFrequency(state);
                    var context = lowerBounds[stateIndex];
                    var contextFrequencyTable = frequencyTable.ContextFrequencyTables[context];
                    var symbol = ConvertFrequencyToSymbol(contextFrequencyTable, frequency);
                    localOutputStream.Seek(outputByteIndex + stateIndex * batchSize, SeekOrigin.Begin);
                    localOutputStream.WriteByte(symbol);
                    var cummulativeFrequency = contextFrequencyTable.CummulativeFrequency[symbol];
                    var symbolFrequency = contextFrequencyTable.SymbolFrequency[symbol];
                    states[stateIndex] = AdvanceState(state, cummulativeFrequency, symbolFrequency);
                    states[stateIndex] = RenormState(states[stateIndex], reader);
                    lowerBounds[stateIndex] = symbol;
                }

                outputByteIndex++;
            }

            // Handle remaining bytes
            outputByteIndex *= StateCount;
            localOutputStream.Seek(outputByteIndex + (StateCount-1) * batchSize, SeekOrigin.Begin);
            while (outputByteIndex < uncompressedSize)
            {
                var stateIndex = StateCount - 1;
                var state = states[stateIndex];
                var frequency = GetCummulativeFrequency(state);
                var context = lowerBounds[stateIndex];
                var contextFrequencyTable = frequencyTable.ContextFrequencyTables[context];
                var symbol = ConvertFrequencyToSymbol(contextFrequencyTable, frequency);
                localOutputStream.WriteByte(symbol);
                var cummulativeFrequency = contextFrequencyTable.CummulativeFrequency[symbol];
                var symbolFrequency = contextFrequencyTable.SymbolFrequency[symbol];
                states[stateIndex] = AdvanceState(state, cummulativeFrequency, symbolFrequency);
                states[stateIndex] = RenormState(states[stateIndex], reader);
                lowerBounds[stateIndex] = symbol;
                outputByteIndex++;
            }

            localOutputStream.Seek(0, SeekOrigin.Begin);
            if(useLocalMemoryStream)
            {
                localOutputStream.CopyTo(output);
                localOutputStream.Dispose();
            }
        }

        private static RansOrder1FrequencyTable ReadOrder1FrequencyTable(CramBinaryReader reader)
        {
            var frequencyTable = new RansOrder1FrequencyTable();
            var symbol = reader.ReadByte();
            var lastSymbol = symbol;
            var runLengthEncoding = 0;
            do
            {
                var contextFrequencyTable = ReadOrder0FrequencyTable(reader);
                frequencyTable.ContextFrequencyTables[symbol] = contextFrequencyTable;
                if (runLengthEncoding > 0)
                {
                    runLengthEncoding--;
                    symbol++;
                }
                else
                {
                    symbol = reader.ReadByte();
                    if (symbol == lastSymbol + 1)
                    {
                        runLengthEncoding = reader.ReadByte();
                    }
                }

                lastSymbol = symbol;
            } while (symbol != 0x00);

            return frequencyTable;
        }

        private static RansOrder0FrequencyTable ReadOrder0FrequencyTable(CramBinaryReader reader)
        {
            var frequencyTable = new RansOrder0FrequencyTable();
            var symbol = reader.ReadByte();
            var lastSymbol = symbol;
            var runLengthEncoding = 0;
            do
            {
                var frequency = reader.ReadItf8();
                frequencyTable.SymbolFrequency[symbol] = (ushort)frequency;
                if (runLengthEncoding > 0)
                {
                    runLengthEncoding--;
                    symbol++;
                }
                else
                {
                    symbol = reader.ReadByte();
                    if (symbol == lastSymbol + 1)
                    {
                        runLengthEncoding = reader.ReadByte();
                    }
                }
                lastSymbol = symbol;
            } while (symbol != 0x0);

            frequencyTable.CummulativeFrequency[0] = 0;
            for (symbol = 0; symbol < byte.MaxValue; symbol++)
            {
                var previousCummulativeFrequency = frequencyTable.CummulativeFrequency[symbol];
                var cummulativeFrequency = (ushort)(previousCummulativeFrequency + frequencyTable.SymbolFrequency[symbol]);
                frequencyTable.CummulativeFrequency[(byte)(symbol + 1)] = cummulativeFrequency;
                for (int frequency = previousCummulativeFrequency; frequency <= cummulativeFrequency; frequency++)
                {
                    frequencyTable.InverseCummulativeLookup[frequency] = symbol;
                }
            }
            return frequencyTable;
        }

        private static ushort GetCummulativeFrequency(uint state)
        {
            return (ushort)(state & 0xfff);
        }

        private static byte ConvertFrequencyToSymbol(RansOrder0FrequencyTable frequencyTable, ushort frequency)
        {
            return frequencyTable.InverseCummulativeLookup[frequency];
        }

        private static uint AdvanceState(uint state, ushort cummulativeFrequency, ushort frequency)
        {
            return frequency * (state >> 12) + (state & 0xfff) - cummulativeFrequency;
        }

        private static uint RenormState(uint state, CramBinaryReader reader)
        {
            while (state < (1 << 23))
            {
                state = (state << 8) + reader.ReadByte();
            }
            return state;
        }

        public class RansOrder0FrequencyTable
        {
            public readonly ushort[] SymbolFrequency = new ushort[256];
            public readonly ushort[] CummulativeFrequency = new ushort[256];
            public readonly byte[] InverseCummulativeLookup = new byte[0x1000];
        }

        public class RansOrder1FrequencyTable
        {
            public readonly RansOrder0FrequencyTable[] ContextFrequencyTables = new RansOrder0FrequencyTable[256];

            public RansOrder1FrequencyTable()
            {
                for (int i = 0; i < ContextFrequencyTables.Length; i++)
                {
                    ContextFrequencyTables[i] = new RansOrder0FrequencyTable();
                }
            }
        }
    }
}
