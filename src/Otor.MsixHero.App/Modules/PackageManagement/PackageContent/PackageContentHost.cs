using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Common;
using Prism.Events;
using Prism.Navigation.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent
{
    internal class PackageContentHost : Control
    {
        static PackageContentHost()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PackageContentHost), new FrameworkPropertyMetadata(typeof(PackageContentHost)));
        }
        
        private readonly ObservableObject<object> _context;
        
        public PackageContentHost(IEventAggregator eventAggregator,
            IInteractionService interactionService,
            IUacElevation uacElevation,
            IAppxPackageQueryService packageQueryService,
            IConfigurationService configurationService,
            PrismServices prismServices, 
            IAppxFileViewer fileViewer, 
            FileInvoker fileInvoker)
        {
            this.DataContext = new PackageContentViewModel(interactionService, configurationService, packageQueryService, eventAggregator, uacElevation, prismServices, fileViewer, fileInvoker);
            this._context = RegionContext.GetObservableContext(this);
            this._context.PropertyChanged += this.OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
#pragma warning disable CS4014
            ((PackageContentViewModel)this.DataContext).LoadPackage(this._context.Value, CancellationToken.None);
#pragma warning restore CS4014
        }
    }
}
