using System.Windows.Input;
using otor.msixhero.lib.Infrastructure.Update;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.Settings.ViewModel
{
    public class UpdateViewModel : NotifyPropertyChanged
    {
        private readonly IUpdateChecker updateChecker;
        private bool isChecked;

        public UpdateViewModel(IUpdateChecker updateChecker)
        {
            this.updateChecker = updateChecker;
            this.Check = new DelegateCommand(this.CheckExecute, this.CheckCanExecute);
            this.UpdateCheck = new AsyncProperty<UpdateCheckResult>();
        }

        public bool IsChecked
        {
            get => this.isChecked;
            set => this.SetField(ref this.isChecked, value);
        }

        public ICommand Check { get; private set; }

        public AsyncProperty<UpdateCheckResult> UpdateCheck { get; }

        private bool CheckCanExecute(object obj)
        {
            return true;
        }

        private async void CheckExecute(object obj)
        {
            await this.UpdateCheck.Load(this.updateChecker.CheckForNewVersion());
        }
    }
}
