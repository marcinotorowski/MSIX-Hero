using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Windows.Management.Deployment;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using otor.msixhero.lib;
using Process = System.Diagnostics.Process;

namespace otor.msihero.lib
{
    public enum PackageFindMode
    {
        Auto,
        CurrentUser,
        AllUsers
    }

    public class AppxPackageManager
    {
        public void RunTool(Package package, string toolName)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (toolName == null)
            {
                throw new ArgumentNullException(nameof(toolName));
            }
            
            using var ps = PowerShell.Create();
            ps.AddCommand("Set-ExecutionPolicy");
            ps.AddParameter("ExecutionPolicy", "ByPass");
            ps.AddParameter("Scope", "Process");
            ps.Invoke();
            ps.Commands.Clear();

            ps.AddCommand("Import-Module");
            ps.AddParameter("Name", "Appx");
            ps.Invoke();
            ps.Commands.Clear();

            ps.AddCommand("Invoke-CommandInDesktopPackage");
            ps.AddParameter("Command", toolName);
            ps.AddParameter("PackageFamilyName", package.PackageFamilyName);
            ps.AddParameter("AppId", package.Name);
            ps.AddParameter("PreventBreakaway");

            ps.Invoke();
        }

        public void RunApp(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (package.ManifestLocation == null || !System.IO.File.Exists(package.ManifestLocation))
            {
                throw new FileNotFoundException();
            }

            var entryPoint = GetEntryPoints(package).FirstOrDefault();
            if (entryPoint == null)
            {
                throw new InvalidOperationException("This package has no entry points.");
            }

            var p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true, 
                FileName = entryPoint
            };

            p.StartInfo = startInfo;
            p.Start();
        }

        private static int processPid = 0;

        private IEnumerable<Package> GetPackagesAdmin()
        {
            var process = processPid == 0 ? null : Process.GetProcessesByName("otor.msixhero.adminhelper").FirstOrDefault(p => p.Id == processPid);
            if (process == null)
            {
                var psi = new ProcessStartInfo(string.Join(AppDomain.CurrentDomain.BaseDirectory, "otor.msixhero.adminhelper" + ".exe"), "--pipe " + Process.GetCurrentProcess().Id);
                psi.Verb = "runas";
                psi.UseShellExecute = true;
                psi.WindowStyle = ProcessWindowStyle.Normal;

                var p = Process.Start(psi);
                System.Threading.Thread.Sleep(400);
                processPid = p.Id;
            }

            // using var pipeClient = new NamedPipeClientStream("msixhero-" + Process.GetCurrentProcess().Id);
            // pipeClient.Connect(1000);
            // var stream = pipeClient;

            using var tcpClient = new TcpClient();
            tcpClient.Connect("localhost", 45678);
            using var stream = tcpClient.GetStream();

            using var binaryWriter = new BinaryWriter(stream);
            using var binaryReader = new BinaryReader(stream);

            binaryWriter.Write("listAllUserPackages");
            var result = binaryReader.ReadBoolean();
            if (!result)
            {
                var exceptionName = binaryReader.ReadString();
                var stackTrace = binaryReader.ReadString();
                throw new InvalidOperationException(exceptionName + "\r\n" + stackTrace);
            }

            var xmlSerializer = new XmlSerializer(typeof(List<Package>));
            var byteLength = binaryReader.ReadInt32();

            var byteArray = binaryReader.ReadBytes(byteLength);
            using var memoryStream = new MemoryStream(byteArray);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var str = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());

            memoryStream.Seek(0, SeekOrigin.Begin);
            var deserialized = (List<Package>)xmlSerializer.Deserialize(memoryStream);
            return deserialized;
        }

        public IEnumerable<Package> GetPackages(PackageFindMode mode = PackageFindMode.Auto, bool elevateIfRequired = true)
        {
            if (mode == PackageFindMode.Auto)
            {
                mode = UserHelper.IsAdministrator() ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser;
            }

            if (elevateIfRequired && mode == PackageFindMode.AllUsers && !UserHelper.IsAdministrator())
            {
                foreach (var item in this.GetPackagesAdmin())
                {
                    yield return item;
                }
            }
            else
            {
                var pkgMan = new PackageManager();
                IEnumerable<Windows.ApplicationModel.Package> allPackages;

                switch (mode)
                {
                    case PackageFindMode.CurrentUser:
                        allPackages = pkgMan.FindPackagesForUser(string.Empty);
                        break;
                    case PackageFindMode.AllUsers:
                        allPackages = pkgMan.FindPackages();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }

                foreach (var item in allPackages)
                {
                    string installLocation = null;
                    try
                    {
                        installLocation = item.InstalledLocation?.Path;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    var details = GetManifestDetails(installLocation);

                    yield return new Package()
                    {
                        DisplayName = details.DisplayName,
                        Name = item.Id.Name,
                        Image = details.Logo,
                        ProductId = item.Id.FullName,
                        InstallLocation = installLocation,
                        PackageFamilyName = item.Id.FamilyName,
                        Description = details.Description,
                        DisplayPublisherName = details.DisplayPublisherName,
                        Publisher = item.Id.Publisher,
                        Version = new Version(item.Id.Version.Major, item.Id.Version.Minor, item.Id.Version.Build, item.Id.Version.Revision),
                        SignatureKind = Convert(item.SignatureKind)
                    };
                }
            }
        }

        private static IEnumerable<string> GetEntryPoints(Package package)
        {
            if (!System.IO.File.Exists(package.ManifestLocation))
            {
                yield break;
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(package.ManifestLocation);
            var nodes = xmlDocument.SelectNodes("/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']");

            foreach (var node in nodes.OfType<XmlNode>())
            {
                var attrVal = node.Attributes["Id"]?.Value;
                if (string.IsNullOrEmpty(attrVal))
                {
                    attrVal = string.Empty;
                }
                else
                {
                    attrVal = "!" + attrVal;
                }

                yield return @"shell:appsFolder\" + package.PackageFamilyName + attrVal;
            }
        }

        private static PkgDetails GetManifestDetails(string installLocation)
        {
            string img = null;
            string name = null;
            string publisher = null;
            string description = null;

            if (installLocation == null)
            {
                return new PkgDetails();
            }
                       
            var appxManifest = System.IO.Path.Combine(installLocation, "AppxManifest.xml");
            if (!System.IO.File.Exists(appxManifest))
            {
                return new PkgDetails();
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(appxManifest);

            var node = xmlDocument.SelectSingleNode("/*[local-name()='Package']/*[local-name()='Properties']");

            foreach (XmlNode subNode in node.ChildNodes)
            {
                switch (subNode.LocalName)
                {
                    case "DisplayName":
                        name = subNode.InnerText;
                        break;
                    case "Logo":

                        img = System.IO.Path.Combine(installLocation, subNode.InnerText);
                        if (!System.IO.File.Exists(img))
                        {
                            var extension = System.IO.Path.GetExtension(img);
                            var baseName = System.IO.Path.GetFileNameWithoutExtension(img);
                            var baseFolder = System.IO.Path.GetDirectoryName(img);

                            img = null;

                            var dirInfo = new DirectoryInfo(Path.Combine(installLocation, baseFolder));
                            if (dirInfo.Exists)
                            {
                                var found = dirInfo.EnumerateFiles(baseName + "*" + extension).FirstOrDefault();
                                if (found != null)
                                {
                                    img = found.FullName;
                                }
                            }
                        }

                        break;
                    case "PublisherDisplayName":
                        publisher = subNode.InnerText;
                        break;
                    case "Description":
                        description = subNode.InnerText;
                        break;
                }
            }
            
            return new PkgDetails(name, publisher, img, description);
        }

        private static SignatureKind Convert(Windows.ApplicationModel.PackageSignatureKind signatureKind)
        {
            switch (signatureKind)
            {
                case Windows.ApplicationModel.PackageSignatureKind.None:
                    return SignatureKind.None;
                case Windows.ApplicationModel.PackageSignatureKind.Developer:
                    return SignatureKind.Developer;
                case Windows.ApplicationModel.PackageSignatureKind.Enterprise:
                    return SignatureKind.Enterprise;
                case Windows.ApplicationModel.PackageSignatureKind.Store:
                    return SignatureKind.Store;
                case Windows.ApplicationModel.PackageSignatureKind.System:
                    return SignatureKind.System;
                default:
                    throw new NotSupportedException();
            }
        }

        private struct PkgDetails
        {
            public PkgDetails(string displayName, string displayPublisherName, string logo, string description)
            {
                DisplayName = displayName;
                DisplayPublisherName = displayPublisherName;
                Logo = logo;
                Description = description;
            }

            public string Description;
            public string DisplayName;
            public string DisplayPublisherName;
            public string Logo;
        }
    }
}
