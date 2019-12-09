using otor.msixhero.lib.BusinessLayer.Appx.AppInstaller;
using otor.msixhero.lib.Domain.Appx.AppInstaller;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Builder;
using otor.msixhero.ui.Modules.Dialogs.Common.PackageSelector.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.AppInstaller.ViewModel
{
    public class AppInstallerViewModel : NotifyPropertyChanged, IDialogAware, IDataErrorInfo
    {
        private readonly IAppxContentBuilder appxContentBuilder;
        private readonly IInteractionService interactionService;
        private readonly IConfigurationService configurationService;
        private int progress;
        private string progressMessage;
        private bool isLoading;
        private bool isSuccess;
        private ICommand openSuccessLink;
        private ICommand reset;

        public AppInstallerViewModel(
            IAppxContentBuilder appxContentBuilder,
            IInteractionService interactionService,
            IConfigurationService configurationService)
        {
            this.appxContentBuilder = appxContentBuilder;
            this.interactionService = interactionService;
            this.configurationService = configurationService;

            this.AppInstallerUpdateCheckingMethod = new ChangeableProperty<AppInstallerUpdateCheckingMethod>(lib.BusinessLayer.Appx.AppInstaller.AppInstallerUpdateCheckingMethod.LaunchAndBackground);
            this.AllowDowngrades = new ChangeableProperty<bool>();
            this.BlockLaunching = new ChangeableProperty<bool>();
            this.ShowPrompt = new ChangeableProperty<bool>();

            this.AllowDowngrades.ValueChanged += this.OnBooleanChanged;
            this.BlockLaunching.ValueChanged += this.OnBooleanChanged;
            this.ShowPrompt.ValueChanged += this.OnBooleanChanged;

            this.MainPackageUri = new ValidatedChangeableProperty<string>(this.ValidateUri, true);
            this.AppInstallerUri = new ValidatedChangeableProperty<string>(this.ValidateUriOrEmpty, true);

            this.OutputPath = new ChangeableFileProperty(interactionService)
            {
                Validators = new[] { ChangeableFileProperty.ValidatePath },
                OpenForSaving = true,
                Filter = "App-Installer files|*.appinstaller|All files|*.*"
            };

            this.AppInstallerUpdateCheckingMethod.ValueChanged += this.AppInstallerUpdateCheckingMethodValueChanged;
            this.Hours = new ValidatedChangeableProperty<string>(this.ValidateHours, "24");

            this.PackageSelection = new PackageSelectorViewModel(interactionService)
            {
                AllowBundles = true,
                AllowManifests = true,
                ShowPackageTypeSelector = true,
                CustomPrompt = "What will be targeted by this .appinstaller?"
            };

            this.PackageSelection.InputPath.ValueChanged += this.InputPathOnValueChanged;

            this.ChangeableContainer = new ChangeableContainer(
                this.MainPackageUri,
                this.AppInstallerUri,
                this.AppInstallerUpdateCheckingMethod,
                this.AllowDowngrades,
                this.BlockLaunching,
                this.ShowPrompt,
                this.Hours,
                this.PackageSelection)
            {
                IsValidated = false
            };
        }

        public bool ShowLaunchOptions =>
            this.AppInstallerUpdateCheckingMethod.CurrentValue == lib.BusinessLayer.Appx.AppInstaller.AppInstallerUpdateCheckingMethod.LaunchAndBackground ||
            this.AppInstallerUpdateCheckingMethod.CurrentValue == lib.BusinessLayer.Appx.AppInstaller.AppInstallerUpdateCheckingMethod.Launch;

        public ChangeableProperty<AppInstallerUpdateCheckingMethod> AppInstallerUpdateCheckingMethod { get; }

        public ValidatedChangeableProperty<string> Hours { get; }

        public ChangeableProperty<bool> BlockLaunching { get; }

        public ChangeableProperty<bool> ShowPrompt { get; }

        public ChangeableProperty<bool> AllowDowngrades { get; }

        public ChangeableContainer ChangeableContainer { get; }

        public ChangeableFileProperty OutputPath { get; }

        public ChangeableProperty<string> MainPackageUri { get; }

        public ChangeableProperty<string> AppInstallerUri { get; }

        public string CompatibleWindows
        {
            get
            {
                var minWin10 = this.appxContentBuilder.GetMinimumSupportedWindowsVersion(this.GetCurrentAppInstallerConfig());
                return $"Windows 10 {minWin10}";
            }
        }


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

        public string Error
        {
            get => this.ChangeableContainer.IsValid ? null : this.ChangeableContainer.ValidationMessage;
        }

        public string this[string columnName] => null;

        public bool CanSave()
        {
            return this.ChangeableContainer.IsValid;
        }

        public string Title
        {
            get => "Create .appinstaller";
        }

        public PackageSelectorViewModel PackageSelection { get; }

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
        }

        public async Task Save()
        {
            this.ChangeableContainer.IsValidated = true;
            if (!this.ChangeableContainer.IsValid)
            {
                return;
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

                if (!this.interactionService.SaveFile(this.OutputPath.CurrentValue, this.OutputPath.Filter, out var selected))
                {
                    return;
                }

                var appInstaller = this.GetCurrentAppInstallerConfig();
                await this.appxContentBuilder.Create(appInstaller, selected).ConfigureAwait(false);
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

        private AppInstallerConfig GetCurrentAppInstallerConfig()
        {
            var builder = new AppInstallerBuilder
            {
                MainPackageType = this.PackageSelection.PackageType.CurrentValue,
                MainPackageName = this.PackageSelection.Name.CurrentValue,
                MainPackageArchitecture = this.PackageSelection.Architecture.CurrentValue,
                MainPackagePublisher = this.PackageSelection.Publisher.CurrentValue,
                MainPackageVersion = this.PackageSelection.Version.CurrentValue,
                HoursBetweenUpdateChecks = int.Parse(this.Hours.CurrentValue),
                CheckForUpdates = this.AppInstallerUpdateCheckingMethod.CurrentValue,
                ShowPrompt = this.ShowPrompt.CurrentValue,
                UpdateBlocksActivation = this.BlockLaunching.CurrentValue,
                AllowDowngrades = this.AllowDowngrades.CurrentValue,
                RedirectUri = string.IsNullOrEmpty(this.AppInstallerUri.CurrentValue) ? null : new Uri(this.AppInstallerUri.CurrentValue),
                MainPackageUri = string.IsNullOrEmpty(this.MainPackageUri.CurrentValue) ? null : new Uri(this.MainPackageUri.CurrentValue),
            };

            var appInstaller = builder.Build();
            return appInstaller;
        }

        private void ResetExecuted(object parameter)
        {
            this.PackageSelection.Reset();
            this.OutputPath.Reset();
            this.IsSuccess = false;
        }

        private void OpenSuccessLinkExecuted(object parameter)
        {
            Process.Start("explorer.exe", "/select," + this.OutputPath.CurrentValue);
        }

        private string ValidateUri(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "The value may not be empty.";
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            {
                return $"The value '{value}' is not a valid URI.";
            }

            return null;
        }

        private string ValidateUriOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            {
                return $"The value '{value}' is not a valid URI.";
            }

            return null;
        }

        private void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty((string)e.NewValue))
            {
                var isManifest = string.Equals(Path.GetFileName((string)e.NewValue), "appxmanifest.xml", StringComparison.OrdinalIgnoreCase);
                
                if (isManifest)
                {
                    return;
                }
            }

            if (string.IsNullOrEmpty(this.OutputPath.CurrentValue) && !string.IsNullOrEmpty(this.PackageSelection.InputPath.CurrentValue))
            {
                this.OutputPath.CurrentValue = this.PackageSelection.InputPath.CurrentValue + ".appinstaller";
            }

            if (string.IsNullOrEmpty(this.MainPackageUri.CurrentValue) && !string.IsNullOrEmpty(e.NewValue as string))
            {
                var newFilePath = new FileInfo((string)e.NewValue);
                var configValue = this.configurationService.GetCurrentConfiguration().AppInstaller?.DefaultRemoteLocationPackages;
                if (string.IsNullOrEmpty(configValue))
                {
                    configValue = "http://server-name/";
                }

                this.MainPackageUri.CurrentValue = $"{configValue.TrimEnd('/')}/{newFilePath.Name}";
            }
        }

        private void AppInstallerUpdateCheckingMethodValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(ShowLaunchOptions));
            this.OnPropertyChanged(nameof(CompatibleWindows));
        }
        
        private string ValidateHours(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return "This value cannot be empty.";
            }

            if (!int.TryParse(newValue, out var value))
            {
                return $"The value '{newValue}' is not a valid number.";
            }

            if (value < 0 || value > 255)
            {
                return $"The value '{newValue}' lies outside of valid range 0-255.";
            }

            return null;
        }

        private void OnBooleanChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(CompatibleWindows));
        }
    }
}