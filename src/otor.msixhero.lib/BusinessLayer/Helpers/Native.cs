using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Namotion.Reflection;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Full;
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace otor.msixhero.lib.BusinessLayer.Helpers
{
    internal class Native
    {
        internal static string GetPropertyStringValue(string name, IAppxManifestProperties properties)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return GetStringValue(properties, name);
        }

        internal static bool GetPropertyBoolValue(string name, IAppxManifestProperties properties)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return GetBoolValue(properties, name);
        }
        
        internal static IEnumerable<AppxPackage> QueryPackageInfo(string fullName, PackageConstants flags)
        {
            var disposables = new Stack<object>();
            var infoBuffer = IntPtr.Zero;
            var infoRef = IntPtr.Zero;

            try
            {
                OpenPackageInfoByFullName(fullName, 0, out infoRef);
                if (infoRef != IntPtr.Zero)
                {
                    var len = 0;
                    GetPackageInfo(infoRef, flags, ref len, IntPtr.Zero, out var count);
                    if (len == 0)
                    {
                        yield break;
                    }

                    // ReSharper disable once SuspiciousTypeConversion.Global
                    var factory = (IAppxFactory)new AppxFactory();
                    disposables.Push(factory);

                    infoBuffer = Marshal.AllocHGlobal(len);
                    GetPackageInfo(infoRef, flags, ref len, infoBuffer, out count);
                            
                    for (var i = 0; i < count; i++)
                    {
                        var info = (PACKAGE_INFO)Marshal.PtrToStructure(infoBuffer + i * Marshal.SizeOf(typeof(PACKAGE_INFO)), typeof(PACKAGE_INFO));
                        
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        var package = new AppxPackage();
                        package.FamilyName = Marshal.PtrToStringUni(info.packageFamilyName);
                        package.FullName = Marshal.PtrToStringUni(info.packageFullName);
                        package.Path = Marshal.PtrToStringUni(info.path);
                        package.Publisher = Marshal.PtrToStringUni(info.packageId.publisher);
                        package.PublisherId = Marshal.PtrToStringUni(info.packageId.publisherId);
                        package.ResourceId = Marshal.PtrToStringUni(info.packageId.resourceId);
                        package.ProcessorArchitecture = info.packageId.processorArchitecture;
                        package.Version = new Version(info.packageId.VersionMajor, info.packageId.VersionMinor, info.packageId.VersionBuild, info.packageId.VersionRevision).ToString(4);

                        // read manifest
                        string manifestPath = System.IO.Path.Combine(package.Path, "AppXManifest.xml");
                        const int STGM_SHARE_DENY_NONE = 0x40;
                        IStream strm;
                        SHCreateStreamOnFileEx(manifestPath, STGM_SHARE_DENY_NONE, 0, false, IntPtr.Zero, out strm);
                        disposables.Push(strm);

                        if (strm != null)
                        {
                            var reader = factory.CreateManifestReader(strm); 
                            disposables.Push(reader);
                                    
                            var properties = reader.GetProperties();
                            disposables.Push(properties);

                            var packageId = reader.GetPackageId();
                            try
                            {
                                package.Name = packageId.GetName();
                            }
                            finally
                            {
                                Marshal.ReleaseComObject(packageId);
                            }

                            package.Description = GetPropertyStringValue("Description", properties);
                            package.DisplayName = GetPropertyStringValue("DisplayName", properties);
                            package.Logo = GetPropertyStringValue("Logo", properties);
                            package.PublisherDisplayName = GetPropertyStringValue("PublisherDisplayName", properties);
                            package.IsFramework = GetPropertyBoolValue("Framework", properties);

                            var nativeApplications = reader.GetApplications();
                            disposables.Push(nativeApplications);

                            while (nativeApplications.GetHasCurrent())
                            {
                                var nativeApplication = nativeApplications.GetCurrent();
                                try
                                {
                                    var appx = new AppxApplication();
                                    appx.Description = GetStringValue(nativeApplication, "Description");
                                    appx.Name = GetStringValue(nativeApplication, "Name");
                                    appx.Publisher = GetStringValue(nativeApplication, "Publisher");
                                    appx.DisplayName = GetStringValue(nativeApplication, "DisplayName");
                                    appx.EntryPoint = GetStringValue(nativeApplication, "EntryPoint");
                                    appx.Executable = GetStringValue(nativeApplication, "Executable");
                                    appx.Id = GetStringValue(nativeApplication, "Id");
                                    appx.Logo = GetStringValue(nativeApplication, "Logo");
                                    appx.SmallLogo = GetStringValue(nativeApplication, "SmallLogo");
                                    appx.StartPage = GetStringValue(nativeApplication, "StartPage");
                                    appx.Square150x150Logo = GetStringValue(nativeApplication, "Square150x150Logo");
                                    appx.Square30x30Logo = GetStringValue(nativeApplication, "Square30x30Logo");
                                    appx.BackgroundColor = GetStringValue(nativeApplication, "BackgroundColor");
                                    appx.ForegroundText = GetStringValue(nativeApplication, "ForegroundText");
                                    appx.WideLogo = GetStringValue(nativeApplication, "WideLogo");
                                    appx.Wide310x310Logo = GetStringValue(nativeApplication, "Wide310x310Logo");
                                    appx.ShortName = GetStringValue(nativeApplication, "ShortName");
                                    appx.Square310x310Logo = GetStringValue(nativeApplication, "Square310x310Logo");
                                    appx.Square70x70Logo = GetStringValue(nativeApplication, "Square70x70Logo");
                                    appx.MinWidth = GetStringValue(nativeApplication, "MinWidth");
                                }
                                finally
                                {
                                    Marshal.ReleaseComObject(nativeApplication);
                                }

                                nativeApplications.MoveNext();
                            }

                            if (package.PackageDependencies == null)
                            {
                                package.PackageDependencies = new List<AppxPackageDependency>();
                            }

                            var nativeDependencies = reader.GetPackageDependencies();
                            disposables.Push(nativeDependencies);

                            while (nativeDependencies.GetHasCurrent())
                            {
                                var nativeDependency = nativeDependencies.GetCurrent();
                                try
                                {
                                    var appxDepdendency = new AppxPackageDependency();

                                    ulong u;
                                    nativeDependency.GetMinVersion(out u);
                                    string s;
                                    nativeDependency.GetName(out s);
                                    appxDepdendency.Name = s;
                                    nativeDependency.GetPublisher(out s);
                                    appxDepdendency.Publisher = s;

                                    var bitConvert = BitConverter.GetBytes(u);
                                    appxDepdendency.Version = u == 0 ? null : string.Format(
                                        "{0}.{1}.{2}.{3}",
                                        BitConverter.ToUInt16(bitConvert, 6),
                                        BitConverter.ToUInt16(bitConvert, 4),
                                        BitConverter.ToUInt16(bitConvert, 2),
                                        BitConverter.ToUInt16(bitConvert, 0));

                                    nativeDependencies.MoveNext();
                                    package.PackageDependencies.Add(appxDepdendency);
                                }
                                finally
                                {
                                    Marshal.ReleaseComObject(nativeDependency);
                                }

                            }
                        }

                        yield return package;
                    }
                }
            }
            finally
            {
                while (disposables.TryPop(out var disposable))
                {
                    Marshal.ReleaseComObject(disposable);
                }

                if (infoBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(infoBuffer);
                }

                if (infoRef != IntPtr.Zero)
                {
                    ClosePackageInfo(infoRef);
                }
            }
        }

        public static string LoadResourceString(string packageFullName, string resource)
        {
            if (packageFullName == null)
                throw new ArgumentNullException("packageFullName");

            if (string.IsNullOrWhiteSpace(resource))
                return null;

            const string resourceScheme = "ms-resource:";
            if (!resource.StartsWith(resourceScheme))
                return null;

            string part = resource.Substring(resourceScheme.Length);
            string url;

            if (part.StartsWith("/"))
            {
                url = resourceScheme + "//" + part;
            }
            else
            {
                url = resourceScheme + "///resources/" + part;
            }

            string source = string.Format("@{{{0}? {1}}}", packageFullName, url);
            var sb = new StringBuilder(1024);
            int i = SHLoadIndirectString(source, sb, sb.Capacity, IntPtr.Zero);
            if (i != 0)
                return null;

            return sb.ToString();
        }

        private static string GetStringValue(IAppxManifestProperties props, string name)
        {
            if (props == null)
                return null;

            string value;
            props.GetStringValue(name, out value);
            return value;
        }

        private static bool GetBoolValue(IAppxManifestProperties props, string name)
        {
            bool value;
            props.GetBoolValue(name, out value);
            return value;
        }

        internal static string GetStringValue(IAppxManifestApplication app, string name)
        {
            string value;
            app.GetStringValue(name, out value);
            return value;
        }

        [Guid("5842a140-ff9f-4166-8f5c-62f5b7b0c781"), ComImport]
        private class AppxFactory
        {
        }

        [Guid("BEB94909-E451-438B-B5A7-D79E767B75D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAppxFactory
        {
            void _VtblGap0_2(); // skip 2 methods
            IAppxManifestReader CreateManifestReader(IStream inputStream);
        }

        [Guid("4E1BD148-55A0-4480-A3D1-15544710637C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAppxManifestReader
        {
            IAppxManifestPackageId GetPackageId();
            
            IAppxManifestProperties GetProperties();

            IAppxManifestPackageDependenciesEnumerator GetPackageDependencies();

            void _VtblGap1_4(); // skip 4 methods
            IAppxManifestApplicationsEnumerator GetApplications();
        }

        [Guid("9EB8A55A-F04B-4D0D-808D-686185D4847A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAppxManifestApplicationsEnumerator
        {
            IAppxManifestApplication GetCurrent();
            bool GetHasCurrent();
            bool MoveNext();
        }

        [Guid("b43bbcf9-65a6-42dd-bac0-8c6741e7f5a4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAppxManifestPackageDependenciesEnumerator
        {
            IAppxManifestPackageDependency GetCurrent();
            bool GetHasCurrent();
            bool MoveNext();
        }

        [Guid("5DA89BF4-3773-46BE-B650-7E744863B7E8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAppxManifestApplication
        {
            [PreserveSig]
            int GetStringValue([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] out string value);
        }

        [Guid("e4946b59-733e-43f0-a724-3bde4c1285a0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAppxManifestPackageDependency
        {
            [PreserveSig]
            int GetName([MarshalAs(UnmanagedType.LPWStr)] out string value);

            [PreserveSig]
            int GetPublisher([MarshalAs(UnmanagedType.LPWStr)] out string value);

            [PreserveSig]
            int GetMinVersion([MarshalAs(UnmanagedType.U8)] out ulong value);
        }

        [Guid("03FAF64D-F26F-4B2C-AAF7-8FE7789B8BCA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAppxManifestProperties
        {
            [PreserveSig]
            int GetBoolValue([MarshalAs(UnmanagedType.LPWStr)]string name, out bool value);
            [PreserveSig]
            int GetStringValue([MarshalAs(UnmanagedType.LPWStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] out string vaue);
        }

        [Guid("283ce2d7-7153-4a91-9649-7a0f7240945f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAppxManifestPackageId : IDisposable
        {
            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetName();

            AppxPackageArchitecture GetArchitecture();

            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetPublisher();

            ulong GetVersion();

            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetResourceId();

            bool ComparePublisher([In, MarshalAs(UnmanagedType.LPWStr)] string otherPublisher);

            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetPackageFullName();

            [return: MarshalAs(UnmanagedType.LPWStr)]
            string GetPackageFamilyName();
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        internal static extern int SHCreateStreamOnFileEx(string fileName, int grfMode, int attributes, bool create, IntPtr reserved, out IStream stream);

        [DllImport("user32.dll")]
        internal static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern int OpenPackageInfoByFullName(string packageFullName, int reserved, out IntPtr packageInfoReference);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetPackageInfo(IntPtr packageInfoReference, PackageConstants flags, ref int bufferLength, IntPtr buffer, out int count);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern int ClosePackageInfo(IntPtr packageInfoReference);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetPackageFullName(IntPtr hProcess, ref int packageFullNameLength, StringBuilder packageFullName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetApplicationUserModelId(IntPtr hProcess, ref int applicationUserModelIdLength, StringBuilder applicationUserModelId);

        [Flags]
        internal enum PackageConstants
        {
            PACKAGE_FILTER_ALL_LOADED = 0x00000000,
            PACKAGE_PROPERTY_FRAMEWORK = 0x00000001,
            PACKAGE_PROPERTY_RESOURCE = 0x00000002,
            PACKAGE_PROPERTY_BUNDLE = 0x00000004,
            PACKAGE_FILTER_HEAD = 0x00000010,
            PACKAGE_FILTER_DIRECT = 0x00000020,
            PACKAGE_FILTER_RESOURCE = 0x00000040,
            PACKAGE_FILTER_BUNDLE = 0x00000080,
            PACKAGE_INFORMATION_BASIC = 0x00000000,
            PACKAGE_INFORMATION_FULL = 0x00000100,
            PACKAGE_PROPERTY_DEVELOPMENT_MODE = 0x00010000,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct PACKAGE_INFO
        {
            public int reserved;
            public int flags;
            public IntPtr path;
            public IntPtr packageFullName;
            public IntPtr packageFamilyName;
            public PACKAGE_ID packageId;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct PACKAGE_ID
        {
            public int reserved;
            public AppxPackageArchitecture processorArchitecture;
            public ushort VersionRevision;
            public ushort VersionBuild;
            public ushort VersionMinor;
            public ushort VersionMajor;
            public IntPtr name;
            public IntPtr publisher;
            public IntPtr resourceId;
            public IntPtr publisherId;
        }
    }
}
