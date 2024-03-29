using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class HuffmanIntCramEncoding : ICramEncoding<int>
    {
        private readonly Dictionary<int, int> codewordSymbolMap;
        private readonly Dictionary<int, int> symbolCodewordMap;
        private readonly Dictionary<int, int> symbolCodeLengthMap;
        private readonly int maxCodeword;

        public HuffmanIntCramEncoding(List<HuffmanCodeSymbol> symbols)
        {
            Symbols = symbols;
            codewordSymbolMap = GenerateCanonicalCodewords(symbols);
            maxCodeword = codewordSymbolMap.Keys.Max();
            symbolCodewordMap = codewordSymbolMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            symbolCodeLengthMap = symbols.ToDictionary(x => x.Symbol, x => x.CodeLength);
        }

        public Codec CodecId => Codec.Huffman;
        public List<HuffmanCodeSymbol> Symbols { get; }


        public void Encode(int item, BitStream stream)
        {
            if (!symbolCodewordMap.ContainsKey(item))
                throw new ArgumentException($"Cannot encode symbol '{item}' that's not in the alphabet.");
            var codeword = symbolCodewordMap[item];
            var codeLength = symbolCodeLengthMap[item];
            var allCodewordIntegerBits = new BitArray(BitConverter.GetBytes(codeword));
            for (int targetIndex = 0; targetIndex < codeLength; targetIndex++)
            {
                var sourceIndex = codeLength - 1 - targetIndex;
                stream.WriteBit(allCodewordIntegerBits[sourceIndex]);
            }
        }

        public int Decode(BitStream bits)
        {
            if (Symbols.Count == 1)
                return Symbols[0].Symbol;
            var codeword = 0;
            while (codeword < maxCodeword)
            {
                codeword = (codeword << 1) + (bits.ReadBit() ? 1 : 0);
                if (codewordSymbolMap.ContainsKey(codeword))
                    return codewordSymbolMap[codeword];
            }
            throw new Exception("Reached end of bit stream but didn't recognize a valid Huffman codeword");
        }

        private Dictionary<int, int> GenerateCanonicalCodewords(List<HuffmanCodeSymbol> symbolsWithBitLength)
        {
            var alphabet = symbolsWithBitLength.OrderBy(x => x.CodeLength).ThenBy(x => x.Symbol).ToList();
            var canonicalCodewords = new Dictionary<int, int>
            {
                { 0, alphabet[0].Symbol }
            };
            var lastCodeWord = 0;
            foreach (var symbol in alphabet.Skip(1))
            {
                var codeword = GetNextCodeword(lastCodeWord, symbol.CodeLength);
                canonicalCodewords.Add(codeword, symbol.Symbol);
                lastCodeWord = codeword;
            }
            return canonicalCodewords;
        }

        private int GetNextCodeword(int lastCodeWord, int symbolBitLength)
        {
            if (symbolBitLength == 0)
                return 0;
            var codeWord = lastCodeWord + 1;
            while ((codeWord & (1 << symbolBitLength-1)) == 0)
            {
                codeWord <<= 1;
            }
            return codeWord;
        }
    }
}