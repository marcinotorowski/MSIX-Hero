using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using otor.msixhero.lib.BusinessLayer.Appx.Detection;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Manifest.Summary;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Progress;
using Package = Windows.ApplicationModel.Package;

namespace otor.msixhero.lib.BusinessLayer.Appx.Details
{
    public class PackageDetailsProvider
    {
        public Task<AppxPackage> GetPackage(string packageName,
            PackageFindMode mode = PackageFindMode.CurrentUser, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = default)
        {
            return this.GetPackage(packageName, mode, false, cancellationToken, progress);
        }

        private async Task<AppxPackage> GetPackage(string packageName,
            PackageFindMode mode = PackageFindMode.CurrentUser, 
            bool lookForAddons = false,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = default)
        {
            var result = await Task.Run(() =>
            {
                var nativePkg = Native.QueryPackageInfo(
                    packageName,
                    lookForAddons 
                    ? Native.PackageConstants.PACKAGE_INFORMATION_FULL | Native.PackageConstants.PACKAGE_FILTER_OPTIONAL
                    : Native.PackageConstants.PACKAGE_INFORMATION_FULL);
                
                AppxPackage mainApp = null;
                IList<AppxPackage> dependencies = new List<AppxPackage>();

                foreach (var item in nativePkg)
                {
                    if (mainApp == null)
                    {
                        mainApp = item;
                    }
                    else
                    {
                        dependencies.Add(item);
                    }
                }

                if (mainApp == null)
                {
                    return null;
                }

                foreach (var dependency in mainApp.PackageDependencies)
                {
                    dependency.Dependency = dependencies.FirstOrDefault(d =>
                        d.Publisher == dependency.Publisher &&
                        d.Name == dependency.Name &&
                        Version.Parse(d.Version) >= Version.Parse(dependency.Version));
                }

                return mainApp;
            }, cancellationToken).ConfigureAwait(false);
            
            var pkgMan = new PackageManager();
            Package managedPackage;

            switch (mode)
            {
                case PackageFindMode.CurrentUser:
                    managedPackage = pkgMan.FindPackageForUser(string.Empty, packageName);
                    break;
                case PackageFindMode.AllUsers:
                    managedPackage = pkgMan.FindPackage(packageName);
                    break;
                default:
                    throw new NotSupportedException();
            }

            result.Addons = new List<AppxPackage>();

            if (managedPackage != null)
            {
                foreach (var managedDependency in managedPackage.Dependencies.Where(p => p.IsOptional))
                {
                    if (result.PackageDependencies.Any(mpd => mpd.Dependency.FullName == managedDependency.Id.FullName))
                    {
                        continue;
                    }

                    var dependency = await this.GetPackage(managedDependency.Id.FullName, mode, true, cancellationToken, progress).ConfigureAwait(false);
                    if (dependency == null)
                    {
                        continue;
                    }

                    result.Addons.Add(dependency);
                }
            }

            return result;
        }

        // ReSharper disable UnusedMember.Global
        // ReSharper disable InconsistentNaming
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        internal class Native
        {
            public const int PACKAGE_PROPERTY_DEVELOPMENT_MODE = 0x00010000;
            public const int PACKAGE_PROPERTY_RESOURCE = 0x00000002;
            public const int PACKAGE_PROPERTY_BUNDLE = 0x00000004;
            public const int PACKAGE_PROPERTY_OPTIONAL = 0x00000008;
            public const int PACKAGE_PROPERTY_FRAMEWORK = 0x00000001;

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

            private static string ConvertRelativeLogoPathToAbsoluteExisting(string manifestRoot, string logoPath)
            {
                if (string.IsNullOrEmpty(logoPath))
                {
                    return null;
                }

                logoPath = logoPath.Replace("/", "\\");
                var p = Path.Combine(manifestRoot, logoPath);
                if (File.Exists(p))
                {
                    return p;
                }

                var extension = Path.GetExtension(logoPath);
                logoPath = logoPath.Substring(0, logoPath.Length - extension.Length) + ".scale-100" + extension;

                p = Path.Combine(manifestRoot, logoPath);
                if (File.Exists(p))
                {
                    return p;
                }

                return null;
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
                            package.IsDevelopment = PACKAGE_PROPERTY_DEVELOPMENT_MODE == (info.flags & PACKAGE_PROPERTY_DEVELOPMENT_MODE);
                            package.IsBundle = PACKAGE_PROPERTY_BUNDLE == (info.flags & PACKAGE_PROPERTY_BUNDLE);
                            package.IsResource = PACKAGE_PROPERTY_RESOURCE == (info.flags & PACKAGE_PROPERTY_RESOURCE);
                            package.IsOptional = PACKAGE_PROPERTY_OPTIONAL == (info.flags & PACKAGE_PROPERTY_OPTIONAL);
                            package.IsFramework = PACKAGE_PROPERTY_FRAMEWORK == (info.flags & PACKAGE_PROPERTY_FRAMEWORK);

                            // read manifest
                            string manifestPath = Path.Combine(package.Path, "AppXManifest.xml");
                            const int STGM_SHARE_DENY_NONE = 0x40;
                            SHCreateStreamOnFileEx(manifestPath, STGM_SHARE_DENY_NONE, 0, false, IntPtr.Zero, out var stream);
                            disposables.Push(stream);

                            if (stream != null)
                            {
                                var reader = (IAppxManifestReader4)factory.CreateManifestReader(stream);
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
                                package.Logo = Path.Combine(package.Path, GetPropertyStringValue("Logo", properties));
                                package.PublisherDisplayName = GetPropertyStringValue("PublisherDisplayName", properties);
                                // we already know the framework and so from package info
                                // package.IsFramework = GetPropertyBoolValue("Framework", properties); 

                                if (package.PackageDependencies == null)
                                {
                                    package.PackageDependencies = new List<AppxPackageDependency>();
                                }

                                if (package.OperatingSystemDependencies == null)
                                {
                                    package.OperatingSystemDependencies = new List<AppxOperatingSystemDependency>();
                                }

                                if (package.Applications == null)
                                {
                                    package.Applications = new List<AppxApplication>();
                                }

                                var nativeApplications = reader.GetApplications();
                                disposables.Push(nativeApplications);

                                var manifestRoot = Path.GetDirectoryName(manifestPath);

                                var psfReader = new PsfReader();
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
                                        appx.Logo = ConvertRelativeLogoPathToAbsoluteExisting(manifestRoot, GetStringValue(nativeApplication, "Logo"));
                                        appx.SmallLogo = ConvertRelativeLogoPathToAbsoluteExisting(manifestRoot, GetStringValue(nativeApplication, "SmallLogo"));
                                        appx.StartPage = GetStringValue(nativeApplication, "StartPage");
                                        appx.Square150x150Logo = ConvertRelativeLogoPathToAbsoluteExisting(manifestRoot, GetStringValue(nativeApplication, "Square150x150Logo"));
                                        appx.Square30x30Logo = ConvertRelativeLogoPathToAbsoluteExisting(manifestRoot, GetStringValue(nativeApplication, "Square30x30Logo"));
                                        appx.BackgroundColor = GetStringValue(nativeApplication, "BackgroundColor");
                                        appx.ForegroundText = GetStringValue(nativeApplication, "ForegroundText");
                                        appx.WideLogo = ConvertRelativeLogoPathToAbsoluteExisting(manifestRoot, GetStringValue(nativeApplication, "WideLogo"));
                                        appx.Wide310x310Logo = ConvertRelativeLogoPathToAbsoluteExisting(manifestRoot, GetStringValue(nativeApplication, "Wide310x310Logo"));
                                        appx.ShortName = GetStringValue(nativeApplication, "ShortName");
                                        appx.Square310x310Logo = ConvertRelativeLogoPathToAbsoluteExisting(manifestRoot, GetStringValue(nativeApplication, "Square310x310Logo"));
                                        appx.Square70x70Logo = ConvertRelativeLogoPathToAbsoluteExisting(manifestRoot, GetStringValue(nativeApplication, "Square70x70Logo"));
                                        appx.MinWidth = GetStringValue(nativeApplication, "MinWidth");

                                        if (PackageTypeConverter.GetPackageTypeFrom(appx.EntryPoint, appx.Executable, appx.StartPage) == MsixPackageType.BridgePsf)
                                        {
                                            appx.Psf = psfReader.Read(appx.Id, manifestRoot);
                                        }

                                        package.Applications.Add(appx);
                                    }
                                    finally
                                    {
                                        Marshal.ReleaseComObject(nativeApplication);
                                    }

                                    nativeApplications.MoveNext();
                                }

                                var nativeTargetPlatformDependencies = reader.GetTargetDeviceFamilies();
                                disposables.Push(nativeTargetPlatformDependencies);

                                while (nativeTargetPlatformDependencies.GetHasCurrent())
                                {
                                    var nativeDependency = nativeTargetPlatformDependencies.GetCurrent();
                                    try
                                    {
                                        nativeTargetPlatformDependencies.MoveNext();

                                        var minVersion = nativeDependency.GetMinVersion();
                                        var maxVersion = nativeDependency.GetMaxVersionTested();
                                        var bitConvert = BitConverter.GetBytes(minVersion);

                                        var actualMinVersion = minVersion == 0 ? null : string.Format(
                                            "{0}.{1}.{2}.{3}",
                                            BitConverter.ToUInt16(bitConvert, 6),
                                            BitConverter.ToUInt16(bitConvert, 4),
                                            BitConverter.ToUInt16(bitConvert, 2),
                                            BitConverter.ToUInt16(bitConvert, 0));

                                        bitConvert = BitConverter.GetBytes(minVersion);
                                        var actualMaxVersion = maxVersion == 0 ? null : string.Format(
                                            "{0}.{1}.{2}.{3}",
                                            BitConverter.ToUInt16(bitConvert, 6),
                                            BitConverter.ToUInt16(bitConvert, 4),
                                            BitConverter.ToUInt16(bitConvert, 2),
                                            BitConverter.ToUInt16(bitConvert, 0));

                                        package.OperatingSystemDependencies.Add(new AppxOperatingSystemDependency()
                                        {
                                            Minimum = Windows10Parser.GetOperatingSystemFromNameAndVersion(nativeDependency.GetName(), actualMinVersion),
                                            Tested = Windows10Parser.GetOperatingSystemFromNameAndVersion(nativeDependency.GetName(), actualMaxVersion),
                                        });
                                    }
                                    finally
                                    {
                                        Marshal.ReleaseComObject(nativeDependency);
                                    }
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

                                        package.PackageDependencies.Add(appxDepdendency);
                                        nativeDependencies.MoveNext();
                                    }
                                    finally
                                    {
                                        Marshal.ReleaseComObject(nativeDependency);
                                    }

                                }
                            }

                            package.BuildInfo = new BuildDetection().Detect(manifestPath);
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
                IAppxManifestReader4 CreateManifestReader(IStream inputStream);
            }

            [Guid("C43825AB-69B7-400A-9709-CC37F5A72D24"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            private interface IAppxManifestReader3 : IDisposable
            {
                IAppxManifestPackageId GetPackageId();

                IAppxManifestProperties GetProperties();

                IAppxManifestPackageDependenciesEnumerator GetPackageDependencies();

                void _VtblGap1_4(); // skip 4 methods

                IAppxManifestApplicationsEnumerator GetApplications();

                void _VtblGap1_3(); // skip 3 methods

                IAppxManifestTargetDeviceFamiliesEnumerator GetTargetDeviceFamilies();
            }

            // Note: The AppxPackaging.idl definition of this interface implements IAppxManifestReader3.
            // Therefore the functions in IAppxManifestReader, IAppxManifestReader2, and IAppxManifestReader3 should be re-declared here.
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*", Justification = "Interop")]
            [Guid("4579BB7C-741D-4161-B5A1-47BD3B78AD9B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            private interface IAppxManifestReader4 : IDisposable
            {
                // IAppxManifestReader functions
                IAppxManifestPackageId GetPackageId();

                IAppxManifestProperties GetProperties();

                IAppxManifestPackageDependenciesEnumerator GetPackageDependencies();
                void _VtblGap1_4(); // skip 4 methods

                IAppxManifestApplicationsEnumerator GetApplications();
                void _VtblGap1_3(); // skip 3 methods

                IAppxManifestTargetDeviceFamiliesEnumerator GetTargetDeviceFamilies();

                // IAppxManifestReader4 functions
                IAppxManifestOptionalPackageInfo GetOptionalPackageInfo();
            }


            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "*", Justification = "Interop")]
            [Guid("2634847D-5B5D-4FE5-A243-002FF95EDC7E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            internal interface IAppxManifestOptionalPackageInfo : IDisposable
            {
                bool GetIsOptionalPackage();

                [return: MarshalAs(UnmanagedType.LPWStr)]
                string GetMainPackageName();
            }

            [Guid("9EB8A55A-F04B-4D0D-808D-686185D4847A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            private interface IAppxManifestApplicationsEnumerator
            {
                IAppxManifestApplication GetCurrent();
                bool GetHasCurrent();
                bool MoveNext();
            }

            [Guid("36537F36-27A4-4788-88C0-733819575017"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IAppxManifestTargetDeviceFamiliesEnumerator : IDisposable
            {
                IAppxManifestTargetDeviceFamily GetCurrent();

                bool GetHasCurrent();

                bool MoveNext();
            }


            [Guid("9091B09B-C8D5-4F31-8687-A338259FAEFB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IAppxManifestTargetDeviceFamily : IDisposable
            {
                [return: MarshalAs(UnmanagedType.LPWStr)]
                string GetName();

                ulong GetMinVersion();

                ulong GetMaxVersionTested();
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
                PACKAGE_FILTER_OPTIONAL = 0x00020000,
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
}
