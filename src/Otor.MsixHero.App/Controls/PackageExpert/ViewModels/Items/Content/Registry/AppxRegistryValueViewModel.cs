using Otor.MsixHero.Appx.Packaging.Registry;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Content.Registry
{
    public class AppxRegistryValueViewModel : TreeNodeViewModel
    {
        public AppxRegistryValueViewModel(AppxRegistryValue value)
        {
            this.Name = value.Name;
            this.Path = value.Path;
            
            if (value.Type != "RegNone")
            {
                this.Type = value.Type;
                this.Data = value.Data;
            }
        }
        
        public string Type { get; }
        
        public string Data { get; }
    }
}
