using System.Windows.Input;
using Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.View
{
    public partial class UpdateImpactDialogContentView
    {
        public UpdateImpactDialogContentView()
        {
            InitializeComponent();
        }

        private void Header1Clicked(object sender, MouseButtonEventArgs e)
        {
            var dataContext = (UpdateImpactViewModel)this.DataContext;
            var disableClicks = dataContext.DisableAutoClicks;
            dataContext.DisableAutoClicks = true;
            try
            {
                var current = dataContext.Path1.CurrentValue;
                dataContext.Path1.Browse.Execute(false);
                if (current == dataContext.Path1.CurrentValue)
                {
                    return;
                }

                dataContext.Compare.Execute(null);
            }
            finally
            {
                dataContext.DisableAutoClicks = disableClicks;
            }
        }

        private void Header2Clicked(object sender, MouseButtonEventArgs e)
        {
            var dataContext = (UpdateImpactViewModel)this.DataContext;
            var disableClicks = dataContext.DisableAutoClicks;
            dataContext.DisableAutoClicks = true;
            try
            {
                var current = dataContext.Path2.CurrentValue;
                dataContext.Path2.Browse.Execute(false);
                if (current == dataContext.Path1.CurrentValue)
                {
                    return;
                }

                dataContext.Compare.Execute(null);
            }
            finally
            {
                dataContext.DisableAutoClicks = disableClicks;
            }
        }
    }
}
