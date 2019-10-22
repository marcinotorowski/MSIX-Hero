using System;
using System.Collections.Generic;
using System.Text;

namespace MSI_Hero.Domain.Actions
{
    public class SetPackageSidebarVisibility : IAction
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
