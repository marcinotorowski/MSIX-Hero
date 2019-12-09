using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Builder;
using otor.msixhero.lib.Domain.Appx.AppInstaller;
using otor.msixhero.lib.Domain.Appx.ModificationPackage;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Modules.Dialogs.Common.PackageSelector.ViewModel;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.ModificationPackage.ViewModel
{
    public class ModificationPackageViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IAppxContentBuilder contentBuilder;
        private readonly IInteractionService interactionService;
        private int progress;
        private string progressMessage;
        private bool isLoading;
        private bool isSuccess;
        private ICommand openSuccessLink;
        private ICommand reset;

        public ModificationPackageViewModel(IAppxContentBuilder contentBuilder, IInteractionService interactionService)
        {
            this.contentBuilder = contentBuilder;
            this.interactionService = interactionService;

            this.PackageSelection = new PackageSelectorViewModel(interactionService)
            {
                AllowBundles = false,
                ShowPackageTypeSelector = false,
                RequireFullIdentity = false,
                IsValidated = false
            };

            this.ModificationPackageDetails = new PackageSelectorViewModel(interactionService)
            {
                AllowBundles = false,
                ShowPackageTypeSelector = false,
                AllowBrowsing = false,
                AllowManifests = false,
                RequireFullIdentity = true,
                IsValidated = false
            };

            this.ModificationPackageDetails.Version.CurrentValue = "1.0.0";
            this.ModificationPackageDetails.Architecture.CurrentValue = AppInstallerPackageArchitecture.neutral;
            this.ModificationPackageDetails.Commit();
            
            this.Create = new ChangeableProperty<ModificationPackageBuilderAction>();

            this.ChangeableContainer = new ChangeableContainer(this.PackageSelection, this.ModificationPackageDetails, this.Create)
            {
                IsValidated = false
            };
        }

        public ChangeableContainer ChangeableContainer { get; }
        
        public PackageSelectorViewModel PackageSelection { get; }

        public PackageSelectorViewModel ModificationPackageDetails { get; }

        public ChangeableProperty<ModificationPackageBuilderAction> Create { get; }

        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetField(ref this.isLoading, value);
        }

        public int Progress
        {
            get => this.progress;
            private set => this.SetField(ref this.progress, value);
        }

        public string ProgressMessage
        {
            get => this.progressMessage;
            private set => this.SetField(ref this.progressMessage, value);
        }

        public ICommand OpenSuccessLink
        {
            get { return this.openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand Reset
        {
            get { return this.reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public bool IsSuccess
        {
            get => this.isSuccess;
            set => this.SetField(ref this.isSuccess, value);
        }
        
        public bool CanSave()
        {
            return this.ChangeableContainer.IsValid;
        }

        public string Title
        {
            get => "Create modification package stub";
        }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (!parameters.TryGetValue("file", out string sourceFile))
            {
                return;
            }

            this.PackageSelection.InputPath.CurrentValue = sourceFile;
            this.PackageSelection.AllowChangingSourcePackage = false;

            this.ModificationPackageDetails.Architecture.CurrentValue = this.PackageSelection.Architecture.CurrentValue;
            this.ModificationPackageDetails.Name.CurrentValue = this.PackageSelection.Name.CurrentValue + "-Modification";
            this.ModificationPackageDetails.Commit();
        }

        public async Task Save()
        {
            this.ChangeableContainer.IsValidated = true;
            if (!this.ChangeableContainer.IsValid)
            {
                return;
            }

            // ReSharper disable once NotAccessedVariable
            string selectedPath;

            switch (this.Create.CurrentValue)
            {
                case ModificationPackageBuilderAction.Manifest:
                    if (!this.interactionService.SelectFolder(out selectedPath))
                    {
                        return;
                    }

                    selectedPath = Path.Join(selectedPath, "AppxManifest.xml");
                    break;

                case ModificationPackageBuilderAction.Msix:
                case ModificationPackageBuilderAction.SignedMsix:
                    if (!this.interactionService.SaveFile("MSIX Modification Packages|*.msix", out selectedPath))
                    {
                        return;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var token = new Progress();

            EventHandler<ProgressData> handler = (sender, data) =>
            {
                this.Progress = data.Progress;
                this.ProgressMessage = data.Message;
            };

            this.IsLoading = true;
            try
            {
                token.ProgressChanged += handler;

                var modificationPkgCreationRequest = new ModificationPackageConfig
                {
                    Name = this.ModificationPackageDetails.Name.CurrentValue,
                    Publisher = this.ModificationPackageDetails.Publisher.CurrentValue,
                    Architecture = this.ModificationPackageDetails.Architecture.CurrentValue,
                    Version = this.ModificationPackageDetails.Version.CurrentValue,
                    ParentName = this.ModificationPackageDetails.Name.CurrentValue,
                    ParentPublisher = this.ModificationPackageDetails.Publisher.CurrentValue
                };

                await this.contentBuilder.Create(modificationPkgCreationRequest, selectedPath, this.Create.CurrentValue).ConfigureAwait(false);
                
                this.IsSuccess = true;
            }
            finally
            {
                token.ProgressChanged -= handler;
                this.IsLoading = false;
                this.Progress = 100;
                this.ProgressMessage = null;
            }
        }

        private void ResetExecuted(object parameter)
        {
            this.PackageSelection.Reset();
            this.IsSuccess = false;
        }

        private void OpenSuccessLinkExecuted(object parameter)
        {
        }
    }
}

