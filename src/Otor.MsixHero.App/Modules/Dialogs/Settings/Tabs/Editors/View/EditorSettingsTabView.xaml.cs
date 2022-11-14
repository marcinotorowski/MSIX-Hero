using System.ComponentModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Editors.ViewModel;
using Prism.Common;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Editors.View
{
    public partial class EditorSettingsTabView
    {
        private readonly ObservableObject<object> _context;

        public EditorSettingsTabView()
        {
            this.InitializeComponent();
            this._context = RegionContext.GetObservableContext(this);
            this._context.PropertyChanged += this.ContextOnPropertyChanged;
        }

        private void ContextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var dataContext = (EditorSettingsTabViewModel)this.DataContext;
            if (this._context.Value is ISettingsContext context)
            {
                dataContext.Register(context);
            }
        }
    }
}
