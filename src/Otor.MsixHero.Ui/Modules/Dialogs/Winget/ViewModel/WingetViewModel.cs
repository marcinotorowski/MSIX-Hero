using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Commands.RoutedCommand;
using Otor.MsixHero.Ui.Controls.ChangeableDialog.ViewModel;
using Otor.MsixHero.Ui.Controls.Progress;
using Otor.MsixHero.Winget.Yaml;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Dialogs.Winget.ViewModel
{
    public class WingetViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private readonly IInteractionService interactionService;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManager;
        private ICommand openSuccessLink;
        private ICommand reset;
        private ICommand open;
        private string yamlPath;

        public WingetViewModel(IInteractionService interactionService, ISelfElevationProxyProvider<IAppxPackageManager> packageManager) : base("Create winget manifest", interactionService)
        {
            this.interactionService = interactionService;
            this.packageManager = packageManager;
            this.AddChild(this.Definition = new WingetDefinitionViewModel(interactionService));
        }

        public WingetDefinitionViewModel Definition { get; private set; }

        public ICommand OpenSuccessLinkCommand
        {
            get { return this.openSuccessLink ??= new DelegateCommand(this.OpenSuccessLinkExecuted); }
        }

        public ICommand ResetCommand
        {
            get { return this.reset ??= new DelegateCommand(this.ResetExecuted); }
        }

        public ICommand OpenCommand
        {
            get { return this.open ??= new DelegateCommand(this.OpenExecuted, this.CanOpen); }
        }

        public bool WingetVerified { get; private set; }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            this.WingetVerified = false;

            if (!this.IsValid)
            {
                return false;
            }

            string selected;
            var userSelected = 
                string.IsNullOrEmpty(this.yamlPath) || !File.Exists(this.yamlPath)
                    ? this.interactionService.SaveFile("*.yaml", out selected)
                    : this.interactionService.SaveFile(this.yamlPath, "*.yaml", out selected);

            if (!userSelected)
            {
                return false;
            }

            this.yamlPath = selected;

            var tempPath = Path.Combine(Path.GetTempPath(), "msixhero-" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".yaml");
            try
            {
                if (!(await this.Definition.Save(tempPath, cancellationToken, progress).ConfigureAwait(false)))
                {
                    return false;
                }

                var yamlReader = new YamlReader();
                var pkgManager = await this.packageManager.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
                var validationDetails = await yamlReader.ValidateAsync(tempPath, pkgManager, false, cancellationToken).ConfigureAwait(false);

                if (validationDetails != null)
                {
                    progress.Report(new ProgressData(100, "Validating with winget CLI..."));
                    await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken).ConfigureAwait(false);

                    if (validationDetails.IndexOf("Manifest validation succeeded.", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        var msg = string.Join(Environment.NewLine, validationDetails.Split(Environment.NewLine).Skip(1));
                        if (1 == this.interactionService.ShowMessage("Winget CLI returned validation errors.", new[] { "Ignore these errors", "Continue editing" }, "Validation errors", msg))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        this.WingetVerified = true;
                        this.OnPropertyChanged(nameof(WingetVerified));
                    }
                }

                File.Move(tempPath, selected, true);
                return true;
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        public ProgressProperty GeneralProgress { get; } = new ProgressProperty();

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("yaml", out string yamlSelectedPath))
            {
                this.yamlPath = Path.ChangeExtension(yamlSelectedPath, ".yaml");
#pragma warning disable 4014
                this.GeneralProgress.MonitorProgress(this.Definition.LoadFromYaml(yamlSelectedPath));
            }
            else if (parameters.TryGetValue("msix", out string msixPath))
            {
                this.yamlPath = Path.ChangeExtension(Path.GetFileNameWithoutExtension(msixPath), ".yaml");
                this.GeneralProgress.MonitorProgress(this.Definition.LoadFromFile(msixPath));
            }
            else
            {
                this.GeneralProgress.MonitorProgress(this.Definition.NewManifest(CancellationToken.None));
            }
        }

        private void ResetExecuted(object parameter)
        {
            this.State.IsSaved = false;
        }

        private bool CanOpen(object obj)
        {
            return !this.State.IsSaved;
        }

        private async void OpenExecuted(object parameter)
        {
            if (this.State.IsSaved)
            {
                this.State.IsSaved = false;
            }

            if (!this.interactionService.SelectFile("*.yaml", out var selectedFile))
            {
                return;
            }

            this.yamlPath = selectedFile;
            var task = this.Definition.LoadFromYaml(selectedFile);
            this.GeneralProgress.MonitorProgress(task);
            await task.ConfigureAwait(false);
        }

        private void OpenSuccessLinkExecuted(object parameter)
        {
            Process.Start("explorer.exe", "/select," + this.yamlPath);
        }
    }
}

