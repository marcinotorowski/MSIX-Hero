using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Otor.MsixHero.Infrastructure.Updates;
using Otor.MsixHero.Ui.Commands.RoutedCommand;
using Otor.MsixHero.Ui.Helpers;
using Otor.MsixHero.Ui.ViewModel;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Dialogs.Help.ViewModel
{
    public class HelpViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IUpdateChecker updateChecker;
        private bool isChecked;

        public HelpViewModel(IUpdateChecker updateChecker)
        {
            this.CloseCommand = new DelegateCommand(o => ((IDialogAware)this).OnDialogClosed(), o => ((IDialogAware)this).CanCloseDialog());
            this.updateChecker = updateChecker;
            this.Check = new DelegateCommand(this.CheckExecute, this.CheckCanExecute);
            this.UpdateCheck = new AsyncProperty<UpdateCheckResult>();

            var assemblyLocation = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;
            this.Version = FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;
        }

        public string Version { get; }

        public ICommand CloseCommand { get; }

        bool IDialogAware.CanCloseDialog()
        {
            return true;
        }

        void IDialogAware.OnDialogClosed()
        {
            this.RequestClose?.Invoke(new DialogResult());
        }

        void IDialogAware.OnDialogOpened(IDialogParameters parameters)
        {
        }

        string IDialogAware.Title => "About MSIX Hero";

        public event Action<IDialogResult> RequestClose;

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
            await this.UpdateCheck.Load(this.updateChecker.CheckForNewVersion()).ConfigureAwait(false);
        }
    }
}
