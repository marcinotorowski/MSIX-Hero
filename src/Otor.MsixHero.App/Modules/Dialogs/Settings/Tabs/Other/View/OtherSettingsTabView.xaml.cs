using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Infrastructure.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Dapplo.Log;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Infrastructure.Logging;
using Prism.Common;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Other.View
{
    public partial class OtherSettingsTabView
    {
        private static readonly LogSource Logger = new();
        private readonly IAppxPackageRunService _packageRunService;
        private readonly ObservableObject<object> _context;

        public OtherSettingsTabView(IAppxPackageRunService packageRunService)
        {
            this.InitializeComponent();
            this._packageRunService = packageRunService;

            this._context = RegionContext.GetObservableContext(this);
            this._context.PropertyChanged += this.ContextOnPropertyChanged;
        }

        private void ContextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var context = (SettingsContext)this._context.Value;

            if (this.DataContext is ISettingsComponent dataContext)
            {
                context.Register(dataContext);
            }
        }

        private async void OpenLogsClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                ((Hyperlink)sender).IsEnabled = false;
                this.LogsLoading.Visibility = Visibility.Visible;

                await Task.Delay(30).ConfigureAwait(true);

                var logFile = LogManager.LogFile;
                if (logFile == null)
                {
                    return;
                }

                var familyName = PackageIdentity.FromCurrentProcess()?.GetFamilyName();
                if (string.IsNullOrEmpty(familyName))
                {
                    Logger.Info().WriteLine($"Opening log file {logFile} in notepad.exe...");

                    ExceptionGuard.Guard(() =>
                    {
                        var psi = new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            FileName = "notepad.exe",
                            Arguments = "\"" + logFile + "\""
                        };

                        Process.Start(psi);
                    });
                }
                else
                {
                    var notepadPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
                    Logger.Info().WriteLine($"Opening log file {logFile} in '{notepadPath}' (inside MSIX container)...");
                    await this._packageRunService.RunToolInContext(familyName, "MSIXHero", notepadPath, logFile).ConfigureAwait(true);

                }
            }
            finally
            {
                this.LogsLoading.Visibility = Visibility.Collapsed;
                ((Hyperlink)sender).IsEnabled = true;
            }
        }
    }
}
