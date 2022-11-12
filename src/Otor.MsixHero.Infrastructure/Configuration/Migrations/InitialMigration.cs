using System.Collections.Generic;
using System.Reflection;

namespace Otor.MsixHero.Infrastructure.Configuration.Migrations
{
    internal class InitialMigration : ConfigurationMigration
    {
        public InitialMigration(Configuration config) : base(config)
        {
        }

        public override int? TargetRevision => null;

        protected override void DoMigrate()
        {
            if (this.Configuration.AppAttach == null)
            {
                this.Configuration.AppAttach = new AppAttachConfiguration();
            }

            if (this.Configuration.Signing == null)
            {
                this.Configuration.Signing = new SigningConfiguration();
            }

            if (this.Configuration.UiConfiguration == null)
            {
                this.Configuration.UiConfiguration = new UiConfiguration();
            }
            
            if (this.Configuration.Packer == null)
            {
                this.Configuration.Packer = new PackerConfiguration();
            }

            if (this.Configuration.Events == null)
            {
                this.Configuration.Events = new EventsConfiguration();
            }
            
            if (this.Configuration.Packages == null)
            {
                this.Configuration.Packages = new PackagesConfiguration();
            }
            
            if (this.Configuration.Signing == null)
            {
                this.Configuration.Signing = new SigningConfiguration();
            }

            if (this.Configuration.Signing.Profiles == null)
            {
                this.Configuration.Signing.Profiles = new List<SigningProfile>();
            }

            if (this.Configuration.Packages.Tools == null)
            {
                this.Configuration.Packages.Tools = new List<ToolListConfiguration>
                {
                    new ToolListConfiguration { Name = "Registry editor", Path = "regedit.exe", AsAdmin = true },
                    new ToolListConfiguration { Name = "Notepad", Path = "notepad.exe" },
                    new ToolListConfiguration { Name = "Command Prompt", Path = "cmd.exe" },
                    new ToolListConfiguration { Name = "PowerShell Console", Path = "powershell.exe" }
                };
            }

            if (this.Configuration.Packages.StarredApps == null)
            {
                this.Configuration.Packages.StarredApps = new List<string>
                {
                    "48548MarcinOtorowski.MSIXHero_0.0.0.0_Neutral__0ctrt0jrcjnrt"
                };
            }
            
            if (this.Configuration.AppInstaller == null)
            {
                this.Configuration.AppInstaller = new AppInstallerConfiguration();
            }
            
            if (this.Configuration.Advanced == null)
            {
                this.Configuration.Advanced = new AdvancedConfiguration();
            }

            if (this.Configuration.Update == null)
            {
                this.Configuration.Update = new UpdateConfiguration
                {
                    HideNewVersionInfo = false,
                    // ReSharper disable once PossibleNullReferenceException
                    LastShownVersion = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version.ToString(3)
                };
            }
        }
    }
}
