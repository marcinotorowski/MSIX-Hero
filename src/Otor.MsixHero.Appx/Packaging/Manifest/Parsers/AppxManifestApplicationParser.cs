using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Parsers
{
    public class AppxManifestApplicationParser
    {
        private static readonly XNamespace NamespaceWindows10 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/foundation/windows10");
        private static readonly XNamespace NamespaceAppx = XNamespace.Get("http://schemas.microsoft.com/appx/2010/manifest");
        private static readonly XNamespace NamespaceUap = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10");
        private static readonly XNamespace NamespaceUap10 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/10");
        private static readonly XNamespace NamespaceUap3 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/3");
        private static readonly XNamespace NamespaceUap5 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/5");
        private static readonly XNamespace NamespaceDesktop2 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/desktop/windows10/2");
        private static readonly XNamespace NamespaceDesktop6 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/desktop/windows10/6");
        private static readonly XNamespace NamespaceDesktop10 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/desktop/windows10/10");

        protected readonly AppxExtensionsParser ExtensionParser = new AppxExtensionsParser();

        public async IAsyncEnumerable<AppxApplication> ParseManifest(
            IAppxFileReader fileReader,
            XDocument document,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (document.Root == null)
            {
                yield break;
            }

            var nodeApplicationsRoot = document.Root.Element(NamespaceWindows10 + "Applications") ?? document.Root.Element(NamespaceAppx + "Applications");

            if (nodeApplicationsRoot == null)
            {
                yield break;
            }

            foreach (var node in nodeApplicationsRoot.Elements().Where(x => x.Name.LocalName == "Application" && (x.Name.Namespace == NamespaceWindows10 || x.Name.Namespace == NamespaceAppx)))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var appxApplication = new AppxApplication
                {
                    EntryPoint = node.Attribute("EntryPoint")?.Value,
                    StartPage = node.Attribute("StartPage")?.Value,
                    Executable = node.Attribute("Executable")?.Value,
                    Id = node.Attribute("Id")?.Value,
                    HostId = node.Attribute(NamespaceUap10 + "HostId")?.Value,
                    Parameters = node.Attribute(NamespaceUap10 + "Parameters")?.Value,
                    Extensions = new List<AppxExtension>()
                };

                await SetExtensions(appxApplication, node, cancellationToken);
                await SetVisualElements(fileReader, appxApplication, node, cancellationToken);

                yield return appxApplication;
            }
        }

        private static async Task SetVisualElements(IAppxFileReader fileReader, AppxApplication application, XContainer applicationNode, CancellationToken cancellationToken = default)
        {
            var visualElements = applicationNode.Elements().FirstOrDefault(e => e.Name.LocalName == "VisualElements");
            if (visualElements != null)
            {
                application.Description = visualElements.Attribute("Description")?.Value;
                application.DisplayName = visualElements.Attribute("DisplayName")?.Value;
                application.BackgroundColor = visualElements.Attribute("BackgroundColor")?.Value;
                application.Square150x150Logo = visualElements.Attribute("Square150x150Logo")?.Value;
                application.Square44x44Logo = visualElements.Attribute("Square44x44Logo")?.Value;
                application.Visible = visualElements.Attribute("AppListEntry")?.Value != "none";

                var defaultTile = visualElements.Element(NamespaceUap + "DefaultTile");
                if (defaultTile != null)
                {
                    application.Wide310x150Logo = defaultTile.Attribute("Wide310x150Logo")?.Value;
                    application.Square310x310Logo = defaultTile.Attribute("Square310x310Logo")?.Value;
                    application.Square71x71Logo = defaultTile.Attribute("Square71x71Logo")?.Value;
                    application.ShortName = defaultTile.Attribute("ShortName")?.Value;
                }

                var logo = application.Square44x44Logo ?? application.Square30x30Logo ?? application.Square71x71Logo ?? application.Square150x150Logo;
                if (logo != null)
                {
                    await using var stream =
                        fileReader.GetResource(application.Square44x44Logo) ??
                        fileReader.GetResource(application.Square30x30Logo) ??
                        fileReader.GetResource(application.Square71x71Logo) ??
                        fileReader.GetResource(application.Square150x150Logo);

                    if (stream != null)
                    {
                        var bytes = new byte[stream.Length];
                        await stream.ReadAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
                        application.Logo = bytes;
                    }
                }
            }
        }

        private Task SetExtensions(AppxApplication application, XContainer applicationNode, CancellationToken cancellationToken = default)
        {
            if (applicationNode == null)
            {
                return Task.CompletedTask;
            }

            application.Extensions ??= new List<AppxExtension>();

            var extensionsNode = applicationNode.Elements().FirstOrDefault(e => e.Name.LocalName == "Extensions");

            foreach (var extension in this.ExtensionParser.ParseManifest(extensionsNode))
            {
                application.Extensions.Add(extension);
            }

            foreach (var extension in extensionsNode?
                .Elements()
                .Where(e =>
                    e.Name.LocalName == "Extension" &&
                    (
                        e.Name.Namespace == NamespaceWindows10 ||
                        e.Name.Namespace == NamespaceAppx ||
                        e.Name.Namespace == NamespaceDesktop6 ||
                        e.Name.Namespace == NamespaceDesktop2 ||
                        e.Name.Namespace == NamespaceUap10 ||
                        e.Name.Namespace == NamespaceUap5 ||
                        e.Name.Namespace == NamespaceUap3)) ?? Enumerable.Empty<XElement>())
            {
                var category = extension.Attribute("Category")?.Value;
                if (category == "windows.appExecutionAlias")
                {
                    var aliasNode = extension.Element(NamespaceUap3 + "AppExecutionAlias") ?? extension.Element(NamespaceUap5 + "AppExecutionAlias");
                    if (aliasNode != null)
                    {
                        var desktopExecAliases = aliasNode.Elements().Where(e => e.Name.LocalName == "ExecutionAlias");
                        application.ExecutionAlias = desktopExecAliases.Select(a => a.Attribute("Alias")?.Value).Where(a => a != null).ToList();
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
