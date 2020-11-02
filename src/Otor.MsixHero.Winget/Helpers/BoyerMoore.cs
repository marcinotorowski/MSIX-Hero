using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace Otor.MsixHero.Winget.Helpers
{
    internal class BoyerMoore
    {
        private int[] _jumpTable;
        private byte[] _pattern;
        private int _patternLength;
        public BoyerMoore()
        {
        }
        public BoyerMoore(byte[] pattern)
        {
            _pattern = pattern;
            _jumpTable = new int[256];
            _patternLength = _pattern.Length;
            for (var index = 0; index < 256; index++)
                _jumpTable[index] = _patternLength;
            for (var index = 0; index < _patternLength - 1; index++)
                _jumpTable[_pattern[index]] = _patternLength - index - 1;
        }
        public void SetPattern(byte[] pattern)
        {
            _pattern = pattern;
            _jumpTable = new int[256];
            _patternLength = _pattern.Length;
            for (var index = 0; index < 256; index++)
                _jumpTable[index] = _patternLength;
            for (var index = 0; index < _patternLength - 1; index++)
                _jumpTable[_pattern[index]] = _patternLength - index - 1;
        }

        public unsafe int Search(byte[] searchArray, int startIndex = 0)
        {
            if (_pattern == null)
                throw new Exception("Pattern has not been set.");
            if (_patternLength > searchArray.Length)
                throw new Exception("Search Pattern length exceeds search array length.");
            var index = startIndex;
            var limit = searchArray.Length - _patternLength;
            var patternLengthMinusOne = _patternLength - 1;
            fixed (byte* pointerToByteArray = searchArray)
            {
                var pointerToByteArrayStartingIndex = pointerToByteArray + startIndex;
                fixed (byte* pointerToPattern = _pattern)
                {
                    while (index <= limit)
                    {
                        var j = patternLengthMinusOne;
                        while (j >= 0 && pointerToPattern[j] == pointerToByteArrayStartingIndex[index + j])
                            j--;
                        if (j < 0)
                            return index;
                        index += Math.Max(_jumpTable[pointerToByteArrayStartingIndex[index + j]] - _patternLength + 1 + j, 1);
                    }
                }
            }
            return -1;
        }
        public unsafe List<int> SearchAll(byte[] searchArray, int startIndex = 0)
        {
            var index = startIndex;
            var limit = searchArray.Length - _patternLength;
            var patternLengthMinusOne = _patternLength - 1;
            var list = new List<int>();
            fixed (byte* pointerToByteArray = searchArray)
            {
                var pointerToByteArrayStartingIndex = pointerToByteArray + startIndex;
                fixed (byte* pointerToPattern = _pattern)
                {
                    while (index <= limit)
                    {
                        var j = patternLengthMinusOne;
                        while (j >= 0 && pointerToPattern[j] == pointerToByteArrayStartingIndex[index + j])
                            j--;
                        if (j < 0)
                            list.Add(index);
                        index += Math.Max(_jumpTable[pointerToByteArrayStartingIndex[index + j]] - _patternLength + 1 + j, 1);
                    }
                }
            }
            return list;
        }
        public int SuperSearch(byte[] searchArray, int nth, int start = 0)
        {
            var e = start;
            var c = 0;
            do
            {
                e = Search(searchArray, e);
                if (e == -1)
                    return -1;
                c++;
                e++;
            } while (c < nth);
            return e - 1;
        }
    }
}