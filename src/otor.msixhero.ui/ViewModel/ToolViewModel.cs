using System.Windows.Media;
using System.Windows.Media.Imaging;
using otor.msixhero.lib.Infrastructure.Helpers;

namespace otor.msixhero.ui.ViewModel
{
    public class ToolViewModel : NotifyPropertyChanged
    {
        public ToolViewModel(string name, string path, string icon = null)
        {
            this.Path = path;
            Icon = icon;
            this.Name = name;
        }

        public string Path { get; }

        public string Icon { get; }

        public string Name { get; }

        public ImageSource Image => ShellIcon.GetIconFor(string.IsNullOrEmpty(this.Icon) ? this.Path : this.Icon);
    }
}
