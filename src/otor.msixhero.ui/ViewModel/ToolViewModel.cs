using System.Windows.Media;
using System.Windows.Media.Imaging;
using otor.msixhero.lib.Infrastructure.Helpers;

namespace otor.msixhero.ui.ViewModel
{
    public class ToolViewModel : NotifyPropertyChanged
    {
        public ToolViewModel(string name, string path)
        {
            this.Path = path;
            this.Name = name;
        }

        public string Path { get; }

        public string Name { get; }

        public ImageSource Image => ShellIcon.GetIconFor(this.Path);
    }
}
