using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using Otor.MsixHero.App.Helpers;
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
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.Names.ViewModel
{
    public class NamesViewModel : ChangeableDialogViewModel, IDataErrorInfo, IDialogAware
    {
        private readonly IInteractionService interactionService;
        private string name;
        private string publisherHash;
        private string publisher;
        private string version;
        private string resource;
        private AppxPackageArchitecture architecture;
        private string fullName;
        private string familyName;

        private readonly Func<string, string> validatePublisher = AppxValidatorFactory.ValidateSubject();
        private readonly Func<string, string> validateVersion = AppxValidatorFactory.ValidateVersion();
        private readonly Func<string, string> validatePackageName = AppxValidatorFactory.ValidatePackageName();
        private readonly Func<string, string> validateResourceId = AppxValidatorFactory.ValidateResourceId(false);
        private ICommand openCommand, copyCommand;

        public NamesViewModel(IInteractionService interactionService) : base("Package name calculator", interactionService)
        {
            this.interactionService = interactionService;
        }

        public ICommand OpenCommand => this.openCommand ??= new ActionCommand(this.OnOpenCommand);
        public ICommand CopyCommand => this.copyCommand ??= new DelegateCommand<PackageProperty?>(this.OnCopyCommand);

        public new string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Publisher):
                        return this.validatePublisher(this.publisher);
                    case nameof(Version):
                        return this.validateVersion(this.version);
                    case nameof(Name):
                        return this.validatePackageName(this.name);
                    /*case nameof(FullName):
                        return null == (this[nameof(Publisher)] ?? this[nameof(Name)] ?? this[nameof(Version)] ?? this[nameof(Resource)]) ? null : "The value cannot be calculated";
                    case nameof(FamilyName):
                        return null == (this[nameof(Publisher)] ?? this[nameof(Name)]) ? null : "The value cannot be calculated";
                    case nameof(PublisherHash):
                        return null == this[nameof(Publisher)] ? null : "The value cannot be calculated";*/
                    case nameof(Resource):
                        return this.validateResourceId(this.resource);
                    default:
                        return null;
                }
            }
        }
        
        public string Publisher
        {
            get => this.publisher;
            set
            {
                this.SetField(ref this.publisher, value);
                this.SetCalculatedProperties();
            }
        }

        public string PublisherHash => this.publisherHash;

        public AppxPackageArchitecture Architecture
        {
            get => this.architecture;
            set
            {
                this.SetField(ref this.architecture, value);
                this.SetCalculatedProperties();
            }
        }

        public string FullName => this.fullName;

        public string FamilyName => this.familyName;

        public string Version
        {
            get => this.version;
            set
            {
                this.SetField(ref this.version, value);
                this.SetCalculatedProperties();
            }
        }

        public string Name
        {
            get => this.name;
            set
            {
                this.SetField(ref this.name, value);
                this.SetCalculatedProperties();
            }
        }

        public string Resource
        {
            get => this.resource;
            set
            {
                this.SetField(ref this.resource, value);
                this.SetCalculatedProperties();
            }
        }

        void IDialogAware.OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.Count == 0)
            {
                this.name = "PackageName";
                this.version = "1.0.0.0";
                this.architecture = AppxPackageArchitecture.x64;
                this.publisher = "CN=Publisher Name";
                this.resource = "";
                this.SetCalculatedProperties();
            }
            else
            {
#pragma warning disable CS4014
                this.LoadFile(parameters.GetValue<string>(parameters.Keys.First()));
#pragma warning restore CS4014
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
                this.fullName = "Invalid publisher name";
                this.publisherHash = "Invalid publisher name";
                this.familyName = "Invalid publisher name";
            }
            else if (this[nameof(Name)] != null)
            {
                this.fullName = "Invalid package name";
                this.familyName = "Invalid package name";
            }
            else if (this[nameof(Version)] != null)
            {
                this.fullName = "Invalid package version";
            }
            else if (this[nameof(Resource)] != null)
            {
                this.fullName = "Invalid resource ID";
            }
            else
            {
                this.familyName = AppxPackaging.GetPackageFamilyName(this.name, this.publisher);
                this.publisherHash = this.familyName.Split('_').Last();
                this.fullName = AppxPackaging.GetPackageFullName(this.name, this.publisher, this.architecture, this.version, this.resource);
            }

            this.OnPropertyChanged(null);
        }
        
        private void OnCopyCommand(PackageProperty? parameter)
        {
            switch (parameter.GetValueOrDefault())
            {
                case PackageProperty.FamilyName:
                    Clipboard.SetText(this.familyName);
                    break;
                case PackageProperty.FullName:
                    Clipboard.SetText(this.fullName);
                    break;
                case PackageProperty.Subject:
                    Clipboard.SetText(this.publisherHash);
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
                this.interactionService.ShowError("File '" + file + "' does not exist.");
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

                            this.interactionService.ShowError("Only manifest files are accepted.");
                            return;
                        }

                    default:
                        this.interactionService.ShowError("File extension " + Path.GetExtension(file) + " is not supported.");
                        return;
                }
            }
            catch (Exception e)
            {
                this.interactionService.ShowError(e.Message, e);
                return;
            }

            this.version = manifest.Version;
            this.name = manifest.Name;
            this.publisher = manifest.Publisher;
            this.resource = manifest.ResourceId;
            this.architecture = manifest.ProcessorArchitecture;

            this.SetCalculatedProperties();
        }

        private async void OnOpenCommand()
        {
            var settings = new FileDialogSettings(new DialogFilterBuilder(
                "*" + FileConstants.MsixExtension,
                "*" + FileConstants.AppxExtension,
                FileConstants.AppxManifestFile).BuildFilter());

            if (!this.interactionService.SelectFile(settings, out var selected))
            {
                return;
            }

            await this.LoadFile(selected).ConfigureAwait(false);
        }
    }
}
