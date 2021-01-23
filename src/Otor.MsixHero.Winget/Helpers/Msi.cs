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
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable IdentifierTypo

namespace Otor.MsixHero.Winget.Helpers
{
    public class Msi
    {
        [DllImport("msi.dll", SetLastError = true)]
        static extern uint MsiOpenDatabase(string szDatabasePath, IntPtr phPersist, out IntPtr phDatabase);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        static extern int MsiDatabaseOpenViewW(IntPtr hDatabase, [MarshalAs(UnmanagedType.LPWStr)] string szQuery, out IntPtr phView);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        static extern int MsiViewExecute(IntPtr hView, IntPtr hRecord);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        static extern uint MsiViewFetch(IntPtr hView, out IntPtr hRecord);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        static extern int MsiRecordGetString(IntPtr hRecord, int iField,
           [Out] StringBuilder szValueBuf, ref int pcchValueBuf);

        [DllImport("msi.dll", ExactSpelling = true)]
        static extern IntPtr MsiCreateRecord(uint cParams);

        [DllImport("msi.dll", ExactSpelling = true)]
        static extern uint MsiCloseHandle(IntPtr hAny);

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        internal struct FILETIME
        {
            /// <summary>Low-order part of the file time.</summary>
            public uint LowDateTime;
            /// <summary>High-order part of the file time.</summary>
            public uint HighDateTime;
        }

        [DllImport("msi.dll", EntryPoint = "MsiSummaryInfoGetPropertyW", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern uint MsiSummaryInfoGetProperty(uint summaryInfo, uint property, out uint dataType, out uint integerValue, ref FILETIME fileTimeValue, StringBuilder stringValueBuf, ref int stringValueBufSize);

        [DllImport("msi.dll", EntryPoint = "MsiGetSummaryInformationW", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern uint MsiGetSummaryInformation(uint install, string path, uint updateCount, out uint summaryInfo);

        public Dictionary<string, string> GetProperties(string fileName)
        {
            const string sqlStatement = "SELECT * FROM Property";
            IntPtr phDatabase = default;
            IntPtr phView = default;
            IntPtr hRecord = default;

            var dictionary = new Dictionary<string, string>();

            try
            {
                var szValueBuf = new StringBuilder();
                var pcchValueBuf = 0;

                // Open the MSI database in the input file 
                if (MsiOpenDatabase(fileName, IntPtr.Zero, out phDatabase) != 0)
                {
                    return dictionary;
                }

                hRecord = MsiCreateRecord(1);

                // Open a view on the Property table for the version property 
                if (MsiDatabaseOpenViewW(phDatabase, sqlStatement, out phView) != 0)
                {
                    return dictionary;
                }

                // Execute the view query 
                if (MsiViewExecute(phView, hRecord) != 0)
                {
                    return dictionary;
                }

                // Get the record from the view 
                while ((MsiViewFetch(phView, out hRecord)) == 0)
                {
                    szValueBuf.Clear();

                    // Get the version from the data 
                    if (MsiRecordGetString(hRecord, 1, szValueBuf, ref pcchValueBuf) == 234)
                    {
                        szValueBuf.Capacity = Math.Max(pcchValueBuf + 1, szValueBuf.Capacity);
                        pcchValueBuf++;
                        MsiRecordGetString(hRecord, 1, szValueBuf, ref pcchValueBuf);
                    }

                    var propName = szValueBuf.ToString();

                    szValueBuf.Clear();
                    if (MsiRecordGetString(hRecord, 2, szValueBuf, ref pcchValueBuf) == 234)
                    {
                        szValueBuf.Capacity = Math.Max(pcchValueBuf + 1, szValueBuf.Capacity);
                        pcchValueBuf++;
                        MsiRecordGetString(hRecord, 2, szValueBuf, ref pcchValueBuf);
                    }
                        
                    dictionary[propName] = szValueBuf.ToString();
                }

                var stringValue = new StringBuilder("") { Capacity = 255 };
                var bufSize = 0;
                var timeValue = new FILETIME();

                uint sis;
                if (MsiGetSummaryInformation((uint) phDatabase, fileName, 1, out sis) != 0)
                {
                    return dictionary;
                }
                
                var ret = MsiSummaryInfoGetProperty(
                    sis,
                7,
                out _,
                out _,
                ref timeValue,
                stringValue,
                ref bufSize);

                if (ret == 234)
                {
                    bufSize++;
                    stringValue.Capacity = bufSize;
                    MsiSummaryInfoGetProperty(
                        sis,
                        7,
                        out _,
                        out _,
                        ref timeValue,
                        stringValue,
                        ref bufSize);
                }

                dictionary["Template"] = stringValue.ToString();
            }
            finally
            {
                MsiCloseHandle(hRecord);
                MsiCloseHandle(phView);
                MsiCloseHandle(phDatabase);
            }

            return dictionary;
        }
    }
}
