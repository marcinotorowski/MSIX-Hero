// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

// ReSharper disable InconsistentNaming
// ReSharper disable CommentTypo

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Otor.MsixHero.Infrastructure.Helpers
{
    [Obfuscation(Exclude = true)]
    public static class NdDll
    {
        [SecurityCritical]
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern bool RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

        public static Version RtlGetVersion()
        {
            var ver = new OSVERSIONINFOEX();
            if (!RtlGetVersion(ref ver) && ver.MajorVersion < 7)
            {
                return default;
            }

            return new Version(ver.MajorVersion, ver.MinorVersion, ver.BuildNumber, 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        [Obfuscation(Exclude = true)]
        private struct OSVERSIONINFOEX
        {
            // The OSVersionInfoSize field must be set to Marshal.SizeOf(typeof(OSVERSIONINFOEX))
            internal int OSVersionInfoSize;
            internal int MajorVersion;
            internal int MinorVersion;
            internal int BuildNumber;
            internal int PlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal string CSDVersion;
            internal ushort ServicePackMajor;
            internal ushort ServicePackMinor;
            internal short SuiteMask;
            internal byte ProductType;
            internal byte Reserved;
        }
    }
}
