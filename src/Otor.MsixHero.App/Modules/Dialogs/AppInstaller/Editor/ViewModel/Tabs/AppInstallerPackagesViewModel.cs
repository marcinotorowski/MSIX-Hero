using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.AppInstaller.Entities;
using Otor.MsixHero.Appx.Packaging;
using Dapplo.Log;
using Otor.MsixHero.App.Helpers.Dialogs;
using Otor.MsixHero.Appx.Reader.Manifest.Entities.Summary;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.AppInstaller.Editor.ViewModel.Tabs
{
    public class AppInstallerPackagesViewModel : ChangeableContainer
    {
        private readonly IInteractionService _interactionService;
        private readonly IConfigurationService _configurationService;
        private static readonly LogSource Logger = new();        
        public AppInstallerPackagesViewModel(
            IInteractionService interactionService, 
            IConfigurationService configurationService,
            IEnumerable<AppInstallerBaseEntry> entries = null)
        {
            this._interactionService = interactionService;
            this._configurationService = configurationService;
            this.Items = new ValidatedChangeableCollection<AppInstallerBasePackageViewModel>();
            
            if (entries != null)
            {
                this.SetPackages(entries);
            }
            
            this.Items.CollectionChanged += ItemsOnCollectionChanged;
            this.Selected = new ChangeableProperty<AppInstallerBasePackageViewModel>(this.Items.FirstOrDefault());

            this.New = new DelegateCommand<object>(this.OnNew, this.CanNew);
            this.Browse = new DelegateCommand(this.OnBrowse, this.CanBrowse);
            this.Delete = new DelegateCommand(this.OnDelete, this.CanDelete);
            
            this.AddChild(this.Items);
        }
        
        public ICommand New { get; }
        
        public ICommand Delete { get; }
        
        public ICommand Browse { get; }
        
        public ValidatedChangeableCollection<AppInstallerBasePackageViewModel> Items { get; }
        
        public ChangeableProperty<AppInstallerBasePackageViewModel> Selected { get; }
        
        public void SetPackages(IEnumerable<AppInstallerBaseEntry> entries)
        {
            this.Items.Clear();

            var newItems = new List<AppInstallerBasePackageViewModel>();
            foreach (var item in entries)
            {
                if (item is AppInstallerPackageEntry package)
                {
                    newItems.Add(new AppInstallerPackageViewModel(package));
                }
                else if (item is AppInstallerBundleEntry bundle)
                {
                    newItems.Add(new AppInstallerBundleViewModel(bundle));
                }
                else
                {
                    Logger.Warn().WriteLine($"Unknown type of the entry ({item.GetType().Name}. Only packages and bundles are supported.");
                }
            }

            this.Items.AddRange(newItems);
            this.SetHeaders();
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.SetHeaders();
        }
        
        private void SetHeaders()
        {
            var bundles = 0;
            var packages = 0;
            foreach (var current in this.Items)
            {
                if (current is AppInstallerBundleViewModel)
                {
                    current.Header.CurrentValue = string.Format(Resources.Localization.Dialogs_AppInstaller_List_Bundle_Format, ++bundles);
                }
                else if (current is AppInstallerPackageViewModel)
                {
                    current.Header.CurrentValue = string.Format(Resources.Localization.Dialogs_AppInstaller_List_Bundle_Package, ++packages);
                }
            }
        }

        private void OnNew(object payload)
        {
            switch (payload is AppInstallerBasePackageViewModelType type ? type : AppInstallerBasePackageViewModelType.Package)
            {
                case AppInstallerBasePackageViewModelType.Bundle:
                    var entry = new AppInstallerBundleViewModel(new AppInstallerBundleEntry());
                    this.Items.Add(entry);
                    break;
                default:
                    var pkgEntry = new AppInstallerPackageViewModel(new AppInstallerPackageEntry());
                    this.Items.Add(pkgEntry);
                    break;
            }

            this.Selected.CurrentValue = this.Items.Last();
        }

        private void OnDelete()
        {
            var index = this.Items.IndexOf(this.Selected.CurrentValue);
            this.Items.RemoveAt(index);

            if (this.Items.Count < 2)
            {
                this.Selected.CurrentValue = this.Items.FirstOrDefault();
            }
            else
            {
                if (index >= this.Items.Count)
                {
                    index = Math.Max(0, index - 1);
                }

                this.Selected.CurrentValue = this.Items[index];
            }
        }

        private bool CanNew(object payload)
        {
            return true;
        }

        private async void OnBrowse()
        {
            // ReSharper disable StringLiteralTypo
            var f = new DialogFilterBuilder()
                .WithPackages(DialogFilterBuilderPackagesExtensions.PackageTypes.All)
                .WithAll();
            
            // ReSharper restore StringLiteralTypo
            if (!this._interactionService.SelectFiles(new FileDialogSettings(f), out var selectedFiles))
            {
                return;
            }

            try
            {
                var configuration = await this._configurationService.GetCurrentConfigurationAsync().ConfigureAwait(true);
                var configValue = configuration.AppInstaller?.DefaultRemoteLocationPackages;
                if (string.IsNullOrEmpty(configValue))
                {
                    configValue = "http://server-name/";
                }

                var reader = new AppxIdentityReader();
                foreach (var selectedFile in selectedFiles)
                {
                    var newFilePath = new FileInfo(selectedFile);
                    var read = await reader.GetIdentity(selectedFile).ConfigureAwait(true);

                    if (string.Equals(FileExtensions.AppxBundle, newFilePath.Extension, StringComparison.OrdinalIgnoreCase) || string.Equals(FileExtensions.MsixBundle, newFilePath.Extension, StringComparison.OrdinalIgnoreCase))
                    {
                        this.Items.Add(new AppInstallerBundleViewModel(new AppInstallerBundleEntry
                        {
                            Name = read.Name,
                            Publisher = read.Publisher,
                            Version = read.Version,
                            Uri = $"{configValue.TrimEnd('/')}/{newFilePath.Name}"
                        }));
                    }
                    else
                    {
                        this.Items.Add(new AppInstallerPackageViewModel(new AppInstallerPackageEntry
                        {
                            Name = read.Name,
                            Publisher = read.Publisher,
                            Version = read.Version,
                            Architecture = Enum.Parse<AppInstallerPackageArchitecture>(read.Architectures?.Any() == true ? read.Architectures.First().ToString("G") : "neutral", true),
                            Uri = $"{configValue.TrimEnd('/')}/{newFilePath.Name}"
                        }));
                    }   
                }

                this.Selected.CurrentValue = this.Items.Last();
            }
            catch (Exception e)
            {
                Logger.Error().WriteLine(e);
                this._interactionService.ShowError(Resources.Localization.Dialogs_AppInstaller_Errors_Opening, e);
            }
        }

        private bool CanBrowse()
        {
            return true;
        }

        private bool CanDelete()
        {
            return this.Selected.CurrentValue != null;
        }
    }
}