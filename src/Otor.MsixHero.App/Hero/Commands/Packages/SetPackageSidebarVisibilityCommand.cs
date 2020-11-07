using System;
using Otor.MsixHero.App.Hero.Commands.Base;

namespace Otor.MsixHero.App.Hero.Commands.Packages
{
    [Serializable]
    public class SetPackageSidebarVisibilityCommand : UiCommand
    {
        public SetPackageSidebarVisibilityCommand() : this(true)
        {
        }

        public SetPackageSidebarVisibilityCommand(bool isVisible)
        {
            IsVisible = isVisible;
        }

        public bool IsVisible { get; set; }
    }
}
