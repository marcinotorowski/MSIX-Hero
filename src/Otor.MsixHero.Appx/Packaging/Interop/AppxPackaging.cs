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
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Windows.Security.Cryptography.Core;
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
            return name + "_" + GetPublisherHash(publisherId);
        }

        public static string GetPackageFullName(string name, string publisherId, AppxPackageArchitecture architecture, string version, string resourceId = null)
        {
            var parsedVersion = Version.Parse(version);

            string packageFullName = null;
            var packageId = new PACKAGE_ID
            {
                name = name,
                publisher = publisherId,
                processorArchitecture = (uint)architecture,
                version = new PackageVersion(parsedVersion).Version,
                resourceId = resourceId
            };

            uint packageFamilyNameLength = 0;

            // determine the length (null buffer, should always return 122)
            // ReSharper disable once InvertIf
            if (PackageFullNameFromId(packageId, ref packageFamilyNameLength, null) == 122)
            {
                // insufficient buffer

                var packageFamilyNameBuilder = new StringBuilder((int)packageFamilyNameLength);
                if (PackageFullNameFromId(packageId, ref packageFamilyNameLength, packageFamilyNameBuilder) == 0)
                {
                    packageFullName = packageFamilyNameBuilder.ToString();
                }
            }
            return packageFullName;
        }

        private static string GetPublisherHash(string publisherId)
        {
            using var sha = HashAlgorithm.Create(HashAlgorithmNames.Sha256);
            // ReSharper disable once PossibleNullReferenceException
            var encoded = sha.ComputeHash(Encoding.Unicode.GetBytes(publisherId));
            var binaryString = string.Concat(encoded.Take(8).Select(c => Convert.ToString(c, 2).PadLeft(8, '0'))) + '0'; // representing 65-bits = 13 * 5
            var encodedPublisherId = string.Concat(Enumerable.Range(0, binaryString.Length / 5).Select(i => "0123456789abcdefghjkmnpqrstvwxyz".Substring(Convert.ToInt32(binaryString.Substring(i * 5, 5), 2), 1)));
            return encodedPublisherId;
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
        private static extern uint PackageFullNameFromId(PACKAGE_ID packageId, ref uint packageFamilyNameLength, StringBuilder packageFamilyName);

        [StructLayout(LayoutKind.Explicit)]
        private struct PackageVersion
        {
            public PackageVersion(ulong version) : this()
            {
                Version = version;
            }

            private PackageVersion(ushort major, ushort minor, ushort build, ushort revision) : this()
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
