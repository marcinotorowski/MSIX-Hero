using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest
{
    public class SetPackageIdentityExecutor : AppxManifestEditExecutor<SetPackageIdentity>, IValueChangedExecutor
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SetPackageIdentityExecutor));

        public SetPackageIdentityExecutor(XDocument manifest) : base(manifest)
        {
        }

        public event EventHandler<CommandValueChanged> ValueChanged;

        public override Task Execute(SetPackageIdentity command, CancellationToken cancellationToken = default)
        {
            var (_, rootNamespace) = this.EnsureNamespace(Namespaces.Root);
            var identityFullName = rootNamespace + "Identity";

            
            // ReSharper disable once PossibleNullReferenceException
            var identity = this.Manifest.Root.Element(identityFullName);
            if (identity == null)
            {
                identity = new XElement(rootNamespace + "Identity");
                // ReSharper disable once PossibleNullReferenceException
                this.Manifest.Root.Add(identity);
            }

            if (command.Publisher != null)
            {
                var attr = identity.Attribute("Publisher");
                if (attr == null)
                {
                    Logger.Info($"Setting attribute 'Publisher' to '{command.Publisher}'...");
                    attr = new XAttribute("Publisher", command.Publisher);
                    identity.Add(attr);
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("Publisher", command.Publisher));
                }
                else
                {
                    Logger.Info($"Changing attribute 'Publisher' from '{attr.Value}' to '{command.Publisher}'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("Publisher", attr.Value, command.Publisher));
                    attr.Value = command.Publisher;
                }
            }

            if (command.Name != null)
            {
                var attr = identity.Attribute("Name");
                if (attr == null)
                {
                    Logger.Info($"Setting attribute 'Name' to '{command.Name}'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("Name", command.Name));
                    attr = new XAttribute("Name", command.Name);
                    identity.Add(attr);
                }
                else
                {
                    Logger.Info($"Changing attribute 'Name' from '{attr.Value}' to '{command.Name}'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("Publisher", attr.Value, command.Publisher));
                    attr.Value = command.Name;
                }
            }

            if (command.Version != null)
            {
                var attr = identity.Attribute("Version");
                if (attr == null)
                {
                    var newVersion = VersionStringOperations.ResolveMaskedVersion(command.Version);
                    Logger.Info($"Setting attribute 'Version' to '{newVersion}'...");
                    attr = new XAttribute("Version", newVersion);
                    identity.Add(attr);
                }
                else
                {
                    var newVersion = VersionStringOperations.ResolveMaskedVersion(command.Version, attr.Value);

                    Logger.Info($"Changing attribute 'Version' from '{attr.Value}' to '{newVersion}'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("Version", attr.Value, newVersion));
                    attr.Value = newVersion;
                }
            }

            if (command.ProcessorArchitecture != null)
            {
                var attr = identity.Attribute("ProcessorArchitecture");
                if (attr == null)
                {
                    Logger.Info($"Setting attribute 'ProcessorArchitecture' to '{command.ProcessorArchitecture}'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("ProcessorArchitecture", command.ProcessorArchitecture));
                    attr = new XAttribute("ProcessorArchitecture", command.ProcessorArchitecture);
                    identity.Add(attr);
                }
                else
                {
                    Logger.Info($"Changing attribute 'ProcessorArchitecture' from '{attr.Value}' to '{command.ProcessorArchitecture}'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("ProcessorArchitecture", attr.Value, command.ProcessorArchitecture));
                    attr.Value = command.ProcessorArchitecture;
                }
            }

            return Task.CompletedTask;
        }

    }
}
