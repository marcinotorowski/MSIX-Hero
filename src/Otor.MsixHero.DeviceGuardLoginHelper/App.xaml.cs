using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Otor.MsixHero.Appx.Signing.DeviceGuard;

namespace Otor.MsixHero.DeviceGuardLoginHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var dgss = new DgssTokenCreator();
            dgss.SignIn(CancellationToken.None);
        }
    }


}
