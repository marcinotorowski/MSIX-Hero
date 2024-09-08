using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Editor;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Interop;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.Names.ViewModel
{
    public class NamesViewModel : ChangeableDialogViewModel, IDataErrorInfo, IDialogAware
    {
        private readonly IInteractionService _interactionService;
        private string _name;
        private string _publisherHash;
        private string _publisher;
        private string _version;
        private string _resource;
        private string _fullName;
        private string _familyName;
        private AppxPackageArchitecture _architecture;

        private readonly Func<string, string> _validatePublisher = AppxValidatorFactory.ValidateSubject();
        private readonly Func<string, string> _validateVersion = AppxValidatorFactory.ValidateVersion();
        private readonly Func<string, string> _validatePackageName = AppxValidatorFactory.ValidatePackageName();
        private readonly Func<string, string> _validateResourceId = AppxValidatorFactory.ValidateResourceId(false);
        private ICommand _openCommand, _copyCommand;

        public NamesViewModel(IInteractionService interactionService) : base(Resources.Localization.Dialogs_PackageName_Title, interactionService)
        {
            this._interactionService = interactionService;
        }

        public ICommand OpenCommand => this._openCommand ??= new ActionCommand(this.OnOpenCommand);
        public ICommand CopyCommand => this._copyCommand ??= new DelegateCommand<PackageProperty?>(this.OnCopyCommand);

        public new string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Publisher):
                        return this._validatePublisher(this._publisher);
                    case nameof(Version):
                        return this._validateVersion(this._version);
                    case nameof(Name):
                        return this._validatePackageName(this._name);
                    case nameof(Resource):
                        return this._validateResourceId(this._resource);
                    default:
                        return null;
                }
            }
        }
        
        public string Publisher
        {
            get => this._publisher;
            set
            {
                this.SetField(ref this._publisher, value);
                this.SetCalculatedProperties();
            }
        }

        public string PublisherHash => this._publisherHash;

        public AppxPackageArchitecture Architecture
        {
            get => this._architecture;
            set
            {
                this.SetField(ref this._architecture, value);
                this.SetCalculatedProperties();
            }
        }

        public string FullName => this._fullName;

        public string FamilyName => this._familyName;

        public string Version
        {
            get => this._version;
            set
            {
                this.SetField(ref this._version, value);
                this.SetCalculatedProperties();
            }
        }

        public string Name
        {
            get => this._name;
            set
            {
                this.SetField(ref this._name, value);
                this.SetCalculatedProperties();
            }
        }

        public string Resource
        {
            get => this._resource;
            set
            {
                this.SetField(ref this._resource, value);
                this.SetCalculatedProperties();
            }
        }

        void IDialogAware.OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.Count == 0)
            {
                this._name = Resources.Localization.Dialogs_PackageName_DefaultName;
                this._version = "1.0.0.0";
                this._architecture = AppxPackageArchitecture.x64;
                this._publisher = Resources.Localization.Dialogs_PackageName_DefaultPublisher;
                this._resource = "";
                this.SetCalculatedProperties();
            }
            else 
            {
                if (parameters.Keys.Count() == 1 && File.Exists(parameters.Keys.First()))
                {
#pragma warning disable CS4014
                    this.LoadFile(parameters.GetValue<string>(parameters.Keys.First()));
#pragma warning restore CS4014
                }
                else
                {
                    if (parameters.TryGetValue("version", out string v))
                    {
                        this._version = v;
                    }

                    if (parameters.TryGetValue("name", out string n))
                    {
                        this._name = n;
                    }

                    if (parameters.TryGetValue("publisher", out string p))
                    {
                        this._publisher = p;
                    }

                    if (parameters.TryGetValue("resource", out string r))
                    {
                        this._resource = r;
                    }

                    if (parameters.TryGetValue("architecture", out string a))
                    {
                        this._architecture = Enum.TryParse(a, out AppxPackageArchitecture parsed) ? parsed : AppxPackageArchitecture.Neutral;
                    }

                    this.SetCalculatedProperties();
                }
            }
        }

        protected override Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            return Task.FromResult(true);
        }

        private void SetCalculatedProperties()
        {
            if (this[nameof(Publisher)] != null)
            {
                this._fullName = Resources.Localization.Dialogs_PackageName_InvalidPublisherName;
                this._publisherHash = Resources.Localization.Dialogs_PackageName_InvalidPublisherName;
                this._familyName = Resources.Localization.Dialogs_PackageName_InvalidPublisherName;
            }
            else if (this[nameof(Name)] != null)
            {
                this._fullName = Resources.Localization.Dialogs_PackageName_InvalidPackageName;
                this._familyName = Resources.Localization.Dialogs_PackageName_InvalidPackageName;
            }
            else if (this[nameof(Version)] != null)
            {
                this._fullName = Resources.Localization.Dialogs_PackageName_InvalidPackageVersion;
            }
            else if (this[nameof(Resource)] != null)
            {
                this._fullName = Resources.Localization.Dialogs_PackageName_InvalidResourceId;
            }
            else
            {
                this._familyName = AppxPackaging.GetPackageFamilyName(this._name, this._publisher);
                this._publisherHash = this._familyName.Split('_').Last();
                this._fullName = AppxPackaging.GetPackageFullName(this._name, this._publisher, this._architecture, this._version, this._resource);
            }

            this.OnPropertyChanged(null);
        }
        
        private void OnCopyCommand(PackageProperty? parameter)
        {
            switch (parameter.GetValueOrDefault())
            {
                case PackageProperty.FamilyName:
                    Clipboard.SetText(this._familyName);
                    break;
                case PackageProperty.FullName:
                    Clipboard.SetText(this._fullName);
                    break;
                case PackageProperty.Subject:
                    Clipboard.SetText(this._publisherHash);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private async Task LoadFile(string file)
        {
            if (file == null)
            {
                return;
            }

            if (!File.Exists(file))
            {
                this._interactionService.ShowError(string.Format(Resources.Localization.Dialogs_PackageName_MissingFile, file));
                return;
            }

            AppxPackage manifest;

            try
            {
                switch (Path.GetExtension(file).ToLowerInvariant())
                {
                    case FileConstants.MsixExtension:
                    case FileConstants.AppxExtension:
                        {
                            using var reader = FileReaderFactory.CreateFileReader(file);
                            var manifestReader = new AppxManifestReader();
                            manifest = await manifestReader.Read(reader).ConfigureAwait(true);
                            break;
                        }

                    case ".xml":
                        {
                            if (Path.GetFileName(file).Equals(FileConstants.AppxManifestFile, StringComparison.OrdinalIgnoreCase))
                            {
                                using var reader = new FileInfoFileReaderAdapter(file);
                                var manifestReader = new AppxManifestReader();
                                manifest = await manifestReader.Read(reader).ConfigureAwait(true);
                                break;
                            }

                            this._interactionService.ShowError(Resources.Localization.Dialogs_PackageName_OnlyManifests);
                            return;
                        }

                    default:
                        this._interactionService.ShowError(string.Format(Resources.Localization.Dialogs_PackageName_NotSupportedExtension, Path.GetExtension(file)));
                        return;
                }
            }
            catch (Exception e)
            {
                this._interactionService.ShowError(e.Message, e);
                return;
            }

            this._version = manifest.Version;
            this._name = manifest.Name;
            this._publisher = manifest.Publisher;
            this._resource = manifest.ResourceId;
            this._architecture = manifest.ProcessorArchitecture;

            this.SetCalculatedProperties();
        }

        private async void OnOpenCommand()
        {
            var settings = new FileDialogSettings(
                new DialogFilterBuilder()
                    .WithPackages()
                    .WithManifests()
                    .WithAll()
                    .WithAllSupported());

            if (!this._interactionService.SelectFile(settings, out var selected))
            {
                return;
            }

            await this.LoadFile(selected).ConfigureAwait(false);
        }
    }
}
