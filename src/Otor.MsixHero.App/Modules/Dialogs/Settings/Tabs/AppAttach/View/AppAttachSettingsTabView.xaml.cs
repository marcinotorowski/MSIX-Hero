using System.ComponentModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Prism.Common;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.AppAttach.View
{
    public partial class AppAttachSettingsTabView
    {
        private readonly ObservableObject<object> _context;

        public AppAttachSettingsTabView()
        {
            this.InitializeComponent();
            this._context = RegionContext.GetObservableContext(this);
            this._context.PropertyChanged += ContextOnPropertyChanged;
        }

        private void ContextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var context = (SettingsContext)this._context.Value;

            if (this.DataContext is ISettingsComponent dataContext)
            {
                context.Register(dataContext);
            }
        }
    }
}
