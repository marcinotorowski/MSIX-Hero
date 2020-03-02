using System.Diagnostics;
using System.Windows.Input;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Helpers;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Modules.PackageList.ViewModel.Elements.Psf;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel.Elements
{
    public class SinglePackageCommandHandler
    {
        private readonly IConfigurationService configurationService;
        private readonly IInteractionService interactionService;

        public SinglePackageCommandHandler(IConfigurationService configurationService, IInteractionService interactionService)
        {
            this.configurationService = configurationService;
            this.interactionService = interactionService;
            this.EditScript = new DelegateCommand(this.EditScriptExecute);
        }

        public ICommand EditScript { get; }

        private void EditScriptExecute(object param)
        {
            if (!(param is AppxPsfScriptViewModel script))
            {
                return;
            }

            var config = this.configurationService.GetCurrentConfiguration().Editing ?? new EditingConfiguration();
            var executor = new FileInvoker(this.interactionService);
            executor.Execute(config.PowerShellEditorType, config.PowerShellEditor, script.FullLocalPath);
        }
    }
}