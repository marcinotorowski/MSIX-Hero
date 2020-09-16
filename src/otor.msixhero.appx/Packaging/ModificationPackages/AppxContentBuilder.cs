﻿using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Otor.MsixHero.Appx.Packaging.ModificationPackages.Entities;
using Otor.MsixHero.Appx.Packaging.Packer;
using Otor.MsixHero.Infrastructure.Branding;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Packaging.ModificationPackages
{
    public class AppxContentBuilder : IAppxContentBuilder
    {
        private static readonly ILog Logger = LogManager.GetLogger();
        private const string DefaultNamespacePrefix = "msixHero";

        private readonly IAppxPacker packer;

        public AppxContentBuilder(IAppxPacker packer)
        {
            this.packer = packer;
        }

        public async Task Create(ModificationPackageConfig config, string filePath, ModificationPackageBuilderAction action, CancellationToken cancellation = default, IProgress<ProgressData> progress = default)
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            var modPackageTemplate = GetBundledResourcePath("ModificationPackage.AppxManifest.xml");
            var logoPath = GetBundledResourcePath("Logo.png");
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(modPackageTemplate);
            this.PrepareModificationPackage(xmlDoc, config);

            string manifestContent;
            {
                var sb = new StringBuilder();
                using (TextWriter tw = new StringWriter(sb))
                {
                    using (var xmlWriter = new XmlTextWriter(tw))
                    {
                        xmlWriter.Formatting = Formatting.Indented;
                        xmlDoc.WriteTo(xmlWriter);
                    }
                }

                manifestContent = sb.ToString();
            }

            if (action == ModificationPackageBuilderAction.Manifest)
            {
                using (var sourceStream = File.OpenRead(logoPath))
                {
                    var assetsDirectory = Path.Combine(Path.GetDirectoryName(filePath), "Assets");
                    if (!Directory.Exists(assetsDirectory))
                    {
                        Directory.CreateDirectory(assetsDirectory);
                    }
                    
                    using (var targetStream = File.OpenWrite(Path.Combine(assetsDirectory, "Logo.png")))
                    {
                        await sourceStream.CopyToAsync(targetStream, cancellation).ConfigureAwait(false);
                    }
                }

                await File.WriteAllTextAsync(filePath, manifestContent, Encoding.UTF8, cancellation).ConfigureAwait(false);
                return;
            }

            var tempFolder = Environment.ExpandEnvironmentVariables(@"%temp%\msix-hero-" + Guid.NewGuid().ToString("N").Substring(10));
            try
            {   
                using (var sourceStream = File.OpenRead(logoPath))
                {
                    var assetsFolder = Path.Combine(tempFolder, "Assets");
                    if (!Directory.Exists(assetsFolder))
                    {
                        Directory.CreateDirectory(assetsFolder);
                    }

                    using (var targetStream = File.OpenWrite(Path.Combine(assetsFolder, "Logo.png")))
                    {
                        await sourceStream.CopyToAsync(targetStream, cancellation).ConfigureAwait(false);
                    }
                }

                await File.WriteAllTextAsync(Path.Join(tempFolder, "AppxManifest.xml"), manifestContent, Encoding.UTF8, cancellation).ConfigureAwait(false);
                await this.packer.Pack(tempFolder, filePath, 0, cancellation, progress).ConfigureAwait(false);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempFolder))
                    {
                        Directory.Delete(tempFolder, true);
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Clean-up failed.");
                }
            }

            switch (action)
            {
                case ModificationPackageBuilderAction.Manifest:
                case ModificationPackageBuilderAction.Msix:
                    break;
                case ModificationPackageBuilderAction.SignedMsix:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private void PrepareModificationPackage(XmlDocument template, ModificationPackageConfig config)
        {
            var namespaceManager = new XmlNamespaceManager(template.NameTable);
            if (!namespaceManager.HasNamespace("uap4"))
            {
                namespaceManager.AddNamespace("uap4", "http://schemas.microsoft.com/appx/manifest/uap/windows10/4");
            }

            if (!namespaceManager.HasNamespace("build"))
            {
                namespaceManager.AddNamespace("build", "http://schemas.microsoft.com/developer/appx/2015/build");
            }

            namespaceManager.AddNamespace(DefaultNamespacePrefix, template.DocumentElement.NamespaceURI);

            var package = GetOrCreateNode(template, template, "Package", DefaultNamespacePrefix, namespaceManager);
            var dependencies = GetOrCreateNode(template, package, "Dependencies", DefaultNamespacePrefix, namespaceManager);
            var dependency = template.CreateElement("uap4", "MainPackageDependency", "http://schemas.microsoft.com/appx/manifest/uap/windows10/4");
            dependencies.AppendChild(dependency);
            dependency.SetAttribute("Name", config.ParentName);
            dependency.SetAttribute("Publisher", config.ParentPublisher);

            var identity = GetOrCreateNode(template, package, "Identity", DefaultNamespacePrefix, namespaceManager);
            identity.SetAttribute("Name", config.Name);
            identity.SetAttribute("ProcessorArchitecture", config.Architecture.ToString());
            identity.SetAttribute("Publisher", config.Publisher);

            var fixVersion = Version.Parse(config.Version);

            var major = fixVersion.Major;
            var minor = fixVersion.Minor;
            var build = fixVersion.Build;
            var revision = fixVersion.Revision;
            
            if (major < 0)
            {
                throw new FormatException("Invalid version format, major version is required.");
            }

            if (minor < 0)
            {
                throw new FormatException("Invalid version format, major version is required.");
            }

            if (revision < 0)
            {
                revision = 0;
            }

            if (build < 0)
            {
                build = 0;
            }

            identity.SetAttribute("Version", new Version(major, minor, build, revision).ToString(4));

            var properties = GetOrCreateNode(template, package, "Properties", DefaultNamespacePrefix, namespaceManager);
            GetOrCreateNode(template, properties, "DisplayName", DefaultNamespacePrefix, namespaceManager).InnerText = config.DisplayName ?? "Modification Package Name";
            GetOrCreateNode(template, properties, "PublisherDisplayName", DefaultNamespacePrefix, namespaceManager).InnerText = config.DisplayPublisher ?? "Modification Package Publisher Name";
            GetOrCreateNode(template, properties, "Description", DefaultNamespacePrefix, namespaceManager).InnerText = "Modification Package for " + config.ParentName;
            GetOrCreateNode(template, properties, "Logo", DefaultNamespacePrefix, namespaceManager).InnerText = "Assets\\Logo.png";

            var branding = new MsixHeroBrandingInjector();
            branding.Inject(template);
        }

        private static XmlElement GetOrCreateNode(XmlDocument xmlDocument, XmlNode xmlNode, string name, string prefix, XmlNamespaceManager namespaceManager)
        {
            var node = xmlNode.SelectSingleNode($"{prefix}:{name}", namespaceManager);
            // ReSharper disable once InvertIf
            if (node == null)
            {
                return CreateNode(xmlDocument, xmlNode, name, prefix, namespaceManager);
            }

            return (XmlElement)node;
        }

        private static XmlElement CreateNode(XmlDocument xmlDocument, XmlNode xmlNode, string name, string prefix, XmlNamespaceManager namespaceManager)
        {
            var ns = namespaceManager.LookupNamespace(prefix);
            if (string.IsNullOrEmpty(ns))
            {
                ns = xmlDocument.DocumentElement.NamespaceURI;
            }

            if (ns != xmlDocument.DocumentElement.NamespaceURI && !string.IsNullOrEmpty(prefix))
            {
                name = $"{prefix}:{name}";
            }

            var node = xmlDocument.CreateElement(name, ns);
            xmlNode.AppendChild(node);
            return node;
        }

        private static string GetBundledResourcePath(string localName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates", localName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not locale resource {path}.", path);
            }

            return path;
        }
    }
}