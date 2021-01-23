// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections.Generic;

namespace Otor.MsixHero.Winget.Helpers
{
    internal class BoyerMoore
    {
        private int[] jumpTable;
        private byte[] pattern;
        private int patternLength;
        
        public BoyerMoore(byte[] pattern)
        {
            this.pattern = pattern;
            jumpTable = new int[256];
            patternLength = this.pattern.Length;
            for (var index = 0; index < 256; index++)
            {
                jumpTable[index] = patternLength;
            }

            for (var index = 0; index < patternLength - 1; index++)
            {
                jumpTable[this.pattern[index]] = patternLength - index - 1;
            }
        }
        
        public void SetPattern(byte[] newPattern)
        {
            this.pattern = newPattern;
            jumpTable = new int[256];
            patternLength = this.pattern.Length;
            
            for (var index = 0; index < 256; index++)
            {
                jumpTable[index] = patternLength;
            }
            
            for (var index = 0; index < patternLength - 1; index++)
            {
                jumpTable[this.pattern[index]] = patternLength - index - 1;
            }
        }

        public unsafe int Search(byte[] searchArray, int startIndex = 0)
        {
            if (pattern == null)
            {
                throw new Exception("Pattern has not been set.");
            }
            
            if (patternLength > searchArray.Length)
            {
                throw new Exception("Search Pattern length exceeds search array length.");
            }
            
            var index = startIndex;
            var limit = searchArray.Length - patternLength;
            var patternLengthMinusOne = patternLength - 1;
            fixed (byte* pointerToByteArray = searchArray)
            {
                var pointerToByteArrayStartingIndex = pointerToByteArray + startIndex;
                fixed (byte* pointerToPattern = pattern)
                {
                    while (index <= limit)
                    {
                        var j = patternLengthMinusOne;
                        while (j >= 0 && pointerToPattern[j] == pointerToByteArrayStartingIndex[index + j])
                        {
                            j--;
                        }
                        
                        if (j < 0)
                        {
                            return index;
                        }
                        
                        index += Math.Max(jumpTable[pointerToByteArrayStartingIndex[index + j]] - patternLength + 1 + j, 1);
                    }
                }
            }
            return -1;
        }
        public unsafe List<int> SearchAll(byte[] searchArray, int startIndex = 0)
        {
            var index = startIndex;
            var limit = searchArray.Length - patternLength;
            var patternLengthMinusOne = patternLength - 1;
            var list = new List<int>();
            fixed (byte* pointerToByteArray = searchArray)
            {
                var pointerToByteArrayStartingIndex = pointerToByteArray + startIndex;
                fixed (byte* pointerToPattern = pattern)
                {
                    while (index <= limit)
                    {
                        var j = patternLengthMinusOne;
                        while (j >= 0 && pointerToPattern[j] == pointerToByteArrayStartingIndex[index + j])
                        {
                            j--;
                        }
                        
                        if (j < 0)
                        {
                            list.Add(index);
                        }
                        
                        index += Math.Max(jumpTable[pointerToByteArrayStartingIndex[index + j]] - patternLength + 1 + j, 1);
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
                {
                    return -1;
                }
                c++;
                e++;
            } while (c < nth);
            
            return e - 1;
        }
    }
}