using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.UpdateImpact;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Controls.Progress;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.Dialogs.UpdateImpact.ViewModel.Items;

namespace otor.msixhero.ui.Modules.Dialogs.UpdateImpact.ViewModel
{

    public class UpdateImpactViewModel : ChangeableDialogViewModel
    {
        private readonly IAppxUpdateImpactAnalyzer updateImpactAnalyzer;
        private readonly IInteractionService interactionService;

        public UpdateImpactViewModel(IAppxUpdateImpactAnalyzer updateImpactAnalyzer, IInteractionService interactionService) : base("Analyze update impact", interactionService)
        {
            this.updateImpactAnalyzer = updateImpactAnalyzer;
            this.interactionService = interactionService;

            this.Path1 = new ChangeableFileProperty(interactionService)
            {
                Validators = new[] { ChangeableFolderProperty.ValidatePath },
                Filter = "MSIX/APPX packages|*.msix;*.appx|Manifest files|AppxManifest.xml|All files|*.*"
            };

            this.Path2 = new ChangeableFileProperty(interactionService)
            {
                Validators = new[] { ChangeableFileProperty.ValidatePath },
                Filter = "MSIX/APPX packages|*.msix;*.appx|Manifest files|AppxManifest.xml|All files|*.*"
            };
            
            this.AddChildren(this.Path1, this.Path2);
            this.SetValidationMode(ValidationMode.Silent, true);
            this.Compare = new DelegateCommand(this.CompareExecuted);
        }

        public ICommand Compare { get; }

        public ChangeableFileProperty Path1 { get; }

        public ChangeableFileProperty Path2 { get; }

        public ProgressProperty Progress { get; } = new ProgressProperty();

        public AsyncProperty<ComparisonViewModel> Results { get; } = new AsyncProperty<ComparisonViewModel>();

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            return true;
        }

        private async void CompareExecuted(object obj)
        {
            this.Progress.Progress = -1;
            this.Progress.IsLoading = true;
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    var task = this.updateImpactAnalyzer.Analyze(this.Path1.CurrentValue, this.Path2.CurrentValue, cts.Token);
                    this.Progress.MonitorProgress(task, cts);
                    var result = await task.ConfigureAwait(false);

                    result.OldPackage.Path = this.Path1.CurrentValue;
                    result.NewPackage.Path = this.Path2.CurrentValue;

                    await this.Results.Load(Task.FromResult(new ComparisonViewModel(result))).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (UserHandledException)
            {
            }
            catch (Exception e)
            {
                this.interactionService.ShowError("Could not compare selected packages. " + e.Message, e);
            }
            finally
            {
                this.Progress.IsLoading = false;
            }
        }
    }
}

