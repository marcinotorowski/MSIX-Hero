using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Otor.MsixHero.App.Controls.PackageExpert.ViewModels;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Common;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Controls.PackageExpert
{
    public class PackageExpertControl : Control
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PackageExpertControl));

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(PackageExpertControl), new PropertyMetadata(null, OnFilePathChanged));
        
        private static readonly DependencyPropertyKey PackagePropertyKey = DependencyProperty.RegisterReadOnly("Package", typeof(PackageExpertViewModel), typeof(PackageExpertControl), new PropertyMetadata(null));
        public static readonly DependencyProperty PackageProperty = PackagePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ErrorMessagePropertyKey = DependencyProperty.RegisterReadOnly("ErrorMessage", typeof(string), typeof(PackageExpertControl), new PropertyMetadata(null));
        public static readonly DependencyProperty ErrorMessageProperty = ErrorMessagePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsLoadingPropertyKey = DependencyProperty.RegisterReadOnly("IsLoading", typeof(string), typeof(PackageExpertControl), new PropertyMetadata(null));
        public static readonly DependencyProperty IsLoadingProperty = IsLoadingPropertyKey.DependencyProperty;

        static PackageExpertControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PackageExpertControl), new FrameworkPropertyMetadata(typeof(PackageExpertControl)));
        }

        private readonly IInterProcessCommunicationManager ipcManager;

        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;

        private readonly IRunningDetector runningDetector;

        private readonly IInteractionService interactionService;

        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerProvider;

        private readonly IDialogService dialogService;

        private readonly ObservableObject<object> context;

        public PackageExpertControl(
            IInterProcessCommunicationManager ipcManager,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            IRunningDetector runningDetector,
            IInteractionService interactionService,
            ISelfElevationProxyProvider<ISigningManager> signingManagerProvider,
            IDialogService dialogService
        )
        {
            this.context = RegionContext.GetObservableContext(this);
            this.context.PropertyChanged += this.OnPropertyChanged;
            this.ipcManager = ipcManager;
            this.packageManagerProvider = packageManagerProvider;
            this.runningDetector = runningDetector;
            this.interactionService = interactionService;
            this.signingManagerProvider = signingManagerProvider;
            this.dialogService = dialogService;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.FilePath = (string) this.context.Value;
        }

        public PackageExpertViewModel Package
        {
            get => (PackageExpertViewModel)GetValue(PackageProperty);
            private set => SetValue(PackagePropertyKey, value);
        }

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            private set => SetValue(ErrorMessagePropertyKey, value);
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            private set => SetValue(IsLoadingPropertyKey, value);
        }

        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        private static async void OnFilePathChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var sender = (PackageExpertControl)dependencyObject;
            var newFilePath = (string) eventArgs.NewValue;

            if (newFilePath == null)
            {
                sender.Package = null;
                return;
            }

            var newDataContext = new PackageExpertViewModel(
                newFilePath,
                sender.ipcManager,
                sender.packageManagerProvider,
                sender.signingManagerProvider,
                sender.interactionService,
                sender.runningDetector,
                sender.dialogService);

            try
            {
                await newDataContext.Load().ConfigureAwait(true);
                sender.Package = newDataContext;
                sender.ErrorMessage = null;
            }
            catch (Exception exception)
            {
                sender.Package = null;
                sender.ErrorMessage = "Could not load details. " + exception.Message;
                Logger.Warn($"Could not load details of package '{newFilePath}'.");
            }
        }
    }
}
