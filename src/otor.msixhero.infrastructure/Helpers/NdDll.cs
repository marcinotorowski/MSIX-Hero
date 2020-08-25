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
            if (!RtlGetVersion(ref ver))
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
