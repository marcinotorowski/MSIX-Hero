using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest
{
    public class SetBuildMetaDataExecutor : AppxManifestEditExecutor<SetBuildMetaData>, IValueChangedExecutor
    {
        public SetBuildMetaDataExecutor(XDocument manifest) : base(manifest)
        {
        }

        public event EventHandler<CommandValueChanged> ValueChanged;

        public override Task Execute(SetBuildMetaData command, CancellationToken cancellationToken = default)
        {
            if (this.Manifest.Root == null)
            {
                throw new InvalidOperationException(Resources.Localization.AppxEditor_Error_PathNoRegFile);
            }

            var (_, buildNamespace) = this.EnsureNamespace(Namespaces.Build);

            var metaData = this.Manifest.Root.Element(buildNamespace + "Metadata");
            if (metaData == null)
            {
                metaData = new XElement(buildNamespace + "Metadata");
                this.Manifest.Root.Add(metaData);
            }

            foreach (var value in command.Values)
            {
                var node = metaData.Elements(buildNamespace + "Item").FirstOrDefault(item => string.Equals(item.Attribute("Name")?.Value, value.Key, StringComparison.OrdinalIgnoreCase));
                if (node == null)
                {
                    node = new XElement(buildNamespace + "Item");
                    node.SetAttributeValue("Name", value.Key);
                    metaData.Add(node);

                    this.ValueChanged?.Invoke(this, new CommandValueChanged(value.Key, value.Value));
                }
                else
                {
                    var attr = node.Attribute("Version");
                    if (attr != null && command.OnlyCreateNew)
                    {
                        continue;
                    }

                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (attr != null)
                    {
                        this.ValueChanged?.Invoke(this, new CommandValueChanged(value.Key, attr.Value, value.Value));
                    }
                    else
                    {
                        this.ValueChanged?.Invoke(this, new CommandValueChanged(value.Key, value.Value));
                    }
                }

                node.SetAttributeValue("Version", value.Value);
            }

            return Task.CompletedTask;
        }
    }
}
