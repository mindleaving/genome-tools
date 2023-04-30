using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.Extensions;

namespace GenomeTools.ChemistryLibrary.IO.Bam
{
    public class BamTagValue
    {
        public BamTagValue(
            object value,
            int bytesRead)
        {
            Value = value;
            BytesRead = bytesRead;
        }

        public object Value { get; }
        public int BytesRead { get; }
    }
    public class BamTagReader
    {
        public BamTagValue ReadTagValue(BinaryReader reader)
        {
            var type = (char)reader.ReadByte();
            switch (type)
            {
                case 'A':
                    return new((char)reader.ReadByte(), 1 + 1); // 1 for type-byte (above) + 1 for value byte.
                case 'B':
                    return ReadBArray(reader);
                case 'Z':
                {
                    var str = ReadNullTerminatedString(reader);
                    return new BamTagValue(str.Value, 1 + str.BytesRead);
                }
                case 'H':
                {
                    var str = ReadNullTerminatedString(reader);
                    return new(ParserHelpers.ParseHexString((string)str.Value), 1 + str.BytesRead);
                }
                case 'c':
                case 'C':
                case 's':
                case 'S':
                case 'i':
                case 'I':
                case 'f':
                    var value = ReadTypedItem(reader, type);
                    return new(value.Value, 1 + value.BytesRead);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"Unknown tag-type '{type}'. Expected one of: ABZHcCsSiIf");
            }
        }

        private BamTagValue ReadNullTerminatedString(BinaryReader reader)
        {
            var stringBuilder = new StringBuilder();
            var bytesRead = 0;
            while (true)
            {
                var c = reader.ReadChar();
                bytesRead++;
                if (c == '\0')
                    return new(stringBuilder.ToString(), bytesRead);
                stringBuilder.Append(c);
            }
        }

        private BamTagValue ReadBArray(BinaryReader reader)
        {
            var itemType = (char)reader.ReadByte();
            var itemCount = reader.ReadUInt32();
            var items = new List<BamTagValue>();
            for (int i = 0; i < itemCount; i++)
            {
                var item = ReadTypedItem(reader, itemType);
                items.Add(item);
            }
            return new(items, 1 + 4 + items.Sum(x => x.BytesRead));
        }

        private BamTagValue ReadTypedItem(BinaryReader reader, char itemType)
        {
            switch (itemType)
            {
                case 'c':
                    return new(reader.ReadSByte(), 1);
                case 'C':
                    return new(reader.ReadByte(), 1);
                case 's':
                    return new(reader.ReadInt16(), 2);
                case 'S':
                    return new(reader.ReadUInt16(), 2);
                case 'i':
                    return new(reader.ReadInt32(), 4);
                case 'I':
                    return new(reader.ReadUInt32(), 4);
                case 'f':
                    return new(reader.ReadSingle(), 4);
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType), $"Unknown item type '{itemType}' for B-tag array");
            }
        }
    }
}
