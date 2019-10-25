using System;

namespace otor.msixhero.lib.BusinessLayer.Actions
{
    [Serializable]
    public class SetPackageSidebarVisibility : BaseAction
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
