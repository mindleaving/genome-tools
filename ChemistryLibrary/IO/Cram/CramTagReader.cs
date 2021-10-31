using System;
using System.Collections.Generic;
using System.Text;
using GenomeTools.ChemistryLibrary.Extensions;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramTagReader
    {
        public object ReadTagValue(CramBinaryReader reader)
        {
            var type = (char)reader.ReadByte();
            switch (type)
            {
                case 'A':
                    return (char)reader.ReadByte();
                case 'B':
                    return ReadBArray(reader);
                case 'Z':
                {
                    return ReadNullTerminatedString(reader);
                }
                case 'H':
                {
                    var str = ReadNullTerminatedString(reader);
                    return ParserHelpers.ParseHexString(str);
                }
                case 'c':
                case 'C':
                case 's':
                case 'S':
                case 'i':
                case 'I':
                case 'f':
                    return ReadTypedItem(reader, type);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"Unknown tag-type '{type}'. Expected one of: ABZHcCsSiIf");
            }
        }

        private string ReadNullTerminatedString(CramBinaryReader reader)
        {
            var stringBuilder = new StringBuilder();
            while (true)
            {
                var c = reader.ReadChar();
                if (c == '\0')
                    return stringBuilder.ToString();
                stringBuilder.Append(c);
            }
        }

        private List<object> ReadBArray(CramBinaryReader reader)
        {
            var itemType = (char)reader.ReadByte();
            var itemCount = reader.ReadUInt32();
            var items = new List<object>();
            for (int i = 0; i < itemCount; i++)
            {
                var item = ReadTypedItem(reader, itemType);
                items.Add(item);
            }
            return items;
        }

        private object ReadTypedItem(CramBinaryReader reader, char itemType)
        {
            switch (itemType)
            {
                case 'c':
                    return reader.ReadSByte();
                case 'C':
                    return reader.ReadByte();
                case 's':
                    return reader.ReadInt16();
                case 'S':
                    return reader.ReadUInt16();
                case 'i':
                    return reader.ReadInt32();
                case 'I':
                    return reader.ReadUInt32();
                case 'f':
                    return reader.ReadSingle();
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType), $"Unknown item type '{itemType}' for B-tag array");
            }
        }
    }
}
