using System.Windows;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Otor.MsixHero.App.Modules.Packages.ViewModels.PackageExpert;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Packages.Views.PackageExpert
{
    /// <summary>
    /// Interaction logic for PackageExpert
    /// </summary>
    public partial class PackageExpert
    {
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(PackageExpert), new PropertyMetadata(null, OnFilePathChanged));
        private IInterProcessCommunicationManager ipcManager;
        private ISelfElevationProxyProvider<IAppxPackageManager> appxPackageManagerProxy;
        private IRunningDetector interactionService;
        private IInteractionService signManager;
        private ISelfElevationProxyProvider<ISigningManager> selfElevationProxy;
        private IDialogService dialogService;

        public PackageExpert()
        {
            this.InitializeComponent();
        }

        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        private static async void OnFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (PackageExpert)d;
            var val = (string) e.NewValue;

            if (val == null)
            {
               // obj.DataContext = null;
                return;
            }

            var newDataContext = new PackageExpertViewModel(
                val,
                obj.ipcManager,
                obj.appxPackageManagerProxy,
                obj.selfElevationProxy,
                obj.signManager,
                obj.interactionService,
                obj.dialogService);

            await newDataContext.Load().ConfigureAwait(true);
            obj.DataContext = newDataContext;
        }
    }
}
