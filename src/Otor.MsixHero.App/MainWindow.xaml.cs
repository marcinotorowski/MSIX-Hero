using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using Otor.MsixHero.App.Modules;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IModuleManager moduleManager;
        private readonly IDialogService dialogService;

        public MainWindow(IModuleManager moduleManager, IDialogService dialogService)
        {
            this.moduleManager = moduleManager;
            this.dialogService = dialogService;
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            WindowChrome.GetWindowChrome(this).CaptionHeight = 55;
        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Help);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.About);
        }
    }
}
