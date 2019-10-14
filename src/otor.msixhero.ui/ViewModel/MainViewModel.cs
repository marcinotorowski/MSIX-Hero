using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using otor.msihero.lib;

namespace MSI_Hero.ViewModel
{
    public class MainViewModel : NotifyPropertyChanged
    {
        public MainViewModel(AppxPackageManager packageManager)
        {
            this.PackageList = new PackageListViewModel(packageManager);
            this.Tools = new ObservableCollection<ToolViewModel>();
            this.Tools.Add(new ToolViewModel("notepad.exe"));
            this.Tools.Add(new ToolViewModel("regedit.exe"));
            this.Tools.Add(new ToolViewModel("powershell.exe"));

#pragma warning disable 4014
            this.PackageList.RefreshPackages();
#pragma warning restore 4014
        }

        public MainViewModel() : this(new AppxPackageManager())
        {
        }

        public PackageListViewModel PackageList { get; }

        public CommandHandler CommandHandler => new CommandHandler(this.PackageList);

        public ObservableCollection<ToolViewModel> Tools { get; }
    }
}
