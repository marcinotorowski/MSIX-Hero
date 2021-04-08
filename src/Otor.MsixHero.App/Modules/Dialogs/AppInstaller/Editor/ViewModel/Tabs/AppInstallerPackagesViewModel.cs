using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.AppInstaller.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.AppInstaller.Editor.ViewModel.Tabs
{
    public class AppInstallerPackagesViewModel : ChangeableContainer
    {
        private readonly IInteractionService interactionService;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AppInstallerPackagesViewModel));
        
        public AppInstallerPackagesViewModel(IInteractionService interactionService, IEnumerable<AppInstallerBaseEntry> entries = null)
        {
            this.interactionService = interactionService;
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
                    Logger.Warn($"Unknown type of the entry ({item.GetType().Name}. Only packages and bundles are supported.");
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
                    current.Header.CurrentValue = $"BUNDLE #{++bundles}";
                }
                else if (current is AppInstallerPackageViewModel)
                {
                    current.Header.CurrentValue = $"PACKAGE #{++packages}";
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
            var f = new DialogFilterBuilder("*.msix", "*.appx", ".appxbundle", "*.msixbundle").BuildFilter();
            // ReSharper restore StringLiteralTypo
            if (!this.interactionService.SelectFiles(new FileDialogSettings(f), out var selectedFiles))
            {
                return;
            }

            try
            {
                foreach (var selectedFile in selectedFiles)
                {
                    var read = await AppxManifestSummaryBuilder.FromFile(selectedFile, AppxManifestSummaryBuilderMode.Identity);
                    this.Items.Add(new AppInstallerPackageViewModel(new AppInstallerPackageEntry
                    {
                        Name = read.Name,
                        Publisher = read.Publisher,
                        Version = read.Version,
                        Architecture = Enum.Parse<AppInstallerPackageArchitecture>(read.ProcessorArchitecture)
                    }));
                }
                
                this.Selected.CurrentValue = this.Items.Last();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                this.interactionService.ShowError("Could not open selected package.", e);
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