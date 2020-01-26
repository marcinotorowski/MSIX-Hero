using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Org.BouncyCastle.X509;
using otor.msixhero.lib.BusinessLayer.Appx.Detection;
using otor.msixhero.lib.Domain.Appx.Signing;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.Wrappers;
using X509Certificate = System.Security.Cryptography.X509Certificates.X509Certificate;

namespace otor.msixhero.lib.BusinessLayer.Appx.Signing
{
    public class AppxSigningManager : IAppxSigningManager
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        public Task<bool> ExtractCertificateFromMsix(
            string msixFile,
            string outputFile,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null)
        {
            return this.ExtractCertificateFromMsix(msixFile, false, outputFile, progress);
        }

        public Task<bool> ExtractCertificateFromMsix(
            string msixFile,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null)
        {
            return this.ExtractCertificateFromMsix(msixFile, true, null, progress);
        }

        public Task<PersonalCertificate> GetCertificateFromMsix(string msixFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (string.IsNullOrEmpty(msixFile))
            {
                throw new ArgumentNullException(nameof(msixFile));
            }

            return Task.Run(
                () =>
                {
                    try
                    {
                        var x509 = X509Certificate.CreateFromSignedFile(msixFile);
                        return CreateFromX509(x509, CertificateStoreType.File);
                    }
                    catch (Exception e)
                    {
                        // This will be probably WindowsCryptographicException but we do not want to expose too much...
                        Logger.Debug("Selected file {0} is not signed and no certificate could be exported from it (exception of type {1}).", msixFile, e.GetType().Name);
                        return null;
                    }
                }, 
                cancellationToken);
        }

        public async Task<List<PersonalCertificate>> GetCertificatesFromStore(CertificateStoreType certStoreType, bool onlyValid = true, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            StoreLocation loc;

            switch (certStoreType)
            {
                case CertificateStoreType.User:
                    loc = StoreLocation.CurrentUser;
                    break;
                case CertificateStoreType.Machine:
                    loc = StoreLocation.LocalMachine;
                    break;
                case CertificateStoreType.MachineUser:
                    var list1 = await this.GetCertificatesFromStore(CertificateStoreType.User, onlyValid, cancellationToken, progress).ConfigureAwait(false);
                    var list2 = await this.GetCertificatesFromStore(CertificateStoreType.Machine, onlyValid, cancellationToken, progress).ConfigureAwait(false);

                    // Remove duplicated certificates
                    for (var index = list2.Count - 1; index >= 0; index--)
                    {
                        var item = list2[index];
                        if (list1.Any(otherItem => item.Thumbprint == otherItem.Thumbprint))
                        {
                            list2.RemoveAt(index);
                        }
                    }

                    return list1.Concat(list2).ToList();

                default:
                    throw new ArgumentOutOfRangeException(nameof(certStoreType), certStoreType, null);
            }

            Logger.Info($"Getting the list of certificates from {loc} containing a private key and in a valid time range.");

            using var store = new X509Store(StoreName.My, loc);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            
            var list = new List<PersonalCertificate>();
            X509Certificate2Collection col;

            if (onlyValid)
            {
                col = store.Certificates.Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, true);
            }
            else
            {
                col = store.Certificates;
            }

            foreach (var certificate in  col)
            {
                Logger.Debug("Processing certificate {0}...", certificate);
                cancellationToken.ThrowIfCancellationRequested();
                list.Add(CreateFromX509(certificate, certStoreType));
            }

            store.Close();
            return list;
        }

        public async Task<bool> InstallCertificate(string certificateFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            try
            {
                var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "install-certificate.ps1");

                using var ps = await PowerShellSession.CreateForModule("PKI", true).ConfigureAwait(false);
                using var cmd = ps.AddCommand(scriptPath);
                using var paramCerOutputFileName = cmd.AddParameter("CerFileName", certificateFile);
                using var result = await ps.InvokeAsync(progress).ConfigureAwait(false);
                return true;
            }
            catch (COMException e)
            {
                Console.WriteLine("COM Exception " + e.HResult);
                if (e.HResult == -2146885623)
                {
                    // This is to catch COMException 0x80092009 file not found which may be thrown for invalid or missing cert files.
                    throw new Exception("Could not install certificate " + certificateFile + ". The file may be invalid, corrupted or missing. System error code: 0x" + e.HResult.ToString("X2"), e);
                }

                throw;
            }
        }

        public async Task SignPackage(
            string package, 
            bool updatePublisher, 
            PersonalCertificate certificate,
            string timestampUrl = null, 
            IncreaseVersionMethod increaseVersion = IncreaseVersionMethod.None,
            CancellationToken cancellationToken = default, 
            IProgress<ProgressData> progress = null)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }
            
            Logger.Info("Signing package {0} using personal certificate {1}.", package, certificate.Subject);
            
            StoreLocation loc;

            switch (certificate.StoreType)
            {
                case CertificateStoreType.User:
                    loc = StoreLocation.CurrentUser;
                    break;
                case CertificateStoreType.Machine:
                    loc = StoreLocation.LocalMachine;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            using var store = new X509Store(StoreName.My, loc);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            var x509 = store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, false);
            if (x509.Count < 1)
            {
                throw new ArgumentException("Certificate could not be located in the store.");
            }

            var isForCodeSigning = x509[0].Extensions.OfType<X509KeyUsageExtension>().Any(ke => ke.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature));

            if (!isForCodeSigning)
            {
                throw new ArgumentException("Selected certificate is not for code-signing.");
            }

            if (!x509[0].HasPrivateKey)
            {
                throw new ArgumentException("Selected certificate does not contain a private key.");
            }
            
            var localCopy = await this.PreparePackageForSigning(
                package, 
                updatePublisher, 
                increaseVersion,
                x509[0], 
                cancellationToken).ConfigureAwait(false);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                string type;
                if (x509[0].SignatureAlgorithm.FriendlyName.EndsWith("rsa", StringComparison.OrdinalIgnoreCase))
                {
                    type = x509[0].SignatureAlgorithm.FriendlyName.Substring(0, x509[0].SignatureAlgorithm.FriendlyName.Length - 3).ToUpperInvariant();
                }
                else
                {
                    throw new NotSupportedException($"Signature algorithm {x509[0].SignatureAlgorithm.FriendlyName} is not supported.");
                }

                Logger.Debug("Signing package {0} with algorithm {1}.", localCopy, x509[0].SignatureAlgorithm.FriendlyName);

                var sdk = new MsixSdkWrapper();
                progress?.Report(new ProgressData(25, "Signing..."));
                await sdk.SignPackageWithPersonal(localCopy, type, certificate.Thumbprint, certificate.StoreType == CertificateStoreType.Machine, timestampUrl, cancellationToken).ConfigureAwait(false);

                progress?.Report(new ProgressData(75, "Signing..."));
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);

                Logger.Debug("Moving {0} to {1}.", localCopy, package);
                File.Copy(localCopy, package, true);
                progress?.Report(new ProgressData(95, "Signing..."));
            }
            finally
            {
                try
                {
                    if (File.Exists(localCopy))
                    {
                        File.Delete(localCopy);
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Clean-up of a temporary file {0} failed.", localCopy);
                }
            }
        }

        public async Task SignPackage(
            string package, 
            bool updatePublisher, 
            string pfxPath, 
            SecureString password, 
            string timestampUrl = null,
            IncreaseVersionMethod increaseVersion = IncreaseVersionMethod.None,
            CancellationToken cancellationToken = default, 
            IProgress<ProgressData> progress = null)
        {
            Logger.Info("Signing package {0} using PFX {1}.", package, pfxPath);
            
            if (!File.Exists(pfxPath))
            {
                throw new FileNotFoundException($"File {pfxPath} does not exit.");
            }

            Logger.Debug("Analyzing given certificate...");
            var x509 = new X509Certificate2(await File.ReadAllBytesAsync(pfxPath, cancellationToken).ConfigureAwait(false), password);

            var localCopy = await this.PreparePackageForSigning(package, updatePublisher, increaseVersion, x509, cancellationToken).ConfigureAwait(false);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                string type;
                if (x509.SignatureAlgorithm.FriendlyName.EndsWith("rsa", StringComparison.OrdinalIgnoreCase))
                {
                    type = x509.SignatureAlgorithm.FriendlyName.Substring(0, x509.SignatureAlgorithm.FriendlyName.Length - 3).ToUpperInvariant();
                }
                else
                {
                    throw new NotSupportedException($"Signature algorithm {x509.SignatureAlgorithm.FriendlyName} is not supported.");
                }

                var openTextPassword = new System.Net.NetworkCredential(string.Empty, password).Password;

                Logger.Debug("Signing package {0} with algorithm {1}.", localCopy, x509.SignatureAlgorithm.FriendlyName);

                var sdk = new MsixSdkWrapper();
                progress?.Report(new ProgressData(25, "Signing..."));
                await sdk.SignPackageWithPfx(localCopy, type, pfxPath, openTextPassword, timestampUrl, cancellationToken).ConfigureAwait(false);
                progress?.Report(new ProgressData(75, "Signing..."));
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);

                Logger.Debug("Moving {0} to {1}.", localCopy, package);
                File.Copy(localCopy, package, true);
                progress?.Report(new ProgressData(95, "Signing..."));
            }
            finally
            {
                try
                {
                    if (File.Exists(localCopy))
                    {
                        File.Delete(localCopy);
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Clean-up of a temporary file {0} failed.", localCopy);
                }
            }
        }

        public Task<bool> ImportCertificateFromMsix(
            string msixFile,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null)
        {
            return this.ExtractCertificateFromMsix(msixFile, true, null, progress);
        }
        
        public async Task<bool> CreateSelfSignedCertificate(
            DirectoryInfo outputDirectory, 
            string publisherName, 
            string publisherDisplayName, 
            string password,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null)
        {
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "create-certificate.ps1");

            using var ps = await PowerShellSession.CreateForModule("PKI", true).ConfigureAwait(false);
            using var cmd = ps.AddCommand(scriptPath);
            using var paramPublisherFriendlyName = cmd.AddParameter("PublisherFriendlyName", publisherDisplayName);
            using var paramPublisherName = cmd.AddParameter("PublisherName", publisherName);
            using var paramPassword = cmd.AddParameter("Password", password);
            using var paramOutputDirectory = cmd.AddParameter("OutputDirectory", outputDirectory.FullName);
            using var paramPfxOutputFileName = cmd.AddParameter("PfxOutputFileName", null);
            using var paramCerOutputFileName = cmd.AddParameter("CerOutputFileName", null);
            using var paramCreatePasswordFile = cmd.AddParameter("CreatePasswordFile");

            using var result = await ps.InvokeAsync(progress).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> ExtractCertificateFromMsix(
            string msixFile,
            bool importToStore = false,
            string outputFile = null,
            IProgress<ProgressData> progress = null)
        {
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "extract-certificate-from-msix.ps1");

            using var ps = await PowerShellSession.CreateForModule("PKI", true).ConfigureAwait(false);
            using var cmd = ps.AddCommand(scriptPath);
            using var paramSourceMsixFile = cmd.AddParameter("SourceMsixFile", msixFile);

            if (outputFile != null)
            {
                cmd.AddParameter("CerOutputFileName", outputFile);
            }

            if (importToStore)
            {
                cmd.AddParameter("ImportToStore");
            }

            using var result = await ps.InvokeAsync(progress).ConfigureAwait(false);
            return true;
        }

        private static PersonalCertificate CreateFromX509(X509Certificate2 certificate, CertificateStoreType certStoreType)
        {
            var cert = new PersonalCertificate
            {
                DisplayName = certificate.FriendlyName,
                Date = certificate.NotAfter,
                Issuer = certificate.Issuer,
                Subject = certificate.Subject,
                Thumbprint = certificate.Thumbprint,
                DigestAlgorithm = certificate.SignatureAlgorithm.FriendlyName,
                StoreType = certStoreType
            };

            return cert;
        }

        private static PersonalCertificate CreateFromX509(X509Certificate certificate, CertificateStoreType certStoreType)
        {
            var parser = new X509CertificateParser();
            var read = parser.ReadCertificate(certificate.GetRawCertData());
            
            var cert = new PersonalCertificate
            {
                Issuer = certificate.Issuer,
                Subject = certificate.Subject,
                DigestAlgorithm = (read.SigAlgName.EndsWith("withRSA", StringComparison.OrdinalIgnoreCase) ? read.SigAlgName.Substring(0, read.SigAlgName.Length - "withRSA".Length) : read.SigAlgName).Replace("-", string.Empty),
                StoreType = certStoreType
            };

            var list = read.SubjectDN.GetValueList();
            if (list?.Count > 0)
            {
                cert.DisplayName = list[^1].ToString();
            }

            // var indexOf = cert.Subject.IndexOf("CN=", StringComparison.OrdinalIgnoreCase);
            // if (indexOf != -1)
            // {
            //     indexOf += "CN=".Length;
            //     var nextIndexOf = cert.Subject.IndexOf(',', indexOf);
               
            //     cert.DisplayName = nextIndexOf == -1 ? cert.Subject.Substring(indexOf, cert.Subject.Length - indexOf) : cert.Subject.Substring(indexOf, nextIndexOf - indexOf);
            // }
            
            return cert;
        }

        private async Task<string> PreparePackageForSigning(
            string package, 
            bool updatePublisher, 
            IncreaseVersionMethod increaseVersion,
            X509Certificate certificate, 
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(package))
            {
                throw new FileNotFoundException($"File {package} does not exit.");
            }

            var localCopy = Path.GetTempFileName() + Path.GetExtension(package);

            var sdk = new MsixSdkWrapper();
            if (updatePublisher)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Logger.Info("Updating Publisher property based on the PFX file...");

                var tempDirectory = Path.Combine(Path.GetTempPath(), "MSIX-Hero", Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant());

                try
                {
                    Logger.Debug("Unpacking {0} to {1}.", package, tempDirectory);
                    await sdk.UnpackPackage(package, tempDirectory, cancellationToken).ConfigureAwait(false);

                    var manifestFilePath = Path.Combine(tempDirectory, "AppxManifest.xml");
                    if (!File.Exists(manifestFilePath))
                    {
                        throw new FileNotFoundException($"Package {package} contains no XML manifest.");
                    }

                    string newXmlContent;

                    Logger.Debug("Opening manifest file {0}.", manifestFilePath);
                    using (var stream = File.OpenRead(manifestFilePath))
                    {
                        var xmlDocument = new XmlDocument();
                        xmlDocument.Load(stream);
                        var identity = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Identity']");

                        var publisher = identity.Attributes["Publisher"];
                        if (publisher == null)
                        {
                            publisher = xmlDocument.CreateAttribute("Publisher");
                            identity.Attributes.Append(publisher);
                        }

                        Logger.Info("Replacing Publisher '{0}' with '{1}'", publisher.InnerText, certificate.Subject);
                        publisher.InnerText = certificate.Subject;

                        if (increaseVersion != IncreaseVersionMethod.None)
                        {
                            var version = identity.Attributes["Version"];
                            if (version == null)
                            {
                                throw new FormatException("The attribute Version does not exist in the package identity element. The manifest seems to be corrupted.");
                            }
                            else
                            {
                                var content = version.InnerText;
                                if (!Version.TryParse(content, out var parsedVersion))
                                {
                                    throw new FormatException($"Version {content} is not a valid version string. The manifest seems to be corrupted.");
                                }

                                switch (increaseVersion)
                                {
                                    case IncreaseVersionMethod.Major:
                                        parsedVersion = new Version(Math.Max(0, parsedVersion.Major) + 1, Math.Max(0, parsedVersion.Minor), Math.Max(0, parsedVersion.Build), Math.Max(0, parsedVersion.Revision));
                                        break;
                                    case IncreaseVersionMethod.Minor:
                                        parsedVersion = new Version(Math.Max(0, parsedVersion.Major), Math.Max(0, parsedVersion.Minor) + 1, Math.Max(0, parsedVersion.Build), Math.Max(0, parsedVersion.Revision));
                                        break;
                                    case IncreaseVersionMethod.Build:
                                        parsedVersion = new Version(Math.Max(0, parsedVersion.Major), Math.Max(0, parsedVersion.Minor), Math.Max(0, parsedVersion.Build) + 1, Math.Max(0, parsedVersion.Revision));
                                        break;
                                    case IncreaseVersionMethod.Revision:
                                        parsedVersion = new Version(Math.Max(0, parsedVersion.Major), Math.Max(0, parsedVersion.Minor), Math.Max(0, parsedVersion.Build), Math.Max(0, parsedVersion.Revision) + 1);
                                        break;
                                }

                                Logger.Info("Replacing Version '{0}' with '{1}'", content, parsedVersion);
                                version.InnerText = parsedVersion.ToString();
                            }
                        }

                        var brandingInjector = new MsixHeroBrandingInjector();
                        brandingInjector.Inject(xmlDocument);

                        var sb = new StringBuilder();
                        using (TextWriter tw = new StringWriter(sb))
                        {
                            using (var xmlWriter = new XmlTextWriter(tw))
                            {
                                xmlWriter.Formatting = Formatting.Indented;
                                xmlDocument.WriteTo(xmlWriter);
                            }
                        }

                        newXmlContent = sb.ToString();
                    }

                    File.Delete(manifestFilePath);

                    cancellationToken.ThrowIfCancellationRequested();
                    await File.WriteAllTextAsync(manifestFilePath, newXmlContent, cancellationToken).ConfigureAwait(false);

                    if (File.Exists(localCopy))
                    {
                        File.Delete(localCopy);
                    }

                    Logger.Debug("Packing {0} to {1}.", tempDirectory, localCopy);
                    cancellationToken.ThrowIfCancellationRequested();
                    await sdk.PackPackageDirectory(tempDirectory, localCopy, true, true, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    try
                    {
                        Logger.Trace("Deleting temporary directory {0}.", tempDirectory);
                        Directory.Delete(tempDirectory, true);
                    }
                    catch (Exception e)
                    {
                        Logger.Warn(e, "Clean-up of temporary directory {0} failed.", tempDirectory);
                    }
                }
            }
            else
            {
                Logger.Debug("Copying {0} to {1}.", package, localCopy);
                File.Copy(package, localCopy, true);
            }

            return localCopy;
        }
    }
}