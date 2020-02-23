using System;

namespace otor.msixhero.lib.Domain.Commands.Packages.Grid
{
    [Serializable]
    public class SetPackageSidebarVisibility : BaseCommand
    {
        public SetPackageSidebarVisibility() : this(true)
        {
        }

        public SetPackageSidebarVisibility(bool isVisible)
        {
            IsVisible = isVisible;
        }

        public bool IsVisible { get; set; }
    }
}
