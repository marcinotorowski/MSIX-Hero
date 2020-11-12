using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Controls.PackageExpert
{
    public class ToolItem
    {
        public ToolItem(ToolListConfiguration model)
        {
            this.Icon = ShellIcon.GetIconFor(string.IsNullOrEmpty(model.Icon) ? model.Path : model.Icon);
            this.Model = model;
            this.Header = model.Name;
            this.Description = model.Path == null ? null : model.Path;
            this.IsUac = model.AsAdmin;
        }

        public string Header { get; }

        public string Description { get; }

        public bool IsUac { get; }

        public ImageSource Icon { get; }

        public ToolListConfiguration Model { get; }
    }
}