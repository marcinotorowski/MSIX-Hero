using System;
using System.IO;
using System.Linq;
using Windows.Management.Deployment;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders
{
    public class PackageIdentityFileReaderAdapter : IAppxDiskFileReader
    {
        private readonly PackageContext context;
        private readonly string packageFullName;
        private readonly string packagePublisher;
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

        public PackageIdentityFileReaderAdapter(PackageContext context, string packageName, string packagePublisher) : this(context, packageName)
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
                    var pkg =
                        string.IsNullOrEmpty(this.packagePublisher)
                            ? pkgManager.FindPackageForUser(string.Empty, this.packageFullName)
                            : pkgManager.FindPackagesForUser(string.Empty, this.packageFullName, this.packagePublisher)
                                .FirstOrDefault();

                    if (pkg == null)
                    {
                        throw new FileNotFoundException("Could not find the required package.");
                    }

                    return new DirectoryInfoFileReaderAdapter(pkg.InstalledLocation.Path);
                }
                default:
                {
                    var pkg =
                        string.IsNullOrEmpty(this.packagePublisher)
                            ? pkgManager.FindPackage(this.packageFullName)
                            : pkgManager.FindPackages(this.packageFullName, this.packagePublisher)
                                .FirstOrDefault();

                    if (pkg == null)
                    {
                        throw new FileNotFoundException("Could not find the required package.");
                    }

                    return new DirectoryInfoFileReaderAdapter(pkg.InstalledLocation.Path);
                }
            }
        }
    }
}