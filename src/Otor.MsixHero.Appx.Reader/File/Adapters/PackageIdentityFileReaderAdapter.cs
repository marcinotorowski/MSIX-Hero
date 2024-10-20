// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.IO;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel;
using Otor.MsixHero.Appx.Common.Enums;
using Windows.Management.Deployment;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.File.Entities;

namespace Otor.MsixHero.Appx.Reader.File.Adapters
{
    public class PackageIdentityFileReaderAdapter : IAppxDiskFileReader
    {
        private readonly PackageInstallationContext _context;
        private readonly string _packageFullName;
        private readonly PackageManager _packageManager;
        private readonly string _packagePublisher;
        private readonly string _packageVersion;
        private IAppxFileReader _adapter;

        public PackageIdentityFileReaderAdapter(PackageManager packageManager, PackageInstallationContext context, string packageFullName)
        {
            if (string.IsNullOrEmpty(packageFullName))
            {
                throw new ArgumentNullException(nameof(packageFullName));
            }

            _packageManager = packageManager;
            _context = context;
            _packageFullName = packageFullName;
        }

        public PackageIdentityFileReaderAdapter(PackageManager packageManager, PackageInstallationContext context, string packageName, string packagePublisher, string version) : this(packageManager, context, packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                throw new ArgumentNullException(nameof(packageName));
            }

            if (string.IsNullOrEmpty(packagePublisher))
            {
                throw new ArgumentNullException(nameof(packagePublisher));
            }

            _packageManager = packageManager;
            _packagePublisher = packagePublisher;
            _packageVersion = version;
        }

        public string RootDirectory
        {
            get
            {
                if (_adapter == null)
                {
                    _adapter = GetAdapter();
                }

                if (_adapter is IAppxDiskFileReader diskReader)
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

            if (_adapter == null)
            {
                _adapter = GetAdapter();
            }

            return _adapter.GetFile(filePath);
        }

        public IAsyncEnumerable<string> EnumerateDirectories(string rootRelativePath = null, CancellationToken cancellationToken = default)
        {
            _adapter ??= GetAdapter();
            return _adapter.EnumerateDirectories(rootRelativePath, cancellationToken);
        }

        public IAsyncEnumerable<AppxFileInfo> EnumerateFiles(string rootRelativePath, string wildcard, SearchOption searchOption = SearchOption.TopDirectoryOnly, CancellationToken cancellationToken = default)
        {
            _adapter ??= GetAdapter();
            return _adapter.EnumerateFiles(rootRelativePath, wildcard, searchOption, cancellationToken);
        }

        public IAsyncEnumerable<AppxFileInfo> EnumerateFiles(string rootRelativePath = null, CancellationToken cancellationToken = default)
        {
            _adapter ??= GetAdapter();
            return _adapter.EnumerateFiles(rootRelativePath, cancellationToken);
        }

        public Stream GetResource(string resourceFilePath)
        {
            if (string.IsNullOrEmpty(resourceFilePath))
            {
                return null;
            }

            if (_adapter == null)
            {
                _adapter = GetAdapter();
            }

            return _adapter.GetResource(resourceFilePath);
        }

        public bool DirectoryExists(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
            {
                return true;
            }

            if (_adapter == null)
            {
                _adapter = GetAdapter();
            }

            return _adapter.DirectoryExists(directoryPath);
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (_adapter == null)
            {
                _adapter = GetAdapter();
            }

            return _adapter.FileExists(filePath);
        }

        void IDisposable.Dispose()
        {
            if (_adapter != null)
            {
                _adapter.Dispose();
            }
        }

        private IAppxFileReader GetAdapter()
        {
            switch (_context)
            {
                case PackageInstallationContext.CurrentUser:
                    {
                        Package pkg = null;
                        if (string.IsNullOrEmpty(_packagePublisher))
                        {
                            pkg = _packageManager.FindPackageForUser(string.Empty, _packageFullName);
                            if (pkg == null)
                            {
                                throw new FileNotFoundException(Resources.Localization.Packages_Error_PackageNotFound);
                            }

                            return new DirectoryInfoFileReaderAdapter(pkg.InstalledLocation.Path);
                        }

                        var pkgs = _packageManager.FindPackagesForUser(string.Empty, _packageFullName, _packagePublisher).OrderBy(v => v.Id.Version);

                        if (string.IsNullOrEmpty(_packageVersion) || !Version.TryParse(_packageVersion, out Version parsedVersion))
                        {
                            pkg = pkgs.FirstOrDefault();
                            if (pkg == null)
                            {
                                throw new FileNotFoundException(Resources.Localization.Packages_Error_PackageNotFound);
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
                        if (string.IsNullOrEmpty(_packagePublisher))
                        {
                            pkg = _packageManager.FindPackage(_packageFullName);
                            if (pkg == null)
                            {
                                throw new FileNotFoundException(Resources.Localization.Packages_Error_PackageNotFound);
                            }

                            return new DirectoryInfoFileReaderAdapter(pkg.InstalledLocation.Path);
                        }

                        var installedPackages = _packageManager.FindPackages(_packageFullName, _packagePublisher).OrderBy(v => v.Id.Version);

                        if (string.IsNullOrEmpty(_packageVersion) || !Version.TryParse(_packageVersion, out Version parsedVersion))
                        {
                            pkg = installedPackages.FirstOrDefault();
                            if (pkg == null)
                            {
                                throw new FileNotFoundException(Resources.Localization.Packages_Error_PackageNotFound);
                            }

                            return new DirectoryInfoFileReaderAdapter(pkg.InstalledLocation.Path);
                        }

                        foreach (var item in installedPackages)
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