using System.Windows.Media;
using System.Windows.Media.Imaging;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Helpers;

namespace otor.msixhero.ui.ViewModel
{
    public class ToolViewModel : NotifyPropertyChanged
    {
        public ToolViewModel(string name, ToolListConfiguration config)
        {
            this.Config = config;
            this.Path = config.Path;
            this.Icon = config.Icon;
            this.Name = name;
        }

        public string Path { get; }

        public string Icon { get; }

        public string Name { get; }

        public ToolListConfiguration Config { get; }

        public ImageSource Image => ShellIcon.GetIconFor(string.IsNullOrEmpty(this.Icon) ? this.Path : this.Icon);
    }
}
