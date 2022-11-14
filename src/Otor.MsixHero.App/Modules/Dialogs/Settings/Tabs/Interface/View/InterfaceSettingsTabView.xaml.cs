using System.ComponentModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Interface.ViewModel;
using Prism.Common;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Interface.View
{
    public partial class InterfaceSettingsTabView
    {
        private readonly ObservableObject<object> _context;

        public InterfaceSettingsTabView()
        {
            this.InitializeComponent();
            this._context = RegionContext.GetObservableContext(this);
            this._context.PropertyChanged += this.ContextOnPropertyChanged;
        }

        private void ContextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var dataContext = (InterfaceSettingsTabViewModel)this.DataContext;
            if (this._context.Value is ISettingsContext context)
            {
                dataContext.Register(context);
            }
        }
    }
}
