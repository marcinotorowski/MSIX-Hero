using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Dapplo.Log;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest
{
    public class SetPackagePropertiesExecutor : AppxManifestEditExecutor<SetPackageProperties>, IValueChangedExecutor
    {
        private static readonly LogSource Logger = new();
        public SetPackagePropertiesExecutor(XDocument manifest) : base(manifest)
        {
        }

        public event EventHandler<CommandValueChanged> ValueChanged;

        public override Task Execute(SetPackageProperties command, CancellationToken cancellationToken = default)
        {
            var (_, rootNamespace) = this.EnsureNamespace(Namespaces.Root);
            var identityFullName = rootNamespace + "Properties";

            // ReSharper disable once PossibleNullReferenceException
            var properties = this.Manifest.Root.Element(identityFullName);
            if (properties == null)
            {
                properties = new XElement(rootNamespace + "Properties");
                // ReSharper disable once PossibleNullReferenceException
                this.Manifest.Root.Add(properties);
            }
            
            if (command.Description != null)
            {
                var prop = properties.Element(rootNamespace + "Description");
                if (prop == null)
                {
                    Logger.Info().WriteLine($"Setting property 'Description' to '{command.Description}'...");
                    prop = new XElement("Description")
                    {
                        Value = command.Description
                    };

                    properties.Add(prop);
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("Description", command.Description));
                }
                else
                {
                    Logger.Info().WriteLine($"Changing attribute 'Description' from '{prop.Value}' to '{command.Description}'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("Description", prop.Value, command.Description));
                    prop.Value = command.Description;
                }
            }

            if (command.ModificationPackage == true)
            {
                var prop = properties.Element(rootNamespace + "ModificationPackage");
                if (prop == null)
                {
                    Logger.Info().WriteLine("Setting property 'ModificationPackage' to 'true'...");
                    prop = new XElement("ModificationPackage")
                    {
                        Value = "true"
                    };
                    properties.Add(prop);
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("ModificationPackage", "true"));
                }
                else
                {
                    Logger.Info().WriteLine($"Changing property 'ModificationPackage' from '{prop.Value}' to 'true'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("ModificationPackage", prop.Value, "true"));
                    prop.Value = "true";
                }
            }
            else if (command.ModificationPackage == false)
            {
                var prop = properties.Element(rootNamespace + "ModificationPackage");
                if (prop != null)
                {
                    Logger.Info().WriteLine("Removing property 'ModificationPackage'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("ModificationPackage", prop.Value, "false"));
                    prop.Remove();
                }
            }

            if (command.Logo != null)
            {
                var prop = properties.Element(rootNamespace + "Logo");
                if (prop == null)
                {
                    Logger.Info().WriteLine($"Setting attribute 'Logo' to '{command.Logo}'...");
                    prop = new XElement("Logo")
                    {
                        Value = command.Logo
                    };

                    this.ValueChanged?.Invoke(this, new CommandValueChanged("Logo", command.Logo));
                    properties.Add(prop);
                }
                else
                {
                    Logger.Info().WriteLine($"Changing attribute 'Logo' from '{prop.Value}' to '{command.Logo}'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("Logo", prop.Value, command.Logo));
                    prop.Value = command.Logo;
                }
            }

            if (command.DisplayName != null)
            {
                var prop = properties.Element(rootNamespace + "DisplayName");
                if (prop == null)
                {
                    Logger.Info().WriteLine($"Setting attribute 'DisplayName' to '{command.DisplayName}'...");
                    prop = new XElement("DisplayName")
                    {
                        Value = command.DisplayName
                    };

                    this.ValueChanged?.Invoke(this, new CommandValueChanged("DisplayName", command.DisplayName));
                    properties.Add(prop);
                }
                else
                {
                    Logger.Info().WriteLine($"Changing attribute 'DisplayName' from '{prop.Value}' to '{command.DisplayName}'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("DisplayName", prop.Value, command.DisplayName));
                    prop.Value = command.DisplayName;
                }
            }

            if (command.PublisherDisplayName != null)
            {
                var prop = properties.Element(rootNamespace + "PublisherDisplayName");
                if (prop == null)
                {
                    Logger.Info().WriteLine($"Setting attribute 'PublisherDisplayName' to '{command.PublisherDisplayName}'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("PublisherDisplayName", command.PublisherDisplayName));
                    prop = new XElement("PublisherDisplayName")
                    {
                        Value = command.PublisherDisplayName
                    };

                    properties.Add(prop);
                }
                else
                {
                    Logger.Info().WriteLine($"Changing attribute 'PublisherDisplayName' from '{prop.Value}' to '{command.PublisherDisplayName}'...");
                    this.ValueChanged?.Invoke(this, new CommandValueChanged("PublisherDisplayName", prop.Value, command.PublisherDisplayName));
                    prop.Value = command.PublisherDisplayName;
                }
            }

            return Task.CompletedTask;
        }
    }
}
