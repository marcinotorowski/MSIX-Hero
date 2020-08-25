using System;
using System.Runtime.InteropServices;
using System.Text;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local

namespace Otor.MsixHero.Appx.Packaging.Interop
{
    public static class AppxPackaging
    {
        public static string GetPackageFamilyName(string name, string publisherId)
        {
            string packageFamilyName = null;
            PACKAGE_ID packageId = new PACKAGE_ID
            {
                name = name,
                publisher = publisherId
            };
            uint packageFamilyNameLength = 0;
            //First get the length of the Package Name -> Pass NULL as Output Buffer
            if (PackageFamilyNameFromId(packageId, ref packageFamilyNameLength, null) == 122) //ERROR_INSUFFICIENT_BUFFER
            {
                StringBuilder packageFamilyNameBuilder = new StringBuilder((int)packageFamilyNameLength);
                if (PackageFamilyNameFromId(packageId, ref packageFamilyNameLength, packageFamilyNameBuilder) == 0)
                {
                    packageFamilyName = packageFamilyNameBuilder.ToString();
                }
            }
            return packageFamilyName;
        }
        
        public static string GetPackageFullName(string name, string publisherId, AppxPackageArchitecture architecture, string version, string resourceId = null)
        {
            var parsedVersion = Version.Parse(version);

            string packageFullName = null;
            PACKAGE_ID packageId = new PACKAGE_ID
            {
                name = name,
                publisher = publisherId,
                processorArchitecture = (uint)architecture,
                version = new PackageVersion(parsedVersion).Version,
                resourceId = resourceId
            };

            uint packageFamilyNameLength = 0;
            //First get the length of the Package Name -> Pass NULL as Output Buffer
            if (PackageFullNameFromId(packageId, ref packageFamilyNameLength, null) == 122) //ERROR_INSUFFICIENT_BUFFER
            {
                StringBuilder packageFamilyNameBuilder = new StringBuilder((int)packageFamilyNameLength);
                if (PackageFullNameFromId(packageId, ref packageFamilyNameLength, packageFamilyNameBuilder) == 0)
                {
                    packageFullName = packageFamilyNameBuilder.ToString();
                }
            }
            return packageFullName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        private class PACKAGE_ID
        {
#pragma warning disable 649
            public uint reserved;
#pragma warning restore 649
            public uint processorArchitecture;
            public ulong version;
            public string name;
            public string publisher;
            public string resourceId;
#pragma warning disable 649
            public string publisherId;
#pragma warning restore 649
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint PackageFamilyNameFromId(PACKAGE_ID packageId, ref uint packageFamilyNameLength, StringBuilder packageFamilyName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint PackageFullNameFromId(PACKAGE_ID packageId, ref uint packageFamilyNameLength, StringBuilder packageFamilyName);

        [StructLayout(LayoutKind.Explicit)]
        private struct PackageVersion
        {
            public PackageVersion(ulong version) : this()
            {
                Version = version;
            }

            public PackageVersion(ushort major, ushort minor, ushort build, ushort revision) : this()
            {
                Revision = revision;
                Build = build;
                Minor = minor;
                Major = major;
            }

            public PackageVersion(Version version) : this((ushort)version.Major, (ushort)version.Minor, (ushort)version.Build, (ushort)version.Revision)
            {
            }

            public PackageVersion(string version) : this(System.Version.Parse(version))
            {
            }


            [FieldOffset(0 * sizeof(ulong))]
            public ulong Version;

            [FieldOffset(0 * sizeof(ushort))]
            public ushort Revision;

            [FieldOffset(1 * sizeof(ushort))]
            public ushort Build;

            [FieldOffset(2 * sizeof(ushort))]
            public ushort Minor;

            [FieldOffset(3 * sizeof(ushort))]
            public ushort Major;
        }
    }
}
