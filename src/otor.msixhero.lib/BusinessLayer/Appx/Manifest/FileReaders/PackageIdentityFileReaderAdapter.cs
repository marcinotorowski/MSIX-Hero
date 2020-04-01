﻿using System;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders
{
    public class PackageIdentityFileReaderAdapter : IAppxDiskFileReader
    {
        private readonly PackageContext context;
        private readonly string packageFullName;
        private readonly string packagePublisher;
        private readonly string packageVersion;
        private IAppxFileReader adapter;

        public PackageIdentityFileReaderAdapter(PackageContext context, string packageFullName)
        {
            if (string.IsNullOrEmpty(packageFullName))
            {
                throw new ArgumentNullException(nameof(packageFullName));
            }

            this.context = context;
            this.packageFullName = packageFullName;
        }

        public PackageIdentityFileReaderAdapter(PackageContext context, string packageName, string packagePublisher, string version) : this(context, packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                throw new ArgumentNullException(nameof(packageName));
            }

            if (string.IsNullOrEmpty(packagePublisher))
            {
                throw new ArgumentNullException(nameof(packagePublisher));
            }

            this.packagePublisher = packagePublisher;
            this.packageVersion = version;
        }

        public string RootDirectory
        {
            get
            {
                if (this.adapter == null)
                {
                    this.adapter = GetAdapter();
                }

                if (this.adapter is IAppxDiskFileReader diskReader)
                {
                    return diskReader.RootDirectory;
                }

                return null;
            }
        }

        public Stream GetFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            if (this.adapter == null)
            {
                this.adapter = this.GetAdapter();
            }

            return this.adapter.GetFile(filePath);
        }

        public Stream GetResource(string resourceFilePath)
        {
            if (string.IsNullOrEmpty(resourceFilePath))
            {
                return null;
            }

            if (this.adapter == null)
            {
                this.adapter = this.GetAdapter();
            }

            return this.adapter.GetResource(resourceFilePath);
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (this.adapter == null)
            {
                this.adapter = this.GetAdapter();
            }

            return this.adapter.FileExists(filePath);
        }

        void IDisposable.Dispose()
        {
            if (this.adapter != null)
            {
                this.adapter.Dispose();
            }
        }

        private IAppxFileReader GetAdapter()
        {
            var pkgManager = new PackageManager();

            switch (this.context)
            {
                case PackageContext.CurrentUser:
                {
                    Package pkg = null;
                    if (string.IsNullOrEmpty(this.packagePublisher))
                    {
                        pkg = pkgManager.FindPackageForUser(string.Empty, this.packageFullName);
                        if (pkg == null)
                        {
                            throw new FileNotFoundException("Could not find the required package.");
                        }

                        return new DirectoryInfoFileReaderAdapter(pkg.InstalledLocation.Path);
                    }

                    var pkgs = pkgManager.FindPackagesForUser(string.Empty, this.packageFullName, this.packagePublisher).OrderBy(v => v.Id.Version);

                    if (string.IsNullOrEmpty(this.packageVersion) || !Version.TryParse(this.packageVersion, out Version parsedVersion))
                    {
                        pkg = pkgs.FirstOrDefault();
                        if (pkg == null)
                        {
                            throw new FileNotFoundException("Could not find the required package.");
                        }

                        return new DirectoryInfoFileReaderAdapter(pkg.InstalledLocation.Path);
                    }
                    
                    foreach (var item in pkgs)
                    {
                        pkg = item;
                        var compareAgainst = new Version(item.Id.Version.Major, item.Id.Version.Minor, item.Id.Version.Build, item.Id.Version.Revision);

                        if (compareAgainst >= parsedVersion)
                        {
                            return new DirectoryInfoFileReaderAdapter(item.InstalledLocation.Path);
                        }
                    }

                    if (pkg == null)
                    {
                        return null;
                    }

                    return new DirectoryInfoFileReaderAdapter(pkg.InstalledLocation.Path);
                }
                default:
                {
                    Package pkg = null;
                    if (string.IsNullOrEmpty(this.packagePublisher))
                    {
                        pkg = pkgManager.FindPackage(this.packageFullName);
                        if (pkg == null)
                        {
                            throw new FileNotFoundException("Could not find the required package.");
                        }

                        return new DirectoryInfoFileReaderAdapter(pkg.InstalledLocation.Path);
                    }

                    var pkgs = pkgManager.FindPackages(this.packageFullName, this.packagePublisher).OrderBy(v => v.Id.Version);

                    if (string.IsNullOrEmpty(this.packageVersion) || !Version.TryParse(this.packageVersion, out Version parsedVersion))
                    {
                        pkg = pkgs.FirstOrDefault();
                        if (pkg == null)
                        {
                            throw new FileNotFoundException("Could not find the required package.");
                        }

                        return new DirectoryInfoFileReaderAdapter(pkg.InstalledLocation.Path);
                    }

                    foreach (var item in pkgs)
                    {
                        pkg = item;
                        var compareAgainst = new Version(item.Id.Version.Major, item.Id.Version.Minor, item.Id.Version.Build, item.Id.Version.Revision);

                        if (compareAgainst >= parsedVersion)
                        {
                            return new DirectoryInfoFileReaderAdapter(item.InstalledLocation.Path);
                        }
                    }

                    if (pkg == null)
                    {
                        return null;
                    }

                    return new DirectoryInfoFileReaderAdapter(pkg.InstalledLocation.Path);
                }
            }
        }
    }
}