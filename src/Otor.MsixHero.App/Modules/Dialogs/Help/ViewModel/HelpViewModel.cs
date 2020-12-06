using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Updates;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Help.ViewModel
{
    public class HelpViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IUpdateChecker updateChecker;
        private readonly IMsixHeroApplication application;
        private bool isChecked;

        public HelpViewModel(IUpdateChecker updateChecker, IMsixHeroApplication application)
        {
            this.CloseCommand = new DelegateCommand(((IDialogAware)this).OnDialogClosed, ((IDialogAware)this).CanCloseDialog);
            this.ShowReleaseNotes = new DelegateCommand(this.OnShowReleaseNotes);

            this.updateChecker = updateChecker;
            this.application = application;
            this.Check = new DelegateCommand(this.CheckExecute, this.CheckCanExecute);
            this.UpdateCheck = new AsyncProperty<UpdateCheckResult>();

            var assemblyLocation = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;
            this.Version = FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;
        }

        public string Version { get; }

        public ICommand CloseCommand { get; }

        public ICommand ShowReleaseNotes { get; }

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

        private void OnShowReleaseNotes()
        {
            this.RequestClose?.Invoke(new DialogResult());
            this.application.CommandExecutor.Invoke(this, new SetCurrentModeCommand(ApplicationMode.WhatsNew));
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

        private bool CheckCanExecute()
        {
            return true;
        }

        private async void CheckExecute()
        {
            await this.UpdateCheck.Load(this.updateChecker.CheckForNewVersion()).ConfigureAwait(false);
        }
    }
}
