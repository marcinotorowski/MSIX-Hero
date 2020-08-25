using System.Windows.Media;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Ui.Helpers;

namespace Otor.MsixHero.Ui.ViewModel
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
